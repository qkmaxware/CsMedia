using System.IO;

namespace Qkmaxware.Media.Audio {

/// <summary>
/// Waveform audio format
/// </summary>
public class WaveformFormat : IBinaryAudioEncoder {
    /// <summary>
    /// MIME type
    /// </summary>
    public static readonly string MIME = "audio/wav";

    public enum WaveFormatCode {
        PCM = 0x0001,
        IEEEFloat = 0x0003,
        ALAW = 0x0006,
        MULAW = 0x0007,
        Extensible = 0xFFFE
    }

    /// <summary>
    /// Encode a waveform to a file at the given path
    /// </summary>
    /// <param name="path">path to file</param>
    /// <param name="sampler">waveform to encode</param>
    public void Save(string path, IWaveformSampler sampler) {
        if (!path.EndsWith(".wav"))
            path += ".wav";
        using (var fs = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(fs)) {
            SaveTo(writer, sampler);
        }
    }

    /// <summary>
    /// Convert a waveform into WAV file format
    /// </summary>
    /// <param name="writer">writer to write to</param>
    /// <param name="sampler">waveform to sample</param>
    public void SaveTo (BinaryWriter writer, IWaveformSampler sampler) {
        // Basic info
        uint sampleCount = 44100;
        ushort channelCount = 1;
        ushort sampleLength = 1; // bytes
        uint sampleRate = 22050;

        // Write RIFF chunk
        writer.Write("RIFF".ToCharArray()); // ckID
        writer.Write(36 + sampleCount * channelCount * sampleLength); // cksize
        writer.Write("WAVE".ToCharArray()); // WAVEID

        // Write fmt chunk
        writer.Write("fmt".ToCharArray()); // ckID
        writer.Write(16);// cksize
        writer.Write((int)WaveFormatCode.PCM); // wFormatTag
        writer.Write(channelCount); // nChannels
        writer.Write(sampleRate); // nSamplesPerSec
        writer.Write(sampleRate * sampleLength * channelCount); // nAvgBytesPerSec
        writer.Write(sampleLength * channelCount); // nBlockAlign
        writer.Write((ushort)(8 * sampleLength)); // wBitsPerSample

        // Write wave chunks
        writer.Write("data".ToCharArray()); // ckID
        writer.Write(sampleCount * sampleLength); // cksize
        double t = 0d;
        for (int i = 0; i < sampleCount; i++, t += 1.0 / sampleRate) {
            writer.Write((byte)(((int)sampler.AmplitudeAt(t) + (sampleLength == 1 ? 128 : 0)) & 0xff));
        }
    }
}

}