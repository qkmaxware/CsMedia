using System.Text;

namespace Qkmaxware.Media.Image;

/// <summary>
/// Class to draw images as ASCII
/// </summary>
public class AsciiDrawer : ITerminalDrawer {
    public char White = ' ';
    public char Light = '░';
    public char Medium = '▒';
    public char Dark = '▓';
    public char Black = '█';

    /// <summary>
    /// Draw an image as ASCII art
    /// </summary>
    /// <param name="image">image to draw</param>
    /// <returns>ascii string</returns>
    public string Draw(IImage image) {
        StringBuilder sb = new StringBuilder();

        for (var row = 0; row < image.Height; row++) {
            for (var col = 0; col < image.Width; col++) {
                var px = image.Pixels?[row, col];
                if (px == null) {
                    sb.Append(Light);
                    continue;
                }
                var luminocity = 0.2126f * (px.Red/255f) + 0.7152f * (px.Green/255f) + 0.0722f * (px.Blue/255f);
                sb.Append(luminocity == 0f ? Black : (luminocity < 0.25f ? Dark : (luminocity < 0.5f ? Medium : (luminocity < 0.75 ? Light : White))));
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}