using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Qkmaxware.Media.Image.Metadata;

namespace Qkmaxware.Media.Image;

public class TiffDecoderException : System.Exception {
    public TiffDecoderException(string message) : base(message) {}
}

public class TiffEncoderException : System.Exception {
    public TiffEncoderException(string message) : base(message) {}
}

/// <summary>
/// Field data type used by TIFF images
/// </summary>
internal enum TiffFieldType : uint {
    Byte = 1,           // 8 bit unsigned int
    ASCII = 2,          // 8 bit ascii code
    Short = 3,          // 16 bit unsigned int
    Long = 4,           // 32 bit unsigned int
    Rational = 5,       // 2 Longs; first is numerator, second denominator
    SignedByte = 6,     // 8 bit signed int
    Undefined = 7,      // Any binary data
    SignedShort = 8,    // 16 bit signed int
    SignedLong = 9,     // 32 bit signed int
    SignedRational = 10,// 2 signed Longs; first is numerator, second denominator
    Float = 11,         // 32 bit IEEE floating point number
    Double = 12         // 64 bit IEEE floating point number
}

/// <summary>
/// Baseline TIFF image format
/// </summary>
public class TagImageFileFormat : IBinaryImageLoader, IBinaryImageEncoder {

    private static byte readU8(ByteIterator reader) => reader.ReadU8();
    private static char readAscii(ByteIterator reader) => Convert.ToChar(reader.ReadU8());
    private static uint readU16(ByteIterator reader) => reader.ReadU16();
    private static uint readU32(ByteIterator reader) => reader.ReadU32();
    private static UnsignedRational readURational(ByteIterator reader) {
        var num = readU32(reader);
        var dem = readU32(reader);
        return new UnsignedRational(num, dem);
    }
    private static int readS8(ByteIterator reader) => reader.ReadS8();
    private static int readS16(ByteIterator reader) => reader.ReadS16();
    private static int readS32(ByteIterator reader) => reader.ReadS32();
    private static SignedRational readSRational(ByteIterator reader) {
        var num = readS32(reader);
        var dem = readS32(reader);
        return new SignedRational(num, dem);
    }
    private static float readIEEEFloat(ByteIterator reader) => reader.ReadIEEEFloat();
    private static double readIEEEDouble(ByteIterator reader) => reader.ReadIEEEDouble();

    /// <summary>
    /// Create a new Tag Image File Format 
    /// </summary>
    public TagImageFileFormat() : base() {}

    public IImage Load(string path) {
        using (var fs = new FileStream(path, FileMode.Open)) 
        using (var reader = new BinaryReader(fs)) {
            return this.LoadFrom(reader);
        }
    }
    public IImage LoadFrom(BinaryReader reader) {
        // Tiff files start with an 8 byte header
        // Bytes 1-2 declare endian order
        var order = reader.ReadInt16() switch {
            0x4949 => ByteOrder.LittleEndian,
            0x4D4D => ByteOrder.BigEndian,
            _ => throw new TiffDecoderException("Cannot read TIFF file byte encoding. The file may not be a TIFF file"),
        };
        ByteIterator byteReader = new ByteIterator(order, reader);
        // Bytes 2-3 
        var TIFF = readU16(byteReader);
        if (TIFF != 42) {
            throw new TiffDecoderException("The file does not contain TIFF identification markings");
        }
        // bytes 4-7
        var IFDptr = readU32(byteReader);
        if (IFDptr == 0) {
            throw new TiffDecoderException("TIFF file does not contain any IFDs");
        }

        // Read first IFD (BASELINE TIFF NOT REQUIRED TO READ MORE THAN 1 IFD)
        reader.BaseStream.Seek(IFDptr, SeekOrigin.Begin);
        var IFDsize = readU16(byteReader);
        if (IFDsize < 1) {
            throw new TiffDecoderException("TIFF IFD must contain at least 1 entry");
        }

        // Read all IFD entries
        TiffMetadata metadata = new TiffMetadata();
        for (var i = 0; i < IFDsize; i++) {
            var tag = readU16(byteReader);                    // The tag that identifies the field
            var typeVal = readU16(byteReader);                // The field type
            var count = readU32(byteReader);                  // The number of values of the indicated type
            var offset = readU32(byteReader);                 // The file offset of the value of the field

            if (Enum.IsDefined(typeof(TiffFieldType), typeVal)) {
                //var fitsIn4Bytes = TiffFieldType.ByteSize() * count;
                metadata.Add(new TiffField { 
                    TagId = tag,
                    DataType = (TiffFieldType)typeVal, 
                    Length = count, 
                    ValueOffset = offset // If this is smaller than 4 bytes it is a value? Otherwise its an offset?
                });
            } else {
                // Unrecognized type, skip over this field
                continue;
            }
        }
        
        // Read next IFD offset
        var nextIFDptr = readU32(byteReader); // For potential future work (baseTiff doesn't need this as we only are required to read 1 image)
        
        // Read in all the data arrays for each IFD entry
        foreach (var meta in metadata) {
            if (meta.ValueOffset < 0xFFFFFFFF && meta.Length == 1) {
                // Offset is value, interpret it as such
                meta.Values = new object[1] { 
                    meta.DataType switch {
                        TiffFieldType.Byte              => (byte)meta.ValueOffset,
                        TiffFieldType.ASCII             => System.Text.Encoding.UTF8.GetString(BitConverter.GetBytes(meta.ValueOffset)),
                        TiffFieldType.Short             => (ushort)meta.ValueOffset,
                        TiffFieldType.Long              => (uint)meta.ValueOffset,
                        TiffFieldType.Rational          => new UnsignedRational((uint)meta.ValueOffset, 1),
                        TiffFieldType.SignedByte        => (sbyte)meta.ValueOffset,
                        TiffFieldType.Undefined         => (byte)meta.ValueOffset,
                        TiffFieldType.SignedShort       => (short)meta.ValueOffset,
                        TiffFieldType.SignedLong        => (int)meta.ValueOffset,
                        TiffFieldType.SignedRational    => new SignedRational((int)meta.ValueOffset, 1),
                        TiffFieldType.Float             => BitConverter.ToSingle(BitConverter.GetBytes(meta.ValueOffset), 0),
                        TiffFieldType.Double            => BitConverter.ToDouble(BitConverter.GetBytes(meta.ValueOffset), 0),
                        _ => meta.ValueOffset
                    }
                };
                continue;
            }
            // Offset is an actual position in the file, location of the data
            reader.BaseStream.Seek(meta.ValueOffset, SeekOrigin.Begin);
            // Read all values based on the type
            var data = new object[meta.Length];
            for (var i = 0; i < data.Length; i++) {
                data[i] = meta.DataType switch {
                    TiffFieldType.Byte              => readU8(byteReader),
                    TiffFieldType.ASCII             => readAscii(byteReader),
                    TiffFieldType.Short             => readU16(byteReader),
                    TiffFieldType.Long              => readU32(byteReader),
                    TiffFieldType.Rational          => readURational(byteReader),
                    TiffFieldType.SignedByte        => readS8(byteReader),
                    TiffFieldType.Undefined         => readU8(byteReader),
                    TiffFieldType.SignedShort       => readS16(byteReader),
                    TiffFieldType.SignedLong        => readS32(byteReader),
                    TiffFieldType.SignedRational    => readSRational(byteReader),
                    TiffFieldType.Float             => readIEEEFloat(byteReader),
                    TiffFieldType.Double            => readIEEEDouble(byteReader),
                    _ => 0
                };
            }
            meta.Values = data;
        }

        // Final checks
         if (!metadata.Contains(ExifTag.PhotometricInterpretation))
            throw new TiffDecoderException("Missing image photometric interpretation metadata");
        if (!metadata.Contains(ExifTag.ImageWidth))
            throw new TiffDecoderException("Missing image width metadata");
        if (!metadata.Contains(ExifTag.ImageHeight))
            throw new TiffDecoderException("Missing image length metadata");
        if (!metadata.Contains(ExifTag.RowsPerStrip))
            throw new TiffDecoderException("Missing rows per strip metadata");
        if (!metadata.Contains(ExifTag.StripByteCounts))
            throw new TiffDecoderException("Missing strip byte counts metadata");
        if (!metadata.Contains(ExifTag.StripOffsets))
            throw new TiffDecoderException("Missing strip offsets metadata");

        // Obtain important metadata for further steps
        var samples = (metadata.Contains(ExifTag.BitsPerSample) ? metadata[ExifTag.BitsPerSample].Values : new object[]{ 8 }).Select(x => Convert.ToInt32(x)).ToArray();
        var maxSampleDepth = samples.Max();
        var type = getType(metadata);
        var compression = getCompression(metadata);
        var numSamplesPerPixel = samples.Length;
        bool interleaveSamples = metadata.Contains(ExifTag.PlanarConfiguration) ? Convert.ToInt32(metadata[ExifTag.PlanarConfiguration].Values[0]) == 1 : true /*default true*/; // 2='Planar' for per channel and 1='Chunky' for interleaved;
        var photoMode = metadata[ExifTag.PhotometricInterpretation];
        bool useColourPalette = photoMode.Values != null && photoMode.Values.Length > 0 && Convert.ToInt32(photoMode.Values[0]) == 3 && metadata.Contains(ExifTag.ColorMap);

        // Create the image
        var img = new MemoryImage(
            maxSampleDepth switch {
                8 => SampleDepth.Bit8,
                16 => SampleDepth.Bit16,
                32 => SampleDepth.Bit32,
                _ => throw new TiffDecoderException($"Images with {maxSampleDepth}bit samples are not currently supported")
            },
            createPixels(metadata, useColourPalette, numSamplesPerPixel)
        );

        // Read image data from strips (uncompress if required)
        ByteIterator? imageBits = null;
        {
            var offsetField = metadata[ExifTag.StripOffsets]; 
            var bytesField = metadata[ExifTag.StripByteCounts];
            var rowsPerStripField = metadata[ExifTag.RowsPerStrip];

            var stripCount = offsetField.Length;
            var strips = new List<byte[]>((int)stripCount);
            for (var i = 0; i < stripCount; i++) {
                var offsets = offsetField.Values;
                var stripOffset = (long)Convert.ToInt64(offsetField.Values[i]);
                var stripBytes = (long)Convert.ToInt64(bytesField.Values[i]);
                
                // Read in bytes
                reader.BaseStream.Seek(stripOffset, SeekOrigin.Begin);
                byte[] bytes = new byte[stripBytes];
                for (var b = 0; b < stripBytes; b++) {
                    bytes[b] = readU8(byteReader);
                }
                strips.Add(bytes);
            }
            // Decode strips as pixel bytes
            switch (compression) {
                case TiffCompression.NoCompression:
                    imageBits = decodeDataNoCompression(order, img.Width, img.Height, Convert.ToInt32(rowsPerStripField.Values[0]), strips);
                    break;
                case TiffCompression.CCITT:
                    throw new TiffDecoderException("CCITT compressed images are not currently supported");
                    //break;
                case TiffCompression.PackBits:
                    throw new TiffDecoderException("Pack Bits compressed images are not currently supported");
                    //break;
            }
        }
        
        // Convert the decoded bytes to pixels
        if (imageBits != null) {     
            // COLOUR PALETTE BASED TIFF IMAGES (always PER CHANNEL, use colour palatte instead of byte array) 
            if (useColourPalette) {
                var palatte = metadata[ExifTag.ColorMap];
                var depth = samples[0];
                for (var row = 0L; row < img.Pixels.GetLongLength(0); row++) {
                    for (var col = 0L; col < img.Pixels.GetLongLength(1); col++) {
                        var mapIndex = depth switch {
                            8 => imageBits.ReadU8(),
                            16 => imageBits.ReadU16(),
                            32 => imageBits.ReadU32(),
                            _ => throw new TiffDecoderException($"Images with {depth}bit samples are not currently supported")
                        };

                        // Copy sample value to pixel
                        var pixel = img.Pixels[row, col];
                        if (pixel.Samples != null) {
                            pixel.Samples[0] = Convert.ToUInt32(palatte.Values[0 * img.Pixels.Length + mapIndex]);
                            pixel.Samples[1] = Convert.ToUInt32(palatte.Values[1 * img.Pixels.Length + mapIndex]);
                            pixel.Samples[2] = Convert.ToUInt32(palatte.Values[2 * img.Pixels.Length + mapIndex]);
                        }
                    }
                }
            } 
            // DENSE PIXEL BASED TIFF IMAGES (RGB etc)
            else {
                if (interleaveSamples) {
                    // INTERLEAVED RGB RGB
                    for (var row = 0L; row < img.Pixels.GetLongLength(0); row++) {
                        for (var col = 0L; col < img.Pixels.GetLongLength(1); col++) {
                            for (var sampleIndex = 0; sampleIndex < numSamplesPerPixel; sampleIndex++) {
                                // Sample depth
                                var depth = samples[sampleIndex]; // Common values are 4 (max 16), 8 (1 byte max 255), 16 (2 bytes max 65 536), 32 (4 bytes max 4 294 967 296)

                                // Read sample
                                uint sample = depth switch {
                                    8 => imageBits.ReadU8(),
                                    16 => imageBits.ReadU16(),
                                    32 => imageBits.ReadU32(),
                                    _ => throw new TiffDecoderException($"Images with {depth}bit samples are not currently supported")
                                };

                                // Copy sample value to pixel
                                var pixel = img.Pixels[row, col];
                                if (pixel.Samples != null)
                                    pixel.Samples[sampleIndex] = sample;
                            }
                        }
                    }
                } else {
                    // PER CHANNEL RR GG BB
                    // All red first, then all green, then all blue etc...
                    for (var sampleIndex = 0; sampleIndex < numSamplesPerPixel; sampleIndex++) {
                        // Sample depth
                        var depth = samples[sampleIndex]; // Common values are 4 (max 16), 8 (1 byte max 255), 16 (2 bytes max 65 536), 32 (4 bytes max 4 294 967 296)

                        for (var row = 0L; row < img.Pixels.GetLongLength(0); row++) {
                            for (var col = 0L; col < img.Pixels.GetLongLength(1); col++) {
                                // Read sample
                                uint sample = depth switch {
                                    8 => imageBits.ReadU8(),
                                    16 => imageBits.ReadU16(),
                                    32 => imageBits.ReadU32(),
                                    _ => throw new TiffDecoderException($"Images with {depth}bit samples are not currently supported")
                                };

                                // Copy sample value to pixel
                                var pixel = img.Pixels[row, col];
                                if (pixel.Samples != null)
                                    pixel.Samples[sampleIndex] = sample;
                            }
                        }
                    }
                }
            }
        }

        // Convert from extracted TIFF metadata to image metadata
        convertTiffMetadataToMediaMetadata(img, metadata);

        // Return the image
        return img;
    }

    private static ByteIterator decodeDataNoCompression(ByteOrder order, int width, int height, int rowsPerStrip, List<byte[]> strips) {
        // Flatten strips
        List<byte> bytes = new List<byte>();
        foreach (var strip in strips) {
            bytes.AddRange(strip);
        }
        return new ByteIterator(order, bytes.ToArray());
    }

    private TiffImageType getType(TiffMetadata metadata) {
        var tag = ExifTag.PhotometricInterpretation;
        if (!metadata.Contains(tag))
            return TiffImageType.WhiteIsZero;
        
        var field = metadata[tag];
        if (field.Values == null || field.Values.Length < 1) {
            return TiffImageType.WhiteIsZero;
        }

        var ivalue = (int)Convert.ToInt32(field.Values[0]);
        if (Enum.IsDefined(typeof(TiffImageType), ivalue)) {
            return (TiffImageType)ivalue;
        } else {
            return TiffImageType.WhiteIsZero;
        }
    }

    private TiffCompression getCompression(TiffMetadata metadata) {
        var tag = ExifTag.Compression;
        if (!metadata.Contains(tag))
            return TiffCompression.NoCompression;
        
        var field = metadata[tag];
        if (field.Values == null || field.Values.Length < 1) {
            return TiffCompression.NoCompression;
        }

        var ivalue = (int)Convert.ToInt32(field.Values[0]);
        if (Enum.IsDefined(typeof(TiffCompression), ivalue)) {
            return (TiffCompression)ivalue;
        } else {
            return TiffCompression.NoCompression;
        }   
    }

    private Pixel[,] createPixels(TiffMetadata metadata, bool isUsingColourPalette, int samples) {
        var Width = Convert.ToInt32(metadata[ExifTag.ImageWidth].Values[0]);
        var Height = Convert.ToInt32(metadata[ExifTag.ImageHeight].Values[0]);

        var Pixels = new Pixel[Height,Width];
        for (var i = 0L; i < Pixels.GetLongLength(0); i++) {
            for (var j = 0L; j < Pixels.GetLongLength(1); j++) {
                Pixels[i,j] = new Pixel(isUsingColourPalette ? 3 : samples);
            }
        }

        return Pixels;
    }

    private static void convertTiffMetadataToMediaMetadata(IImage img, TiffMetadata metadata) {
        foreach (var meta in metadata) {
            var id = meta.TagId;
            var tag = (Enum.IsDefined(typeof(ExifTag), id)) ? (ExifTag?)id : null;
            var name = tag?.ToStringWithSpaces() ?? string.Empty;
            img.Metadata.AddField(meta.DataType switch {
                /*
                Byte = 1,           // 8 bit unsigned int
                ASCII = 2,          // 8 bit ascii code
                Short = 3,          // 16 bit unsigned int
                Long = 4,           // 32 bit unsigned int
                Rational = 5,       // 2 Longs; first is numerator, second denominator
                SignedByte = 6,     // 8 bit signed int
                Undefined = 7,      // Any binary data
                SignedShort = 8,    // 16 bit signed int
                SignedLong = 9,     // 32 bit signed int
                SignedRational = 10,// 2 signed Longs; first is numerator, second denominator
                Float = 11,         // 32 bit IEEE floating point number
                Double = 12         // 64 bit IEEE floating point number
                */
                TiffFieldType.Byte              => meta.Values.Length > 1 ? new Field<byte[]>(name, tag, meta.Values.Select(x => Convert.ToByte(x)).ToArray()) : new Field<byte>(name, tag, Convert.ToByte(meta.Values[0])),
                TiffFieldType.ASCII             => new Field<string>(name, tag, System.Text.Encoding.UTF8.GetString(meta.Values.Select(x => Convert.ToByte(x)).ToArray())),
                TiffFieldType.Short             => meta.Values.Length > 1 ? new Field<ushort[]>(name, tag, meta.Values.Select(x => Convert.ToUInt16(x)).ToArray()) : new Field<ushort>(name, tag, Convert.ToUInt16(meta.Values[0])),
                TiffFieldType.Long              => meta.Values.Length > 1 ? new Field<uint[]>(name, tag, meta.Values.Select(x => Convert.ToUInt32(x)).ToArray()) : new Field<uint>(name, tag, Convert.ToUInt32(meta.Values[0])),
                TiffFieldType.Rational          => meta.Values.Length > 1 ? new Field<UnsignedRational[]>(name, tag, meta.Values.Select(x => (UnsignedRational)x).ToArray()) : new Field<UnsignedRational>(name, tag, (UnsignedRational)meta.Values[0]),
                TiffFieldType.SignedByte        => meta.Values.Length > 1 ? new Field<sbyte[]>(name, tag, meta.Values.Select(x => Convert.ToSByte(x)).ToArray()) : new Field<sbyte>(name, tag, Convert.ToSByte(meta.Values[0])),
                TiffFieldType.SignedShort       => meta.Values.Length > 1 ? new Field<short[]>(name, tag, meta.Values.Select(x => Convert.ToInt16(x)).ToArray()) : new Field<short>(name, tag, Convert.ToInt16(meta.Values[0])),
                TiffFieldType.SignedLong        => meta.Values.Length > 1 ? new Field<int[]>(name, tag, meta.Values.Select(x => Convert.ToInt32(x)).ToArray()) : new Field<int>(name, tag, Convert.ToInt32(meta.Values[0])),
                TiffFieldType.SignedRational    => meta.Values.Length > 1 ? new Field<SignedRational[]>(name, tag, meta.Values.Select(x => (SignedRational)x).ToArray()) : new Field<SignedRational>(name, tag, (SignedRational)meta.Values[0]),
                TiffFieldType.Float             => meta.Values.Length > 1 ? new Field<float[]>(name, tag, meta.Values.Select(x => Convert.ToSingle(x)).ToArray()) : new Field<float>(name, tag, Convert.ToSingle(meta.Values[0])),
                TiffFieldType.Double            => meta.Values.Length > 1 ? new Field<double[]>(name, tag, meta.Values.Select(x => Convert.ToDouble(x)).ToArray()) : new Field<double>(name, tag, Convert.ToDouble(meta.Values[0])),
                TiffFieldType.Undefined         => new Field(name, tag),
                _                               => new Field(name, tag)
            });
        }
    }

    public void Save(string path, IImage image) {
        if (!path.EndsWith(".tif") && !path.EndsWith(".tiff"))
            path += ".tif";
        using (var fs = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(fs)) {
            SaveTo(writer, image);
        }
    }

    public void SaveTo(BinaryWriter writer, IImage image) {
        // Header (Tiff files start with an 8 byte header) 
        {
            // Bytes 1-2 declare endian order
            if (BitConverter.IsLittleEndian) {
                writer.Write((byte)0x49); writer.Write((byte)0x49); // Use little endian
            } else {
                writer.Write((byte)0x4D); writer.Write((byte)0x4D); // Use big endian
            }

            // Write TIFF itentifier markings
            writer.Write((ushort)42);

            // Write IFD pointer
            writer.Write((uint)8);
        }
        var headerEndPtr = 8;

        // Copy all user defined metadata
        TiffMetadata metadata = new TiffMetadata();
        foreach (var field in image.Metadata) {
            // Handle encoding of metadata
            convertMediaMetadataToTiffMetadata(metadata, field);
        }

        // Create primary IFD
        using (MemoryStream IFD = new MemoryStream()) {
            var IFDWriter = new BinaryWriter(IFD);

            // Primary IFD
            List<TiffField> fields = new List<TiffField>();
            List<long> valueFieldOffsets = new List<long>();
            var sampleSize = (int)image.SampleBitDepth;
            var samples = image.Pixels?[0,0]?.Samples?.Length ?? 0;
            if (samples < 3)
                samples = 3;
            var numBytesPerStrip = (sampleSize / 8) /* 16 bits per sample */ * samples * image.Width;
            var stripOffsetFieldIdx = 0;
            {
                // Create fields
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.ImageWidth,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = image.Width,
                });
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.ImageHeight,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = image.Height,
                });
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.Compression,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = 1, // No compression
                });
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.PhotometricInterpretation,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = 2, // FULL RGB images with at least 3 samples per pixel
                });
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.ResolutionUnit,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = 1, // No absolute unit of measurement
                });
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.XResolution,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = 1, // 1 pixel per unit
                });
                fields.Add(new TiffField{  
                    TagId = (uint)ExifTag.YResolution,
                    DataType = TiffFieldType.Short,
                    Length = 1,
                    ValueOffset = 1, // 1 pixel per unit
                });
                {
                    // Samples
                    fields.Add(new TiffField{  
                        TagId = (uint)ExifTag.SamplesPerPixel,
                        DataType = TiffFieldType.Short,
                        Length = 1,
                        ValueOffset = samples, // Number of samples as determined prior
                    });
                    var bits = new object[samples];
                    for (var i = 0; i < samples; i++) {
                        bits[i] = sampleSize; // All samples are 16bit in this export
                    }
                    fields.Add(new TiffField{  
                        TagId = (uint)ExifTag.BitsPerSample,
                        DataType = TiffFieldType.Short,
                        Length = (uint)bits.Length,
                        Values = bits,
                    });
                    if (samples > 3) {
                        fields.Add(new TiffField{
                            TagId = (uint)ExifTag.ExtraSamples,
                            DataType = TiffFieldType.Short,
                            Length = 1,
                            ValueOffset = 0
                        });
                    }
                }
                {
                    // Encoded image data
                    fields.Add(new TiffField{  
                        TagId = (uint)ExifTag.RowsPerStrip, // How many rows of pixels exist per strip
                        DataType = TiffFieldType.Short,
                        Length = 1,
                        ValueOffset = 1, // Each strip is its own row
                    });
                    stripOffsetFieldIdx = fields.Count;
                    var stripOffsetDefaults = new object[image.Height];
                    for (var i = 0; i < stripOffsetDefaults.Length; i++) {
                        stripOffsetDefaults[i] = default(uint);
                    }
                    fields.Add(new TiffField{  
                        TagId = (uint)ExifTag.StripOffsets, // Where are the strips located... I do know the number of strips though
                        DataType = TiffFieldType.Long,
                        Length = (uint)image.Height,
                        Values = stripOffsetDefaults
                    });
                    var bytesPerStrip = new object[image.Height];
                    for (var i = 0; i < bytesPerStrip.Length; i++) {
                        bytesPerStrip[i] = numBytesPerStrip; 
                    }
                    fields.Add(new TiffField{  
                        TagId = (uint)ExifTag.StripByteCounts, // How many bytes per strip, 
                        DataType = TiffFieldType.Short,
                        Length = (uint)bytesPerStrip.Length,
                        Values = bytesPerStrip,
                    });
                }

                // Write IFD
                metadata.Add(fields);
                IFDWriter.Write((ushort)metadata.Count); // Number of IFD entries
                foreach (var field in metadata) {
                    IFDWriter.Write((ushort)field.TagId);
                    IFDWriter.Write((ushort)field.DataType);
                    IFDWriter.Write((uint)field.Length);
                    valueFieldOffsets.Add(IFD.Position); // record the position of this IDF's value Offset field so we can update it later
                    IFDWriter.Write((uint)field.ValueOffset); 
                }
                IFDWriter.Write((uint)0); // No next IFD
            }
            IFDWriter.Flush();
            {
                // IFD array data
                var inMemoryOffsetPtr = 0L;
                for (var i = 0; i < fields.Count; i++) {
                    var field = fields[i];
                    var valueOffsetPtr = valueFieldOffsets[i];
                    if (field.Length > 1 && field.Values != null) {
                        // Write the array of values based on the data type
                        if (field.TagId == (uint)ExifTag.StripOffsets) {
                            inMemoryOffsetPtr = IFD.Position;
                        }
                        var dataPtr = IFD.Position + headerEndPtr; 
                        foreach (var value in field.Values) {
                            switch (field.DataType) {
                                case TiffFieldType.Byte: IFDWriter.Write(Convert.ToByte(value)); break;
                                case TiffFieldType.Short: IFDWriter.Write(Convert.ToUInt16(value)); break;
                                case TiffFieldType.Long: IFDWriter.Write(Convert.ToUInt32(value)); break;
                                case TiffFieldType.SignedShort: IFDWriter.Write(Convert.ToInt16(value)); break;
                                case TiffFieldType.SignedLong: IFDWriter.Write(Convert.ToInt32(value)); break;

                                default: IFDWriter.Write(Convert.ToByte(value)); break;
                            }
                        }
                        var donePtr = IFD.Position;

                        // Update the ValueOffset field to the current pointer
                        IFD.Position = valueOffsetPtr;
                        IFDWriter.Write((uint)dataPtr);

                        // Return to the end of the dataset in preparation for the next write
                        IFD.Position = donePtr;
                    }
                }

                // We can now determine the "start" of the strip offsets and use math to compute each strip offset
                var firstStripPosition = IFD.Position + headerEndPtr;
                var offsets = new long[image.Height];
                for (var i = 0; i < image.Height; i++) {
                    offsets[i] = firstStripPosition + i * numBytesPerStrip;
                }
                // Write these offsets to the IFD (basically what was done above, but for 1 specific field)
                IFD.Position = inMemoryOffsetPtr; // Jump back to the field and write the data ptr;
                foreach (var offset in offsets) {
                    IFDWriter.Write((uint)offset); // Replace the existing "temp" array with the one here
                }
            }
            IFDWriter.Flush();
            var IFDbytes = IFD.ToArray();
            IFDWriter.Dispose();
            foreach (var b in IFDbytes) {
                writer.Write(b);
            }

            // Image Data
            {
                for (var row = 0L; row < image.Height; row++) {
                    for (var col = 0L; col < image.Width; col++) {
                        var pixel = image.Pixels?[row, col];
                        writePixel(image.SampleBitDepth, pixel, samples, writer);
                    }
                }
            }
        }
    }

    private void writePixel(SampleDepth depth, Pixel? pixel, int samples, BinaryWriter writer) {
        for (var i = 0; i < samples; i++) {
            uint value = pixel?.Samples?.ElementAtOrDefault(i) ?? 0;

            switch (depth) {
                case SampleDepth.Bit8: writer.Write((byte)value); break;
                case SampleDepth.Bit16: writer.Write((ushort)value); break;
                case SampleDepth.Bit32: writer.Write((uint)value); break;
                default:
                    throw new TiffEncoderException($"Cannot encode images with a sample depth of {(int)depth}bits");
            }
        }
    }

    private void convertMediaMetadataToTiffMetadata(TiffMetadata metadata, Field field) {
        // Can't encode fields without an EXIF tag
        if (!field.ExifTag.HasValue)
            return; 

        var tiff = new TiffField {
            TagId = (uint)field.ExifTag.Value,
            DataType = field switch {
                Field<byte> byteField                   => TiffFieldType.Byte,           
                Field<string> stringField               => TiffFieldType.ASCII,          
                Field<ushort> ushortField               => TiffFieldType.Short,          
                Field<uint> uintField                   => TiffFieldType.Long,           
                Field<UnsignedRational> urationalField  => TiffFieldType.Rational,       
                Field<sbyte> sbyteField                 => TiffFieldType.SignedByte,     
                Field<short> shortField                 => TiffFieldType.SignedShort,    
                Field<int> intField                     => TiffFieldType.SignedLong,     
                Field<SignedRational> srationalField    => TiffFieldType.SignedRational, 
                Field<float> floatField                 => TiffFieldType.Float,          
                Field<double> doubleField               => TiffFieldType.Double,     

                Field<byte[]> byteField                 => TiffFieldType.Byte,           
                Field<ushort[]> ushortField             => TiffFieldType.Short,          
                Field<uint[]> uintField                 => TiffFieldType.Long,           
                Field<UnsignedRational[]> urationalField=> TiffFieldType.Rational,       
                Field<sbyte[]> sbyteField               => TiffFieldType.SignedByte,     
                Field<short[]> shortField               => TiffFieldType.SignedShort,    
                Field<int[]> intField                   => TiffFieldType.SignedLong,     
                Field<SignedRational[]> srationalField  => TiffFieldType.SignedRational, 
                Field<float[]> floatField               => TiffFieldType.Float,          
                Field<double[]> doubleField             => TiffFieldType.Double,   
                _                                       => TiffFieldType.Undefined,      
            },
            Length = field switch {
                Field<byte> byteField                   => 1,
                Field<string> stringField               => (uint?)stringField.Value?.Length ?? 0u,
                Field<ushort> ushortField               => 1,
                Field<uint> uintField                   => 1,
                Field<UnsignedRational> urationalField  => 1,
                Field<sbyte> sbyteField                 => 1,
                Field<short> shortField                 => 1,
                Field<int> intField                     => 1,
                Field<SignedRational> srationalField    => 1,
                Field<float> floatField                 => 1,
                Field<double> doubleField               => 1,

                Field<byte[]> byteField                 => (uint?)byteField.Value?.Length ?? 0u,
                Field<ushort[]> ushortField             => (uint?)ushortField.Value?.Length ?? 0u,
                Field<uint[]> uintField                 => (uint?)uintField.Value?.Length ?? 0u,
                Field<UnsignedRational[]> urationalField=> (uint?)urationalField.Value?.Length ?? 0u,
                Field<sbyte[]> sbyteField               => (uint?)sbyteField.Value?.Length ?? 0u,
                Field<short[]> shortField               => (uint?)shortField.Value?.Length ?? 0u,
                Field<int[]> intField                   => (uint?)intField.Value?.Length ?? 0u,
                Field<SignedRational[]> srationalField  => (uint?)srationalField.Value?.Length ?? 0u,
                Field<float[]> floatField               => (uint?)floatField.Value?.Length ?? 0u,
                Field<double[]> doubleField             => (uint?)doubleField.Value?.Length ?? 0u,
                _                                       => 0,      
            },
            Values = field switch {
                Field<string> stringField               => System.Text.Encoding.UTF8.GetBytes(stringField.Value ?? string.Empty).Select(x => (object)x).ToArray(),

                Field<byte[]> byteField                 => byteField.Value.Select(x => (object)x).ToArray(),
                Field<ushort[]> ushortField             => ushortField.Value.Select(x => (object)x).ToArray(),
                Field<uint[]> uintField                 => uintField.Value.Select(x => (object)x).ToArray(),
                Field<UnsignedRational[]> urationalField=> urationalField.Value,
                Field<sbyte[]> sbyteField               => sbyteField.Value.Select(x => (object)x).ToArray(),
                Field<short[]> shortField               => shortField.Value.Select(x => (object)x).ToArray(),
                Field<int[]> intField                   => intField.Value.Select(x => (object)x).ToArray(),
                Field<SignedRational[]> srationalField  => srationalField.Value,
                Field<float[]> floatField               => floatField.Value.Select(x => (object)x).ToArray(),
                Field<double[]> doubleField             => doubleField.Value.Select(x => (object)x).ToArray(),
                _                                       => null,      
            } ?? new object[0],
            ValueOffset = field switch {
                Field<byte> byteField                   => byteField.Value,
                Field<string> stringField               => 0,
                Field<ushort> ushortField               => ushortField.Value,
                Field<uint> uintField                   => uintField.Value,
                Field<UnsignedRational> urationalField  => 0,
                Field<sbyte> sbyteField                 => sbyteField.Value,
                Field<short> shortField                 => shortField.Value,
                Field<int> intField                     => intField.Value,
                Field<SignedRational> srationalField    => 0,
                Field<float> floatField                 => BitConverter.ToInt64(BitConverter.GetBytes(floatField.Value)),
                Field<double> doubleField               => BitConverter.ToInt64(BitConverter.GetBytes(doubleField.Value)),
                _                                       => 0,      
            }
        };
        if (tiff.DataType != TiffFieldType.Undefined)
            metadata.Add(tiff);
    }
}

internal class TiffField {
    public uint TagId {get; set;}
    public TiffFieldType DataType {get; set;}
    public uint Length {get; set;}
    public long ValueOffset {get; set;}
    public object[] Values {get; set;} = new object[0];
}

internal class TiffMetadata : IEnumerable<TiffField> {
    private Dictionary<uint, TiffField> _metadata = new Dictionary<uint, TiffField>();
    
    public int Count => _metadata.Count;

    public void Add(TiffField field) {
        _metadata[field.TagId] = field;
    }

    public void Add(IEnumerable<TiffField> fields) {
        foreach (var field in fields)
            Add(field);
    }

    public bool Contains(ExifTag tag) {
        return this.Contains((uint)tag);
    }

    public bool Contains(uint tag) {
        return _metadata.ContainsKey(tag);
    }

    public TiffField this[ExifTag tag] {
        get => this[(uint)tag];
    }

    public TiffField this[uint tag] {
        get => this._metadata[tag];
    }

    public IEnumerator<TiffField> GetEnumerator() {
        return _metadata.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return _metadata.Values.GetEnumerator();
    }
}

public enum TiffCompression {
    NoCompression = 1,
    CCITT = 2,
    PackBits = 32773
}

public enum TiffImageType {
    WhiteIsZero = 0, // Greyscale, white is 0 and black is not-zero
    BlackIsZero = 1, // Greyscale, black is 0 and white is not-zero
    RgbFullColour = 2, // Each pixel is of 3 components (BITS PER SAMPLE is a triplet for each colour)
    PaletteColour = 3, // Each pixel is a reference to a colour in the map (BITS PER SAMPLE is a single value)
}
