using System.IO;

namespace Qkmaxware.Media.Image;

#region Read Only
/// <summary>
/// Interface representing an image loading format
/// </summary>
public interface IImageLoader {
    /// <summary>
    /// Load an image from the hard drive at the given path
    /// </summary>
    /// <param name="path">file path</param>
    /// <returns>image</returns>
    public IImage Load(string path);
}

/// <summary>
/// Interface representing an image loading format from a text based ASCII source
/// </summary>
public interface IAsciiImageLoader : IImageLoader {
    /// <summary>
    /// Load an image from the given text reader
    /// </summary>
    /// <param name="reader">text reader</param>
    /// <returns>image</returns>
    public IImage LoadFrom(TextReader reader);
}

/// <summary>
/// Interface representing an image loading format from a binary source
/// </summary>
public interface IBinaryImageLoader : IImageLoader {
    /// <summary>
    /// Load an image from the given binary reader
    /// </summary>
    /// <param name="reader">binary reader</param>
    /// <returns>image</returns>
    public IImage LoadFrom(BinaryReader reader);
    //public void SaveTo(BinaryWriter writer, IImage image);
}
#endregion

#region Write Only
/// <summary>
/// Interface representing an image saving format
/// </summary>
public interface IImageEncoder {
    /// <summary>
    /// Save an image to the given file path
    /// </summary>
    /// <param name="path">file path</param>
    /// <param name="image">image to save</param>
    public void Save(string path, IImage image);
}
/// <summary>
/// Interface representing an image saving format to a text based ASCII representation
/// </summary>
public interface IAsciiImageEncoder : IImageEncoder {
    /// <summary>
    /// Save an image to the given text writer
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="image">image to save</param>
    public void SaveTo(TextWriter writer, IImage image);
}
/// <summary>
/// Interface representing an image saving format to a binary representation
/// </summary>
public interface IBinaryImageEncoder : IImageEncoder {
    /// <summary>
    /// Save an image to the given binary writer
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="image">image to save</param>
    public void SaveTo(BinaryWriter writer, IImage image);
}
#endregion