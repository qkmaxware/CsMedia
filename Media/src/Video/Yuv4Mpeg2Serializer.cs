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
        float fr = (float)c.R / 255;
        float fg = (float)c.G / 255;
        float fb = (float)c.B / 255;

        float Y = (float)(0.2989 * fr + 0.5866 * fg + 0.1145 * fb);
        float Cb = (float)(-0.1687 * fr - 0.3313 * fg + 0.5000 * fb);
        float Cr = (float)(0.5000 * fr - 0.4184 * fg - 0.0816 * fb);

        return (
            Y: (byte)(Y * 255),
            Cb: (byte)(Cb* 255),
            Cr: (byte)(Cr* 255)
        ); 
    }

    /// <summary>
    /// Return the colour of the image or black if the index is out of bounds
    /// </summary>
    /// <param name="sampler">colour sampler</param>
    /// <param name="row">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns>colour from sampler or black</returns>
    private Colour ColorOrDefault(IColourSampler sampler, int row, int column) {
        var size = sampler.GetSize();
        if (row < 0 || row >= size.Height) {
            return Colour.Black;
        } else if (column < 0 || column >= size.Width) {
            return Colour.Black;
        } else {
            return sampler.GetPixelColour(row, column);
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
        writer.Write((char)0x20); writer.Write('H'); writer.Write(height.ToString().ToCharArray());
        writer.Write((char)0x20); writer.Write('F'); writer.Write(frameRate.ToString().ToCharArray()); writer.Write(':'); writer.Write('1');
        //writer.Write(0x20); writer.Write("Ip".ToCharArray()); // progressive interlacing
        writer.Write((char)0x20); writer.Write("A1:1".ToCharArray()); // square pixels
        writer.Write((char)0x20); writer.Write("C444".ToCharArray());
        writer.Write((char)0x0A);
        writer.Flush(); // Flush after header

        // Write frame(s)
        foreach (var frame in frames) {
            writer.Write("FRAME".ToCharArray());
            writer.Write((char)0x0A);

            // Write Y frame
            for (var row = 0; row < height; row++) {
                for (var column = 0; column < width; column++) {
                    var colour = ColorOrDefault(frame, row, column); // height is the first dimension, width the second
                    var Y = (byte)((colour.R * 77f/256f + colour.G * 150f/256f + colour.B * 29f/256f));

                    writer.Write(Y);
                }
            }
            // Write U frame
            for (var row = 0; row < height; row++) {
                for (var column = 0; column < width; column++) {
                    var colour = ColorOrDefault(frame, row, column); // height is the first dimension, width the second
                    var U = (byte)((colour.R * -43f/256f + colour.G * -84f/256f + colour.B * 127f/256f + 128));

                    writer.Write(U);
                }
            }
            // Write V frame
            for (var row = 0; row < height; row++) {
                for (var column = 0; column < width; column++) {
                    var colour = ColorOrDefault(frame, row, column); // height is the first dimension, width the second
                    var V = (byte)(colour.R * 127f/256f + colour.G * -106f/256f + colour.B * -21f/256f + 128);

                    writer.Write(V);
                }
            }
            //writer.Write((char)0x0A);
            writer.Flush(); // Flush after frame
        }
    }

}

}