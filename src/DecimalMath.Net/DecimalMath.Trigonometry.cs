namespace DecimalMath;

/// <summary>
/// Trigonometric members of <see cref="DecimalMath"/>. Split into its own file
/// because it is a distinct responsibility from roots, powers and logarithms,
/// even though both halves belong to the same public static surface.
/// </summary>
public static partial class DecimalMath
{
    /// <summary>
    /// Computes the sine of <paramref name="x"/>, an angle expressed in radians.
    /// </summary>
    /// <param name="x">An angle, in radians.</param>
    /// <returns>The sine of <paramref name="x"/>.</returns>
    /// <remarks>
    /// <paramref name="x"/> is range-reduced modulo 2*<see cref="Pi"/> before the
    /// series runs. Because <see cref="Pi"/> itself carries a relative error on
    /// the order of 1e-27 (see its own remarks), that error is scaled by the
    /// number of full periods being removed: reducing a very large angle (for
    /// example <c>Sin(1_000_000m)</c>, roughly 159,000 periods) can leave the
    /// reduced angle - and therefore the result - accurate to only a handful of
    /// significant digits rather than decimal's usual ~28. For angles within a
    /// few hundred periods of zero this is not observable; it only matters for
    /// very large <paramref name="x"/>.
    /// </remarks>
    public static decimal Sin(decimal x)
    {
        decimal reduced = ReduceAngle(x);
        return SinSeries(reduced);
    }

    /// <summary>
    /// Computes the cosine of <paramref name="x"/>, an angle expressed in radians.
    /// </summary>
    /// <param name="x">An angle, in radians.</param>
    /// <returns>The cosine of <paramref name="x"/>.</returns>
    /// <remarks>
    /// Subject to the same large-angle range-reduction precision loss described
    /// on <see cref="Sin"/>.
    /// </remarks>
    public static decimal Cos(decimal x)
    {
        decimal reduced = ReduceAngle(x);
        return CosSeries(reduced);
    }

    /// <summary>
    /// Computes the tangent of <paramref name="x"/>, an angle expressed in radians.
    /// </summary>
    /// <param name="x">An angle, in radians.</param>
    /// <returns>The tangent of <paramref name="x"/>.</returns>
    /// <remarks>
    /// Mathematically, tangent is undefined at odd multiples of pi/2, where
    /// cosine is zero; this method throws <see cref="DivideByZeroException"/> in
    /// that case. In practice that exact condition is effectively unreachable
    /// for <see cref="decimal"/> input: pi/2 has no exact finite-decimal
    /// representation, so no decimal <paramref name="x"/> lands precisely on an
    /// asymptote. Passing <paramref name="x"/> near such a point instead
    /// returns a very large finite value (positive or negative depending on
    /// which side of the asymptote <paramref name="x"/> falls) rather than
    /// throwing. Also subject to the large-angle range-reduction precision loss
    /// described on <see cref="Sin"/>.
    /// </remarks>
    public static decimal Tan(decimal x)
    {
        decimal reduced = ReduceAngle(x);
        decimal cosValue = CosSeries(reduced);
        if (cosValue == 0m)
        {
            throw new DivideByZeroException("Tangent is undefined where cosine is zero.");
        }

        return SinSeries(reduced) / cosValue;
    }

    /// <summary>
    /// Computes the arcsine of <paramref name="x"/> via Newton's method, seeded
    /// from a fast <see cref="double"/> approximation and refined against the
    /// decimal <see cref="Sin"/> and <see cref="Cos"/> series.
    /// </summary>
    /// <param name="x">A value in the closed range [-1, 1].</param>
    /// <returns>The arcsine of <paramref name="x"/>, in radians, in the range [-pi/2, pi/2].</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is outside [-1, 1].</exception>
    public static decimal Asin(decimal x)
    {
        if (x < -1m || x > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "Arcsine is only defined on the closed range [-1, 1].");
        }

        if (x == 1m)
        {
            return Pi / Two;
        }

        if (x == -1m)
        {
            return -Pi / Two;
        }

        if (x == 0m)
        {
            return 0m;
        }

        decimal guess = SeedFromAsinDouble(Math.Asin((double)x));

        for (int iteration = 0; iteration < NewtonMaxIterations; iteration++)
        {
            decimal reducedGuess = ReduceAngle(guess);
            decimal cosGuess = CosSeries(reducedGuess);
            if (cosGuess == 0m)
            {
                break;
            }

            decimal nextGuess = guess - (SinSeries(reducedGuess) - x) / cosGuess;
            if (Math.Abs(nextGuess - guess) < ConvergenceTolerance)
            {
                return nextGuess;
            }

            guess = nextGuess;
        }

        return guess;
    }

    /// <summary>
    /// Computes the arccosine of <paramref name="x"/> as pi/2 - <see cref="Asin"/>(x).
    /// </summary>
    /// <param name="x">A value in the closed range [-1, 1].</param>
    /// <returns>The arccosine of <paramref name="x"/>, in radians, in the range [0, pi].</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is outside [-1, 1].</exception>
    public static decimal Acos(decimal x)
    {
        if (x < -1m || x > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "Arccosine is only defined on the closed range [-1, 1].");
        }

        return Pi / Two - Asin(x);
    }

    /// <summary>
    /// Computes the arctangent of <paramref name="x"/> using the identity
    /// atan(x) = asin(x / sqrt(1 + x^2)), which holds for every real
    /// <paramref name="x"/>.
    /// </summary>
    /// <param name="x">The value to take the arctangent of.</param>
    /// <returns>The arctangent of <paramref name="x"/>, in radians, in the range (-pi/2, pi/2).</returns>
    public static decimal Atan(decimal x)
    {
        if (x == 0m)
        {
            return 0m;
        }

        decimal denominator = Sqrt(1m + x * x);
        return Asin(x / denominator);
    }

    /// <summary>
    /// Computes the angle, in radians, between the positive x-axis and the ray
    /// to the point (<paramref name="x"/>, <paramref name="y"/>), matching the
    /// quadrant-aware convention of <see cref="Math.Atan2(double, double)"/>.
    /// </summary>
    /// <param name="y">The y-coordinate.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <returns>The angle, in radians, in the range (-pi, pi].</returns>
    public static decimal Atan2(decimal y, decimal x)
    {
        if (x > 0m)
        {
            return Atan(y / x);
        }

        if (x < 0m)
        {
            return y >= 0m ? Atan(y / x) + Pi : Atan(y / x) - Pi;
        }

        if (y > 0m)
        {
            return Pi / Two;
        }

        if (y < 0m)
        {
            return -Pi / Two;
        }

        return 0m;
    }

    private static decimal ReduceAngle(decimal x)
    {
        decimal twoPi = Two * Pi;
        decimal periodCount = Math.Round(x / twoPi, 0, MidpointRounding.AwayFromZero);
        return x - periodCount * twoPi;
    }

    private static decimal SinSeries(decimal x)
    {
        decimal xSquared = x * x;
        decimal term = x;
        decimal sum = x;
        for (int n = 1; n <= SeriesMaxTerms; n++)
        {
            term *= -xSquared / ((Two * n) * (Two * n + 1m));
            sum += term;
            if (Math.Abs(term) < ConvergenceTolerance)
            {
                break;
            }
        }

        return sum;
    }

    private static decimal CosSeries(decimal x)
    {
        decimal xSquared = x * x;
        decimal term = 1m;
        decimal sum = 1m;
        for (int n = 1; n <= SeriesMaxTerms; n++)
        {
            term *= -xSquared / ((Two * n - 1m) * (Two * n));
            sum += term;
            if (Math.Abs(term) < ConvergenceTolerance)
            {
                break;
            }
        }

        return sum;
    }

    private static decimal SeedFromAsinDouble(double seed)
    {
        return double.IsFinite(seed) ? (decimal)seed : 0m;
    }
}
