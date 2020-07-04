namespace Qkmaxware.Media.Audio {

/// <summary>
/// Interface to sample an audio source
/// </summary>
public interface IWaveformSampler {
    /// <summary>
    /// Get the amplitude at a given instance in time
    /// </summary>
    /// <param name="time">time</param>
    /// <returns>amplitude</returns>
    double AmplitudeAt(double time);
}

}