using System.Drawing;
using Colour = System.Drawing.Color;

namespace Qkmaxware.Media.Image {

/// <summary>
/// Interface representing any class that can sample pixel colours from a given source
/// </summary>
public interface IColourSampler {
    /// <summary>
    /// Dimensions of the pixel space
    /// </summary>
    /// <returns>width and height of the space</returns>
    Size GetSize();
    /// <summary>
    /// Query the colour of the pixel at the given coordinates
    /// </summary>
    /// <param name="row">the row</param>
    /// <param name="column">the column</param>
    /// <returns>colour at the given row and column</returns>
    Colour GetPixelColour(int row, int column);
}

}