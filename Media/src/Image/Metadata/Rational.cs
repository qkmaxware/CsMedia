using System;

namespace Qkmaxware;

public abstract class Rational {
    public abstract int Sign();
    public abstract double ToDouble();
}

public class UnsignedRational : Rational {
    public uint Numerator {get; private set;}
    public uint Denominator {get; private set;}
    public UnsignedRational(uint numerator, uint denominator) {
        this.Numerator = numerator;
        this.Denominator = denominator;
    }

    public override int Sign() => Numerator == 0 ? 0 : 1;
    public override double ToDouble() => (double)Numerator / (double)Denominator;
    public override string ToString() => $"{Numerator}/{Denominator}";
}

public class SignedRational : Rational {
    public int Numerator {get; private set;}
    public int Denominator {get; private set;}
    public SignedRational(int numerator, int denominator) {
        this.Numerator = numerator;
        this.Denominator = denominator;
    }

    public override int Sign() => Math.Sign(Numerator) * Math.Sign(Denominator);
    public override double ToDouble() => (double)Numerator / (double)Denominator;
    public override string ToString() => $"{Numerator}/{Denominator}";
}

