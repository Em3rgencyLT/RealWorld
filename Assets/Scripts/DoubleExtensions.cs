using System;
public static class DoubleExtensions
{
    const double _7 = 0.0000001;

    public static bool Equals7DigitPrecision(this double left, double right)
    {
        return Math.Abs(left - right) < _7;
    }
}