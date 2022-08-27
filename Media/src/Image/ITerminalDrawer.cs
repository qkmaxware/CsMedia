namespace Qkmaxware.Media.Image;

/// <summary>
/// Interface to draw an image to a text based computer terminal or display
/// </summary>
public interface ITerminalDrawer {
    /// <summary>
    /// Draw an image and return the result as a printable string
    /// </summary>
    /// <param name="image">image to draw</param>
    /// <returns>ascii string</returns>
    public string Draw(IImage image);
}