using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Qkmaxware.Media.Image;

public class PortablePixelMapDecoderException : System.Exception {
    public PortablePixelMapDecoderException(string message) : base(message) {}
}

public class PortablePixelMapEncoderException : System.Exception {
    public PortablePixelMapEncoderException(string message) : base(message) {}
}

/// <summary>
/// Image format for Netpbm Portable Pixel Map images (http://netpbm.sourceforge.net/doc/ppm.html)
/// </summary>
public class PortablePixelMapFormat : IAsciiImageLoader, IBinaryImageLoader, IAsciiImageEncoder, IBinaryImageEncoder {
    /// <summary>
    /// Load an arbitary PPM image
    /// </summary>
    /// <param name="path">path to image</param>
    /// <returns>image</returns>
    public IImage Load(string path) {
        char P = default(char), N = default(char);

        using (var reader = new StreamReader(path)) {
            P = (char)reader.Read();
            N = (char)reader.Read();
        }

        if (P == 'P' && N == '3') {
            using (var reader = new StreamReader(path)) {
                return LoadFrom(reader);
            }
        } else if (P == 'P' && N == '6') {
            using (var fs = new FileStream(path, FileMode.Open)) 
            using (var reader = new BinaryReader(fs)) {
                return LoadFrom(reader);
            }
        } else {
            throw new PortablePixelMapDecoderException("Not a valid PPM image");
        }
    }

    /// <summary>
    /// Load a Plain PPM ascii file
    /// </summary>
    /// <param name="reader">text reader</param>
    /// <returns>image</returns>
    public IImage LoadFrom(TextReader reader) {
        // Plain PPM
        var P = (char)reader.Read();
        var N = (char)reader.Read();
        if (P != 'P' || N != '3') {
            throw new PortablePixelMapDecoderException("Not a valid PPM image");
        }

        skipWhitespaceAndComments(reader);
        var width = reader.ReadInt();
        skipWhitespaceAndComments(reader);
        var height = reader.ReadInt();
        skipWhitespaceAndComments(reader);
        var maxBitValue = reader.ReadInt();
        skipWhitespaceAndComments(reader);

        MemoryImage image = new MemoryImage(maxBitValue < 256 ? SampleDepth.Bit8 : SampleDepth.Bit16, width, height);
        for (var row = 0; row < height; row++) {
            for (var col = 0; col < width; col++) {
                // Read red,
                skipWhitespaceAndComments(reader);
                var r = reader.ReadUInt();
                // Read green
                skipWhitespaceAndComments(reader);
                var g = reader.ReadUInt();
                // Read blue
                skipWhitespaceAndComments(reader);
                var b = reader.ReadUInt();

                image.Pixels[row,col] = new Pixel(new uint[]{ r, g, b });
            }
        }

        return image;
    }
    private void skipWhitespaceAndComments(TextReader reader) {
        int c;
        while ((c = reader.Peek()) != -1) {
            if (char.IsWhiteSpace((char)c)) {
                // Skip whitespace
                reader.Read();
                continue;
            } else if ((char)c == '#') {
                // Skip comment
                while ((c = reader.Peek()) != -1 && ((char)c != '\n')) {
                    reader.Read();
                }
                continue;
            } else {
                break;
            }
        }
    }

    /// <summary>
    /// Load a binary PPM file
    /// </summary>
    /// <param name="reader">binary reader</param>
    /// <returns>image</returns>
    public IImage LoadFrom(BinaryReader reader) {
        // Baseline PPM
        var P = reader.ReadChar();
        var N = reader.ReadChar();
        if (P != 'P' || N != '6') {
            throw new PortablePixelMapDecoderException("Not a valid PPM image");
        }

        skipWhitespaceAndComments(reader);
        var width = reader.ReadAsciiInt();
        skipWhitespaceAndComments(reader);
        var height = reader.ReadAsciiInt();
        skipWhitespaceAndComments(reader);
        var maxBitValue = reader.ReadAsciiInt();
        skipWhitespaceAndComments(reader);

        MemoryImage image = new MemoryImage(maxBitValue < 256 ? SampleDepth.Bit8 : SampleDepth.Bit16, width, height);
        for (var row = 0; row < height; row++) {
            for (var col = 0; col < width; col++) {
                // Read red,
                skipWhitespaceAndComments(reader);
                var r = maxBitValue < 256 ? reader.ReadByte() : reader.ReadUInt16();
                // Read green
                skipWhitespaceAndComments(reader);
                var g = maxBitValue < 256 ? reader.ReadByte() : reader.ReadUInt16();
                // Read blue
                skipWhitespaceAndComments(reader);
                var b = maxBitValue < 256 ? reader.ReadByte() : reader.ReadUInt16();

                image.Pixels[row,col] = new Pixel(new uint[]{ r, g, b });
            }
        }

        return image;
    }
    private void skipWhitespaceAndComments(BinaryReader reader) {
        int c;
        while ((c = reader.PeekChar()) != -1) {
            if (char.IsWhiteSpace((char)c)) {
                // Skip whitespace
                reader.Read();
                continue;
            } else if ((char)c == '#') {
                // Skip comment
                while ((c = reader.PeekChar()) != -1 && ((char)c != '\n')) {
                    reader.Read();
                }
                continue;
            } else {
                break;
            }
        }
    }

    public void Save(string path, IImage image) {
        // Default save in binary
        if (!path.EndsWith(".ppm"))
            path += ".ppm";
        using (var fs = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(fs)) {
            SaveTo(writer, image);
        }
    }

    /// <summary>
    /// Save a text based Plain PPM file
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="image">image to save</param>
    public void SaveTo(TextWriter writer, IImage image) {
        var width = image.Width;
        var height = image.Height;
        var maxSampleValue = image.SampleBitDepth switch {
            SampleDepth.Bit8 => 255u,
            SampleDepth.Bit16 => 65_535u,
            SampleDepth.Bit32 => 4_294_967_295u,
            _ => throw new PortablePixelMapEncoderException($"Images with {(int)image.SampleBitDepth}bit samples are not currently supported")
        };
        if (image.Pixels != null) {
            foreach (var pixel in image.Pixels) {
                if (pixel != null) {
                    if (pixel.Red > maxSampleValue)
                        maxSampleValue = pixel.Red;
                    if (pixel.Green > maxSampleValue)
                        maxSampleValue = pixel.Green;
                    if (pixel.Blue > maxSampleValue)
                        maxSampleValue = pixel.Blue;
                }
            }
        }

        //Head
        writer.WriteLine("P3");
        writer.WriteLine(width + " " + height);
        writer.WriteLine(maxSampleValue);

        //Body
        for(int row = 0; row < height; row++) {
            for(int column = 0; column < width; column++){
                var pixel = image.Pixels?[row, column];
                if (pixel == null) {
                    writer.WriteLine(0 + " " + 0 + " " + 0);
                } else {
                    writer.WriteLine(pixel.Red + " " + pixel.Green + " " + pixel.Blue);
                }
            }
        }
    }

    /// <summary>
    /// Save a text based binary PPM file
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="image">image to save</param>
    public void SaveTo(BinaryWriter writer, IImage image) {
        var width = image.Width;
        var height = image.Height;
        var maxSampleValue = image.SampleBitDepth switch {
            SampleDepth.Bit8 => 255u,
            SampleDepth.Bit16 => 65_535u,
            SampleDepth.Bit32 => 4_294_967_295u,
            _ => throw new PortablePixelMapEncoderException($"Images with {(int)image.SampleBitDepth}bit samples are not currently supported")
        };
        if (image.Pixels != null) {
            foreach (var pixel in image.Pixels) {
                if (pixel != null) {
                    if (pixel.Red > maxSampleValue)
                        maxSampleValue = pixel.Red;
                    if (pixel.Green > maxSampleValue)
                        maxSampleValue = pixel.Green;
                    if (pixel.Blue > maxSampleValue)
                        maxSampleValue = pixel.Blue;
                }
            }
        }

        // Header
        writer.Write('P'); writer.Write('6'); writer.Write('\n');
        writer.Write(Encoding.ASCII.GetBytes(width.ToString())); writer.Write(' '); writer.Write(Encoding.ASCII.GetBytes(height.ToString())); writer.Write('\n');
        writer.Write(Encoding.ASCII.GetBytes(maxSampleValue.ToString())); writer.Write('\n');

        // Body
        for(int row = 0; row < height; row++) {
            for(int column = 0; column < width; column++){
                var pixel = image.Pixels?[row, column];
                var r = pixel?.Red ?? 0;
                var g = pixel?.Green ?? 0;
                var b = pixel?.Blue ?? 0;

                if (maxSampleValue < 256) {
                    writer.Write((byte)r); writer.Write((byte)g); writer.Write((byte)b);
                } else if (maxSampleValue < 65_536) {
                    writer.Write((UInt16)r); writer.Write((UInt16)g); writer.Write((UInt16)b);
                } else {
                    writer.Write((UInt32)r); writer.Write((UInt32)g); writer.Write((UInt32)b);
                }
            }
        }
    }
}