namespace Qkmaxware.Media.Image.Filters;

/*
    Ideal Syntax (LINQ like)
    image.ChangeBitDepth(Bit16).Colourize(hue, saturation, lightness).Resize(1920, 1080);
*/

/// <summary>
/// Interface representing a filter which can alter the size, colours, or other characteristics of an image
/// </summary>
public interface IImageFilter {
    public IImage Process(IImage image);
}

