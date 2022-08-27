namespace Qkmaxware.Media.Image.Filters;

/// <summary>
/// Change the bit-depth of a given image, rescaling values as needed
/// </summary>
public class SampleBitDepthFilter : IImageFilter {
    private SampleDepth newDepth;
    public SampleBitDepthFilter(SampleDepth newDepth) {
        this.newDepth = newDepth;
    }

    private static uint scaleLinear(uint value, uint inMax, uint outMax) {
        var result = (double)(value * outMax) / (double)inMax;

        if (result > outMax) {
            return outMax;
        } else if (result < 0) {
            return 0u;
        }

        return (uint)result;
    }

    /// <summary>
    /// Perform sample bit depth scaling
    /// </summary>
    /// <param name="image">image to change the depth of</param>
    /// <returns>image with new bitdepth</returns>
    public IImage Process(IImage image) {
        if (image.SampleBitDepth == newDepth)
            return image; // No need to do anything, already has the same depth

        var from = (uint)image.SampleBitDepth;
        var to = (uint)newDepth;
        var next = new MemoryImage(newDepth, image.Width, image.Height);
        for (var row = 0L; row < image.Height; row++) {
            for (var col = 0L; col < image.Width; col++) {
                var currentPixel = image.Pixels?[row, col];
                Pixel nextPixel = new Pixel(currentPixel?.Samples.Length ?? 0);
                for (var i = 0; i < nextPixel.Samples.Length; i++) {
                    nextPixel.Samples[i] = scaleLinear(currentPixel?.Samples[i] ?? 0, from, to); // Linear scaling from one range to another range
                }
                next.Pixels[row, col] = nextPixel;
            }
        }
        return next;
    }
}