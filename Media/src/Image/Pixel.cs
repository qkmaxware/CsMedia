namespace Qkmaxware.Media.Image;

public class Pixel {
    /// <summary>
    /// First sample, represeting the red component
    /// </summary>
    /// <returns>first sample</returns>
    public uint Red => Samples != null && Samples.Length >= 1 ? Samples[0] : default(uint);
    /// <summary>
    /// Second sample, representing the green component
    /// </summary>
    /// <returns>second sample</returns>
    public uint Green => Samples != null && Samples.Length >= 2 ? Samples[1] : default(uint);
    /// <summary>
    /// Third sample, representing the blue component
    /// </summary>
    /// <returns>third component</returns>
    public uint Blue => Samples != null && Samples.Length >= 3 ? Samples[2] : default(uint);

    /// <summary>
    /// All pixel samples
    /// </summary>
    /// <value>samples</value>
    public uint[] Samples {get; set;}

    /// <summary>
    /// Create a basic RGB pixel
    /// </summary>
    public Pixel() : this(3) {}

    /// <summary>
    /// Create a pixel with the given number of samples
    /// </summary>
    public Pixel(int samples) {
        this.Samples = new uint[samples];
    }

    /// <summary>
    /// Create a pixel with the given sample values
    /// </summary>
    public Pixel(uint[] samples) {
        this.Samples = samples;
    }

    public bool IsRedLargest  => Red > Green && Red > Blue;
    public bool IsGreenLargest => Green > Red && Green > Blue;
    public bool IsBlueLargest => Blue > Red && Blue > Green;
    // public float Luminance => 0.2126f * Red + 0.7152f * Green + 0.0722f * Blue; // Need to be linear gamma corrected RGB

    public override string ToString() {
        return $"(r: {Red}, g: {Green}, b: {Blue})";
    }
}