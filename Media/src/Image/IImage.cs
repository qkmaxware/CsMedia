using System.Drawing;

namespace Qkmaxware.Media.Image;

/// <summary>
/// Interface all images must implement
/// </summary>
public interface IImage {
    // Specs
    public int Width {get;}
    public int Height {get;}

    // Metadata
    public Metadata.MetadataContainer Metadata {get;}

    // Data
    public SampleDepth SampleBitDepth {get;}
    public Pixel[,]? Pixels {get;}
}

/// <summary>
/// An image entirely represented in memory
/// </summary>
public class MemoryImage : IImage {
    public Pixel[,] Pixels {get; private set;}

    public Metadata.MetadataContainer Metadata {get; private set;} = new Metadata.MetadataContainer();

    public int Width => Pixels.GetLength(1);
    public int Height => Pixels.GetLength(0);

    public SampleDepth SampleBitDepth {get; private set;}

    public MemoryImage (SampleDepth depth, int width, int height) {
        this.SampleBitDepth = depth;
        this.Pixels = new Pixel[height,width];
    }

    public MemoryImage(SampleDepth depth, Pixel[,] pixels) {
        this.SampleBitDepth = depth;
        this.Pixels = pixels;
    }

    public MemoryImage (Color[,] colours) : this(SampleDepth.Bit8, colours.GetLength(1), colours.GetLength(0)) {
        for (var row = 0; row < this.Height; row++) {
            for (var col = 0; col < this.Width; col++) {
                var colour = colours[row, col];
                this.Pixels[row, col] = new Pixel(new uint[]{ colour.R, colour.G, colour.B, colour.A });
            }
        }
    }
}