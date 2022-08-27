using System.IO;

namespace Qkmaxware.Media.Video;

#region Read Only
/// <summary>
/// Interface representing an video loading format
/// </summary>
public interface IVideoLoader {
    /// <summary>
    /// Load a waveform from the given path
    /// </summary>
    /// <param name="path">path to file</param>
    /// <returns>waveform</returns>
    public IVideo Load(string path);
}
#endregion

#region Write Only
/// <summary>
/// Interface representing an video saving format
/// </summary>
public interface IVideoEncoder {
    /// <summary>
    /// Encode a waveform to a file at the given path
    /// </summary>
    /// <param name="path">path to file</param>
    /// <param name="sampler">waveform to encode</param>
    public void Save(string path, IVideo sampler);
}
/// <summary>
/// Interface representing an video saving format that encodes to binary
/// </summary>
public interface IBinaryVideoEncoder : IVideoEncoder {
    /// <summary>
    /// Encode a waveform to the given writer
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="sampler">waveform to encode</param>
    public void SaveTo(BinaryWriter writer, IVideo sampler);
}
#endregion