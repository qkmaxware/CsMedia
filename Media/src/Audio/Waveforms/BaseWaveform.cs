using System;

namespace Qkmaxware.Media.Audio {

/// <summary>
/// Base class for all generated waveforms
/// </summary>
public abstract class BaseWaveform : IWaveformSampler {
    protected static double Pi2 = 2 * Math.PI;
    protected static double C20 => 343; // m/s at 20°C

    /// <summary>
    /// Get the amplitude at a given instance in time
    /// </summary>
    /// <param name="time">time</param>
    /// <returns>amplitude</returns>
    public abstract double AmplitudeAt(double d);
}

}