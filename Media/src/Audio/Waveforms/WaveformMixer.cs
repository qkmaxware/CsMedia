using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Qkmaxware.Media.Audio {

/// <summary>
/// Mix the output from several waveforms into a single waveform
/// </summary>
public class WaveformMixer : IEnumerable<IWaveformSampler>, IWaveformSampler {

    /// <summary>
    /// waveforms and their volume
    /// </summary>
    /// <typeparam name="IWaveformSampler">saveform</typeparam>
    /// <typeparam name="double">volume</typeparam>
    private Dictionary<IWaveformSampler, double> volumes = new Dictionary<IWaveformSampler, double>();

    /// <summary>
    /// Create an empty mixer
    /// </summary>
    public WaveformMixer() {}

    /// <summary>
    /// Add a waveform with the given volume to the mixer
    /// </summary>
    /// <param name="sampler">waveform</param>
    /// <param name="volume">volume</param>
    public void Add (IWaveformSampler sampler, double volume) {
        this.volumes[sampler] = volume;
    }

    /// <summary>
    /// Get the amplitude at a given instance in time
    /// </summary>
    /// <param name="time">time</param>
    /// <returns>amplitude</returns>
    public double AmplitudeAt(double time) {
        var amplitude = this.volumes.Select(pair => pair.Key.AmplitudeAt(time) * pair.Value).Sum();
        return amplitude;
    }

    /// <summary>
    /// Get an enumerable of waveforms in this mixer
    /// </summary>
    public IEnumerator<IWaveformSampler> GetEnumerator() {
        return volumes.Keys.GetEnumerator();
    }

    /// <summary>
    /// Get an enumerable of waveforms in this mixer
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

}