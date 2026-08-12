using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Constants;

/// <summary>
/// Asserts transcendental results against high-precision reference values
/// embedded as fixtures, independent of this library's own series
/// computations. Each fixture below is a 28-decimal-place literal - the
/// maximum <see cref="decimal"/> can represent at that magnitude - taken from
/// an arbitrary-precision (BigInteger fixed-point, 60+ digit working
/// precision) reference computation, not from <see cref="DecimalMath"/>
/// itself. This is the layer that actually exercises digits 11 through 28:
/// <see cref="TestTolerances.DoubleOracleRelativeTolerance"/>, used elsewhere
/// against <see cref="System.Math"/> (double), only ever checks the first
/// ~10.
/// </summary>
public class ConstantFixtureTests
{
    [Fact]
    public void PiMatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("3.1415926535897932384626433833");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Pi, TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void EMatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("2.7182818284590452353602874714");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.E, TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void Sqrt2MatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("1.4142135623730950488016887242");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Sqrt(2m), TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void Ln2MatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("0.6931471805599453094172321215");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Ln(2m), TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void Ln10MatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("2.3025850929940456840179914547");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Ln(10m), TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void Exp1MatchesArbitraryPrecisionReference()
    {
        // Deliberately checked against the same 28-digit reference as E:
        // Exp(1) takes a different code path (range reduction through Ln2 plus
        // a Taylor series at the reduced remainder) from E's own direct
        // sum-of-1/n! series, so this doubles as a cross-check between the two.
        decimal reference = decimal.Parse("2.7182818284590452353602874714");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Exp(1m), TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void Exp5MatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("148.4131591025766034211155800406");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Exp(5m), TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void SinAtPiOverSixMatchesExactAlgebraicValue()
    {
        // sin(pi/6) = 1/2 exactly. Using DecimalMath.Pi (itself verified above)
        // to build the input keeps this an end-to-end check of the Sin series.
        decimal reference = 0.5m;
        decimal actual = DecimalMath.Sin(DecimalMath.Pi / 6m);
        ApproximateEqualityAssert.WithinTolerance(reference, actual, TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void CosAtPiOverThreeMatchesExactAlgebraicValue()
    {
        // cos(pi/3) = 1/2 exactly.
        decimal reference = 0.5m;
        decimal actual = DecimalMath.Cos(DecimalMath.Pi / 3m);
        ApproximateEqualityAssert.WithinTolerance(reference, actual, TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void SinAtPiOverFourMatchesArbitraryPrecisionReference()
    {
        // sin(pi/4) = sqrt(2)/2, referenced independently of DecimalMath.Sqrt.
        decimal reference = decimal.Parse("0.7071067811865475244008443621");
        decimal actual = DecimalMath.Sin(DecimalMath.Pi / 4m);
        ApproximateEqualityAssert.WithinTolerance(reference, actual, TestTolerances.HighPrecisionFixtureTolerance);
    }

    [Fact]
    public void AtanAtOneHalfMatchesArbitraryPrecisionReference()
    {
        decimal reference = decimal.Parse("0.4636476090008061162142562315");
        ApproximateEqualityAssert.WithinTolerance(reference, DecimalMath.Atan(0.5m), TestTolerances.HighPrecisionFixtureTolerance);
    }
}
