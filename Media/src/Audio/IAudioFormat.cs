using System.IO;

namespace Qkmaxware.Media.Audio;

#region Read Only
/// <summary>
/// Interface representing an audio loading format
/// </summary>
public interface IAudioLoader {
    /// <summary>
    /// Load a waveform from the given path
    /// </summary>
    /// <param name="path">path to file</param>
    /// <returns>waveform</returns>
    public IWaveformSampler Load(string path);
}
#endregion

#region Write Only
/// <summary>
/// Interface representing an audio saving format
/// </summary>
public interface IAudioEncoder {
    /// <summary>
    /// Encode a waveform to a file at the given path
    /// </summary>
    /// <param name="path">path to file</param>
    /// <param name="sampler">waveform to encode</param>
    public void Save(string path, IWaveformSampler sampler);
}
/// <summary>
/// Interface representing an audio saving format that encodes to binary
/// </summary>
public interface IBinaryAudioEncoder : IAudioEncoder {
    /// <summary>
    /// Encode a waveform to the given writer
    /// </summary>
    /// <param name="writer">writer</param>
    /// <param name="sampler">waveform to encode</param>
    public void SaveTo(BinaryWriter writer, IWaveformSampler sampler);
}
#endregion