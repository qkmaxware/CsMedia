using System.Drawing;
using Colour = System.Drawing.Color;

namespace Qkmaxware.Media.Image {

/// <summary>
/// Extension methods to create samplers from various formats
/// </summary>
public static class SamplerExtensions {
    /// <summary>
    /// Create a colour sampler for a 2d array of colours
    /// </summary>
    /// <param name="colours">the colour array</param>
    /// <returns>colour sampler for this array</returns>
    public static IColourSampler GetSampler(this Colour[,] colours) {
        return new MatrixColourSampler(colours);
    }
    /*/// <summary>
    /// Create a colour sampler for a bitmap image
    /// </summary>
    /// <param name="image">image to sample</param>
    /// <returns>colour sampler for this image</returns>
    public static IColourSampler GetSampler(System.Drawing.Bitmap image) {
        return new BitmapColourSampler(image);
    }*/
}
/*
/// <summary>
/// Class for sampling colours from a 2d array 
/// </summary>
public class BitmapColourSampler : IColourSampler {
    /// <summary>
    /// Colours
    /// </summary>
    private System.Drawing.Bitmap image;
    /// <summary>
    /// Create a new sampler
    /// </summary>
    /// <param name="colours">colours to sample in row,column format</param>
    public BitmapColourSampler(System.Drawing.Bitmap image) {
        this.image = image;
    }
    /// <summary>
    /// Query the colour of the pixel at the given coordinates
    /// </summary>
    /// <param name="x">the row</param>
    /// <param name="y">the column</param>
    /// <returns>colour at the given row and column</returns>
    public Colour GetPixelColour(int x, int y) {
        return this.image.GetPixel(x, y);
    }
    /// <summary>
    /// Dimensions of the pixel space
    /// </summary>
    /// <returns>width and height of the space</returns>
    public Size GetSize() {
        return new Size(
            width: image.Width,
            height: image.Height
        );
    }
}*/

/// <summary>
/// Class for sampling colours from a 2d array 
/// </summary>
public class MatrixColourSampler : IColourSampler {
    /// <summary>
    /// Colours
    /// </summary>
    private Colour[,] colours;
    /// <summary>
    /// Create a new sampler
    /// </summary>
    /// <param name="colours">colours to sample in row,column format</param>
    public MatrixColourSampler(Colour[,] colours) {
        this.colours = colours;
    }
    /// <summary>
    /// Query the colour of the pixel at the given coordinates
    /// </summary>
    /// <param name="x">the row</param>
    /// <param name="y">the column</param>
    /// <returns>colour at the given row and column</returns>
    public Colour GetPixelColour(int x, int y) {
        return colours[x, y];
    }
    /// <summary>
    /// Dimensions of the pixel space
    /// </summary>
    /// <returns>width and height of the space</returns>
    public Size GetSize() {
        return new Size(
            width: colours.GetLength(1),
            height: colours.GetLength(0)
        );
    }
}

}