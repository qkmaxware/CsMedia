using System.IO;
using Colour = System.Drawing.Color;

namespace Qkmaxware.Media.Image {

/// <summary>
/// Static class for encoding images to Netpbm PPM format
/// </summary>
public class PpmSerializer {

    /// <summary>
    /// MIME type
    /// </summary>
    public static readonly string MIME = "image/x-portable-pixmap";

    /// <summary>
    /// Convert an image to a Portal Pixel Map (PPM) file format
    /// </summary>
    /// <param name="writer">writer to write to</param>
    /// <param name="sample">image to sample</param>
    public void Serialize(TextWriter writer, IColourSampler sampler) {
        var size = sampler.GetSize();
        //Head
        writer.WriteLine("P3");
        writer.WriteLine(size.Width + " " + size.Height);
        writer.WriteLine(255);
        //Body
        for(int row = 0; row < size.Height; row++) {
            for(int column = 0; column < size.Width; column++){
                Colour c = sampler.GetPixelColour(row, column);
                writer.WriteLine(c.R + " " + c.G + " " + c.B);
            }
        }
    }
}

}