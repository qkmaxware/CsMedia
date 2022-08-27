using System;

namespace Qkmaxware.Media.Audio {

/// <summary>
/// Cosine waveform generator
/// </summary>
public class CosineWave : BaseWaveform {

    /// <summary>
    /// Wavelength
    /// </summary>
    public double Wavelength {get; private set;}
    /// <summary>
    /// Amplitude
    /// </summary>
    public double Amplitude {get; private set;}
    /// <summary>
    /// Phase
    /// </summary>
    public double Phase {get; private set;}

    /// <summary>
    /// Frequency
    /// </summary>
    public double Frequency => C20 / Wavelength;

    /// <summary>
    /// Create a cosine wave with the given wavelength and amplitude
    /// </summary>
    /// <param name="wavelength">wavelength</param>
    /// <param name="amplitude">amplitude</param>
    /// <param name="phase">phase</param>
    public CosineWave(double wavelength, double amplitude = 1, double phase = 0) {
        this.Wavelength = wavelength;
        this.Amplitude = amplitude;
        this.Phase = phase;
    }

    /// <summary>
    /// Get the amplitude at a given instance in time
    /// </summary>
    /// <param name="time">time</param>
    /// <returns>amplitude</returns>
    public override double AmplitudeAt(double t) {
        return (Amplitude * Math.Cos((Pi2 * t - Phase) / Wavelength));
    }
}

}