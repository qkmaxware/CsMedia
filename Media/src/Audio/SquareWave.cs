using System;

namespace Qkmaxware.Media.Audio {

/// <summary>
/// Square waveform generator
/// </summary>
public class SquareWave : BaseWaveform {

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
    /// Create a square wave with the given wavelength and amplitude
    /// </summary>
    /// <param name="wavelength">wavelength</param>
    /// <param name="amplitude">amplitude</param>
    /// <param name="phase">phase</param>
    public SquareWave(double wavelength, double amplitude = 1, double phase = 0) {
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
        return (Amplitude * Math.Sign(Math.Sin(Pi2 * Frequency * t)));
    }
}

}