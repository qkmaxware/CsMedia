using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colour = System.Drawing.Color;
using Qkmaxware.Media.Image;

namespace Qkmaxware.Media.Video {

/// <summary>
/// Serializer to create YUV4MPEG2 video files
/// </summary>
public class Yuv4Mpeg2Serializer {

    /// <summary>
    /// Convert RGB colours to YCbCr colour space
    /// </summary>
    /// <param name="c">colour</param>
    /// <returns>Y Cb Cr tuple</returns>
    private (byte Y, byte Cb, byte Cr) ConvertToYCbCr(Colour c) {
        // Y is luma
        // Cb is blue-difference
        // Cr is red-difference

        var r = c.R; var g = c.G; var b = c.B;

        var m00 = 0.299; var m01 = 0.587; var m02 = 0.114;
        var m10 =-0.169; var m11 =-0.331; var m12 = 0.500;
        var m20 = 0.500; var m21 =-0.419; var m22 =-0.081;

        var Y   = m00 * r + m01 * g + m02 * b + 0;
        var Cb  = m10 * r + m11 * g + m12 * b + 128;
        var Cr  = m20 * r + m21 * g + m22 * b + 128;

        return (
            Y: (byte)Y,
            Cb: (byte)Cb,
            Cr: (byte)Cr
        ); 
    }

    /// <summary>
    /// Return the colour of the image or black if the index is out of bounds
    /// </summary>
    /// <param name="sampler">colour sampler</param>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns>colour from sampler or black</returns>
    private Colour ColorOrDefault(IColourSampler sampler, int x, int y) {
        var size = sampler.GetSize();
        if (x < 0 || x >= size.Width) {
            return Colour.Black;
        } else if (y < 0 || y >= size.Height) {
            return Colour.Black;
        } else {
            return sampler.GetPixelColour(x, y);
        }
    }

    /// <summary>
    /// Create video file
    /// </summary>
    /// <param name="writer">writer to write to</param>
    /// <param name="frames">video frames</param>
    /// <param name="framesPerSecond">playback speed in frames per second</param>
    public void Serialize(BinaryWriter writer, IEnumerable<IColourSampler> frames, int framesPerSecond = 24) {
        var width = frames.Select(sampler => sampler.GetSize().Width).Max();
        var height = frames.Select(sampler => sampler.GetSize().Height).Max();
        var frameRate = Math.Max(framesPerSecond, 0);

        // Write header
        writer.Write("YUV4MPEG2 ".ToCharArray());
        writer.Write('W'); writer.Write(width.ToString().ToCharArray());
        writer.Write(0x20); writer.Write('H'); writer.Write(height.ToString().ToCharArray());
        writer.Write(0x20); writer.Write('F'); writer.Write(frameRate.ToString().ToCharArray()); writer.Write(':'); writer.Write('1');
        //writer.Write(0x20); writer.Write("Ip".ToCharArray()); // progressive interlacing
        writer.Write(0x20); writer.Write("A1:1".ToCharArray()); // square pixels
        writer.Write(0x20); writer.Write("C444".ToCharArray());
        writer.Write(0x0A);

        // Write frame(s)
        foreach (var frame in frames) {
            writer.Write("FRAME".ToCharArray());

            for (var row = 0; row < width; row++) {
                for (var column = 0; column < height; column++) {
                    var colour = ColorOrDefault(frame, column, row);
                    var (Y, Cb, Cr) = ConvertToYCbCr(colour);

                    writer.Write(Y);
                    writer.Write(Cb);
                    writer.Write(Cr);
                }
            }

            writer.Write(0x0A);
        }
    }

}

}