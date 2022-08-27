namespace Qkmaxware.Media.Image.Filters;

/// <summary>
/// Static class for quick access to image filters
/// </summary>
public static class FilterExtensions {
    /// <summary>
    /// Change the sample bit depth of this image to a new depth
    /// </summary>
    /// <param name="image">image to change</param>
    /// <param name="depth">the desired sample bit depth</param>
    /// <returns>image with samples scaled to the new bit depth</returns>
    public static IImage ChangeBitDepth(this IImage image, SampleDepth depth) => new SampleBitDepthFilter(depth).Process(image);

}