using System.IO;
using Colour = System.Drawing.Color;

namespace Qkmaxware.Media.Image {

/// <summary>
/// Static class for encoding images to TGA format
/// </summary>
public class TgaSerializer {
    /// <summary>
    /// MIME type
    /// </summary>
    public static readonly string MIME = "image/x-targa";

    /// <summary>
    /// Convert an image to a Targa (TGA) file format
    /// </summary>
    /// <param name="writer">writer to write to</param>
    /// <param name="sample">image to sample</param>
    public void Serialize(BinaryWriter writer, IColourSampler sampler) {
        var size = sampler.GetSize();
        //Header
        byte[] header = new byte[18];
        header[0] = (byte)0; //ID Length
        header[1] = (byte)0; //Colour map type (no colour map)
        header[2] = (byte)2; //Image type (uncompressed true-colour image)
        //Width (2 bytes)
        header[12] = (byte)(255 & size.Width);
        header[13] = (byte)(255 & (size.Width >> 8));
        //Height (2 bytes)
        header[14] = (byte)(255 & size.Height);
        header[15] = (byte)(255 & (size.Height >> 8));
        header[16] = (byte)24; //Pixel depth
        header[17] = (byte)32; //
        writer.Write(header);

        //Body
        for(int row = 0; row < size.Height; row++) {
            for(int column = 0; column < size.Width; column++){
                Colour c = sampler.GetPixelColour(row, column);
                writer.Write(c.R);
                writer.Write(c.G);
                writer.Write(c.B);
            }
        }
    }

}

}