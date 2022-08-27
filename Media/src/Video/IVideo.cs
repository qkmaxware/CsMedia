using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Qkmaxware.Media.Image;

namespace Qkmaxware.Media.Video;

/// <summary>
/// Interface representing a video, a collection of image frames
/// </summary>
public interface IVideo : IEnumerable<IImage> {
    /// <summary>
    /// Width of the video
    /// </summary>
    /// <value>pixels</value>
    public int Width {get;}
    /// <summary>
    /// Height of the video
    /// </summary>
    /// <value>pixels</value>
    public int Height {get;}
    /// <summary>
    /// Number of frames per second
    /// </summary>
    /// <value>frames per second</value>
    public int FramesPerSecond {get;}
}

/// <summary>
/// A video entirely represented in memory
/// </summary>
public class MemoryVideo : IVideo {
    private List<IImage> images = new List<IImage>();

    public int Width => images.FirstOrDefault()?.Width ?? 0; 

    public int Height => images.FirstOrDefault()?.Height ?? 0; 

    public int FramesPerSecond {get; set;} = 24;

    public MemoryVideo() {}

    public MemoryVideo(IEnumerable<IImage> frames) : this() {
        this.AddFrames(frames);
    }

    public void AddFrame(IImage image) {
        this.images.Add(image);
    }

    public void AddFrames(IEnumerable<IImage> frames) {
        this.images.AddRange(frames);
    }

    public IEnumerator<IImage> GetEnumerator()  => images.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => images.GetEnumerator();
}