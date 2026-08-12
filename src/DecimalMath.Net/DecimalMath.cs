namespace DecimalMath;

/// <summary>
/// Decimal-precision math operations that the .NET base class library omits for
/// <see cref="decimal"/>: roots, powers, exponentials, logarithms, and (in the
/// partial trigonometry file) the trigonometric functions. Every operation stays
/// entirely within <see cref="decimal"/> arithmetic so results never round-trip
/// through <see cref="double"/> and lose the precision that makes
/// <see cref="decimal"/> suitable for money.
/// </summary>
/// <remarks>
/// Transcendental functions converge via Newton's method or power series until
/// successive terms differ by less than <c>1e-28</c>, the smallest positive
/// value <see cref="decimal"/> can represent. <see cref="Pi"/> and
/// <see cref="E"/> are computed once, at type initialization, from convergent
/// series rather than hard-coded digit strings.
/// <para>
/// "Maximum error" below is <em>relative</em> to the magnitude of the result,
/// not an absolute error bound: a result near 150 (e.g. <c>Exp(5)</c>) can
/// carry an absolute error a couple of orders of magnitude larger than a
/// result near 1, while still meeting the same relative precision. Series
/// truncation stops at a relative error on the order of 1e-27 to 1e-28;
/// residual error beyond that floor comes from per-operation rounding
/// inherent to <see cref="decimal"/> arithmetic, not from the series being
/// cut short.
/// </para>
/// </remarks>
public static partial class DecimalMath
{
    private const int SeriesMaxTerms = 300;
    private const int NewtonMaxIterations = 100;

    /// <summary>
    /// Series and Newton-iteration termination threshold: 1e-28, the smallest
    /// positive value representable at <see cref="decimal"/>'s maximum scale of
    /// 28. This is as tight as truncation-based convergence can be pushed;
    /// residual error beyond this floor comes from per-operation rounding in
    /// <see cref="decimal"/> arithmetic itself, not from stopping the series
    /// early.
    /// </summary>
    private const decimal ConvergenceTolerance = 1e-28m;
    private const decimal Two = 2m;
    private const decimal Three = 3m;

    private const decimal LnRangeLowerBound = 0.75m;
    private const decimal LnRangeUpperBound = 1.5m;

    /// <summary>
    /// The ratio of a circle's circumference to its diameter, accurate to
    /// within a relative error on the order of 1e-27 (the last one or two of
    /// <see cref="decimal"/>'s ~28-29 significant digits may differ from the
    /// true value). Computed once via Machin's formula
    /// (16 atan(1/5) - 4 atan(1/239)) and verified against a 40-digit
    /// arbitrary-precision reference; see <c>ConstantFixtureTests</c>.
    /// </summary>
    public static readonly decimal Pi = ComputePi();

    /// <summary>
    /// Euler's number, the base of the natural logarithm, accurate to within a
    /// relative error on the order of 1e-27 (the last one or two of
    /// <see cref="decimal"/>'s ~28-29 significant digits may differ from the
    /// true value). Computed once from the power series e = sum(1/n!) and
    /// verified against a 40-digit arbitrary-precision reference; see
    /// <c>ConstantFixtureTests</c>.
    /// </summary>
    public static readonly decimal E = ComputeE();

    private static readonly decimal Ln2 = ComputeLnSeries(Two);
    private static readonly decimal Ln10 = ComputeLn10();

    /// <summary>
    /// Computes the square root of <paramref name="x"/> using Newton's method,
    /// seeded from a fast <see cref="double"/> approximation and refined in
    /// <see cref="decimal"/> arithmetic until successive guesses agree to within
    /// the convergence tolerance.
    /// </summary>
    /// <param name="x">The non-negative value to take the square root of.</param>
    /// <returns>The square root of <paramref name="x"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is negative.</exception>
    public static decimal Sqrt(decimal x)
    {
        if (x < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "Square root is not defined for negative values.");
        }

        if (x == 0m)
        {
            return 0m;
        }

        decimal guess = SeedFromDouble(Math.Sqrt((double)x));

        for (int iteration = 0; iteration < NewtonMaxIterations; iteration++)
        {
            decimal nextGuess = (guess + x / guess) / Two;
            if (Math.Abs(nextGuess - guess) < ConvergenceTolerance)
            {
                return nextGuess;
            }

            guess = nextGuess;
        }

        return guess;
    }

    /// <summary>
    /// Computes the cube root of <paramref name="x"/> using Newton's method.
    /// Unlike <see cref="Sqrt"/>, this accepts negative input: the cube root of a
    /// negative number is the negation of the cube root of its magnitude.
    /// </summary>
    /// <param name="x">The value to take the cube root of.</param>
    /// <returns>The cube root of <paramref name="x"/>.</returns>
    public static decimal Cbrt(decimal x)
    {
        if (x == 0m)
        {
            return 0m;
        }

        decimal absoluteX = Math.Abs(x);
        decimal guess = SeedFromDouble(Math.Cbrt((double)absoluteX));

        for (int iteration = 0; iteration < NewtonMaxIterations; iteration++)
        {
            decimal guessSquared = guess * guess;
            decimal nextGuess = guess - (guessSquared * guess - absoluteX) / (Three * guessSquared);
            if (Math.Abs(nextGuess - guess) < ConvergenceTolerance)
            {
                guess = nextGuess;
                break;
            }

            guess = nextGuess;
        }

        return x < 0m ? -guess : guess;
    }

    /// <summary>
    /// Raises <paramref name="x"/> to the power <paramref name="y"/>.
    /// </summary>
    /// <remarks>
    /// Integer exponents (including negative ones) are computed exactly via
    /// exponentiation by squaring, so <c>Pow(x, 2)</c> is simply <c>x * x</c>
    /// with no series involved. Non-integer exponents require a positive base
    /// and are computed as <c>Exp(y * Ln(x))</c>.
    /// </remarks>
    /// <param name="x">The base.</param>
    /// <param name="y">The exponent.</param>
    /// <returns><paramref name="x"/> raised to the power <paramref name="y"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="x"/> is zero and <paramref name="y"/> is negative, or
    /// <paramref name="x"/> is negative and <paramref name="y"/> is not an integer.
    /// </exception>
    public static decimal Pow(decimal x, decimal y)
    {
        if (y == 0m)
        {
            return 1m;
        }

        if (x == 1m)
        {
            return 1m;
        }

        if (x == 0m)
        {
            if (y > 0m)
            {
                return 0m;
            }

            throw new ArgumentOutOfRangeException(nameof(y), y, "Zero cannot be raised to a negative power.");
        }

        bool exponentIsInteger = y == Math.Truncate(y);
        if (exponentIsInteger)
        {
            return PowInteger(x, y);
        }

        if (x < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "A negative base raised to a non-integer power is not a real number.");
        }

        return Exp(y * Ln(x));
    }

    /// <summary>
    /// Computes e raised to the power <paramref name="x"/> using range reduction
    /// (x = k*ln2 + r) followed by the Taylor series for e^r.
    /// </summary>
    /// <param name="x">The exponent.</param>
    /// <returns>e raised to the power <paramref name="x"/>.</returns>
    /// <exception cref="OverflowException">The result does not fit in a <see cref="decimal"/>.</exception>
    public static decimal Exp(decimal x)
    {
        if (x == 0m)
        {
            return 1m;
        }

        int reductionSteps = (int)Math.Round(x / Ln2, MidpointRounding.AwayFromZero);
        decimal remainder = x - reductionSteps * Ln2;

        decimal sum = 1m;
        decimal term = 1m;
        for (int n = 1; n <= SeriesMaxTerms; n++)
        {
            term *= remainder / n;
            sum += term;
            if (Math.Abs(term) < ConvergenceTolerance)
            {
                break;
            }
        }

        return reductionSteps >= 0
            ? sum * PowerOfTwo(reductionSteps)
            : sum / PowerOfTwo(-reductionSteps);
    }

    /// <summary>
    /// Computes the natural logarithm (base e) of <paramref name="x"/>.
    /// </summary>
    /// <param name="x">The positive value to take the logarithm of.</param>
    /// <returns>The natural logarithm of <paramref name="x"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is zero or negative.</exception>
    public static decimal Ln(decimal x)
    {
        if (x <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "Natural logarithm is only defined for positive values.");
        }

        if (x == 1m)
        {
            return 0m;
        }

        decimal reduced = x;
        int powerOfTwoCount = 0;
        while (reduced > LnRangeUpperBound)
        {
            reduced /= Two;
            powerOfTwoCount++;
        }

        while (reduced < LnRangeLowerBound)
        {
            reduced *= Two;
            powerOfTwoCount--;
        }

        return ComputeLnSeries(reduced) + powerOfTwoCount * Ln2;
    }

    /// <summary>
    /// Computes the base-10 logarithm of <paramref name="x"/>.
    /// </summary>
    /// <param name="x">The positive value to take the logarithm of.</param>
    /// <returns>The base-10 logarithm of <paramref name="x"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is zero or negative.</exception>
    public static decimal Log10(decimal x) => Ln(x) / Ln10;

    /// <summary>
    /// Computes the base-2 logarithm of <paramref name="x"/>.
    /// </summary>
    /// <param name="x">The positive value to take the logarithm of.</param>
    /// <returns>The base-2 logarithm of <paramref name="x"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is zero or negative.</exception>
    public static decimal Log2(decimal x) => Ln(x) / Ln2;

    /// <summary>
    /// Computes the logarithm of <paramref name="x"/> in the given
    /// <paramref name="newBase"/>.
    /// </summary>
    /// <param name="x">The positive value to take the logarithm of.</param>
    /// <param name="newBase">The positive logarithm base, which must not equal one.</param>
    /// <returns>The logarithm of <paramref name="x"/> in base <paramref name="newBase"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="x"/> is zero or negative, or <paramref name="newBase"/> is
    /// not positive or equals one.
    /// </exception>
    public static decimal Log(decimal x, decimal newBase)
    {
        if (newBase <= 0m || newBase == 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(newBase), newBase, "Logarithm base must be positive and not equal to one.");
        }

        return Ln(x) / Ln(newBase);
    }

    private static decimal PowInteger(decimal x, decimal integerExponent)
    {
        bool negativeExponent = integerExponent < 0m;
        decimal remainingExponent = Math.Abs(integerExponent);
        decimal baseValue = x;
        decimal result = 1m;

        while (remainingExponent > 0m)
        {
            if (remainingExponent % Two == 1m)
            {
                result *= baseValue;
            }

            remainingExponent = Math.Truncate(remainingExponent / Two);
            if (remainingExponent > 0m)
            {
                baseValue *= baseValue;
            }
        }

        return negativeExponent ? 1m / result : result;
    }

    private static decimal PowerOfTwo(int exponent)
    {
        decimal result = 1m;
        for (int i = 0; i < exponent; i++)
        {
            result *= Two;
        }

        return result;
    }

    private static decimal ComputeE()
    {
        decimal sum = 1m;
        decimal term = 1m;
        for (int n = 1; n <= SeriesMaxTerms; n++)
        {
            term /= n;
            sum += term;
            if (term < ConvergenceTolerance)
            {
                break;
            }
        }

        return sum;
    }

    private static decimal ComputeArcTanSeries(decimal x, int maxTerms)
    {
        decimal xSquared = x * x;
        decimal term = x;
        decimal sum = x;
        for (int n = 1; n <= maxTerms; n++)
        {
            term *= -xSquared;
            decimal denominator = Two * n + 1m;
            decimal add = term / denominator;
            sum += add;
            if (Math.Abs(add) < ConvergenceTolerance)
            {
                break;
            }
        }

        return sum;
    }

    private static decimal ComputePi()
    {
        const decimal MachinCoefficientOne = 16m;
        const decimal MachinCoefficientTwo = 4m;
        decimal arctanOneFifthInput = 1m / 5m;
        decimal arctanOneTwoThirtyNinthInput = 1m / 239m;

        decimal arctanOneFifth = ComputeArcTanSeries(arctanOneFifthInput, SeriesMaxTerms);
        decimal arctanOneTwoThirtyNinth = ComputeArcTanSeries(arctanOneTwoThirtyNinthInput, SeriesMaxTerms);

        return MachinCoefficientOne * arctanOneFifth - MachinCoefficientTwo * arctanOneTwoThirtyNinth;
    }

    private static decimal ComputeLnSeries(decimal y)
    {
        decimal z = (y - 1m) / (y + 1m);
        decimal zSquared = z * z;
        decimal term = z;
        decimal sum = z;
        for (int n = 1; n <= SeriesMaxTerms; n++)
        {
            term *= zSquared;
            decimal denominator = Two * n + 1m;
            decimal add = term / denominator;
            sum += add;
            if (Math.Abs(add) < ConvergenceTolerance)
            {
                break;
            }
        }

        return Two * sum;
    }

    private static decimal ComputeLn10()
    {
        // ln(5) = ln(4) + ln(1.25) = 2*ln(2) + ln(1.25). Routing through 1.25
        // (z = 1/9 in the underlying series) instead of 5 directly (z = 2/3)
        // needs far fewer series terms to converge, which means far less
        // accumulated per-term rounding in decimal arithmetic.
        const decimal OnePointTwoFive = 1.25m;
        decimal lnOnePointTwoFive = ComputeLnSeries(OnePointTwoFive);
        decimal lnFive = Two * Ln2 + lnOnePointTwoFive;
        return Ln2 + lnFive;
    }

    private static decimal SeedFromDouble(double seed)
    {
        return double.IsFinite(seed) && seed > 0d ? (decimal)seed : 1m;
    }
}
