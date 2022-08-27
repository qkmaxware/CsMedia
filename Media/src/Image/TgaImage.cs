using System.IO;

namespace Qkmaxware.Media.Image;

public class TargaGraphicDecoderException : System.Exception {
    public TargaGraphicDecoderException(string message) : base(message) {}
}

public class TargaGraphicEncoderException : System.Exception {
    public TargaGraphicEncoderException(string message) : base(message) {}
}

/// <summary>
/// Image format for Targa graphics images (TGA)
/// </summary>
public class TargaGraphicFormat : IBinaryImageEncoder {
    public void Save(string path, IImage image) {
        if (!path.EndsWith(".tga"))
            path += ".tga";
        // Default save in binary
        using (var fs = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(fs)) {
            SaveTo(writer, image);
        }
    }

    /// <summary>
    /// Save a binary TGA file
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="image">image to save</param>
    public void SaveTo(BinaryWriter writer, IImage image) {
        var width = image.Width;
        var height = image.Height;

        //Header
        byte[] header = new byte[18];
        header[0] = (byte)0; //ID Length
        header[1] = (byte)0; //Colour map type (no colour map)
        header[2] = (byte)2; //Image type (uncompressed true-colour image)
        //Width (2 bytes)
        header[12] = (byte)(255 & width);
        header[13] = (byte)(255 & (width >> 8));
        //Height (2 bytes)
        header[14] = (byte)(255 & height);
        header[15] = (byte)(255 & (height >> 8));
        header[16] = (byte)((int)image.SampleBitDepth * 3); //Pixel depth
        header[17] = (byte)32; //
        writer.Write(header);

        //Body
        for(int row = 0; row < height; row++) {
            for(int column = 0; column < width; column++){
                var pixel = image.Pixels?[row, column];

                // TODO support multiple pixel depths
                switch (image.SampleBitDepth) {
                    case SampleDepth.Bit8: {
                        writer.Write((byte)(pixel?.Blue ?? 0));
                        writer.Write((byte)(pixel?.Green ?? 0));
                        writer.Write((byte)(pixel?.Red ?? 0));
                        break;
                    }
                    case SampleDepth.Bit16: {
                        writer.Write((ushort)(pixel?.Blue ?? 0));
                        writer.Write((ushort)(pixel?.Green ?? 0));
                        writer.Write((ushort)(pixel?.Red ?? 0));
                        break;
                    }
                    case SampleDepth.Bit32: {
                        writer.Write((uint)(pixel?.Blue ?? 0));
                        writer.Write((uint)(pixel?.Green ?? 0));
                        writer.Write((uint)(pixel?.Red ?? 0));
                        break;
                    }
                    default:
                        throw new TargaGraphicEncoderException($"Images with {(int)image.SampleBitDepth}bit samples are not currently supported");
                }
            }
        }
    }
}