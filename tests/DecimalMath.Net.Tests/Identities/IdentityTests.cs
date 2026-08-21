using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Identities;

public class IdentityTests
{
    [Theory]
    [MemberData(nameof(PositiveValues))]
    public void ExpOfLnReturnsOriginalValue(decimal x)
    {
        decimal roundTripped = DecimalMathOps.Exp(DecimalMathOps.Ln(x));
        ApproximateEqualityAssert.WithinTolerance(x, roundTripped, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(ExpSafeValues))]
    public void LnOfExpReturnsOriginalValue(decimal x)
    {
        decimal roundTripped = DecimalMathOps.Ln(DecimalMathOps.Exp(x));
        ApproximateEqualityAssert.WithinTolerance(x, roundTripped, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(NonNegativeValues))]
    public void SqrtSquaredReturnsOriginalValue(decimal x)
    {
        decimal root = DecimalMathOps.Sqrt(x);
        decimal squared = root * root;
        ApproximateEqualityAssert.WithinTolerance(x, squared, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(NonNegativeValues))]
    public void PowOneHalfEqualsSqrt(decimal x)
    {
        decimal viaPow = DecimalMathOps.Pow(x, 0.5m);
        decimal viaSqrt = DecimalMathOps.Sqrt(x);
        ApproximateEqualityAssert.WithinTolerance(viaSqrt, viaPow, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(RealValues))]
    public void CbrtCubedReturnsOriginalValue(decimal x)
    {
        decimal root = DecimalMathOps.Cbrt(x);
        decimal cubed = root * root * root;
        ApproximateEqualityAssert.WithinTolerance(x, cubed, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(AngleValues))]
    public void SinSquaredPlusCosSquaredEqualsOne(decimal angle)
    {
        decimal sin = DecimalMathOps.Sin(angle);
        decimal cos = DecimalMathOps.Cos(angle);
        decimal sumOfSquares = sin * sin + cos * cos;
        ApproximateEqualityAssert.WithinTolerance(1m, sumOfSquares, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(NarrowAngleValues))]
    public void AsinOfSinReturnsOriginalAngle(decimal angle)
    {
        decimal roundTripped = DecimalMathOps.Asin(DecimalMathOps.Sin(angle));
        ApproximateEqualityAssert.WithinTolerance(angle, roundTripped, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(NarrowAngleValues))]
    public void AtanOfTanReturnsOriginalAngle(decimal angle)
    {
        decimal roundTripped = DecimalMathOps.Atan(DecimalMathOps.Tan(angle));
        ApproximateEqualityAssert.WithinTolerance(angle, roundTripped, TestTolerances.DecimalIdentityTolerance);
    }

    public static IEnumerable<object[]> PositiveValues()
    {
        yield return new object[] { 1m };
        yield return new object[] { 2m };
        yield return new object[] { 0.5m };
        yield return new object[] { 100m };
        yield return new object[] { 0.001m };
        yield return new object[] { 9999.999m };
    }

    // Bounded to |x| <= 10 so Exp(x) stays well clear of the point where a
    // System.Decimal's fixed 28-digit scale starts eroding significant digits
    // for very-small-magnitude results (Exp(-50) is representable but retains
    // only a handful of significant digits, which is a Decimal limitation, not
    // an algorithm error). Money math never needs exponents outside this range.
    public static IEnumerable<object[]> ExpSafeValues()
    {
        yield return new object[] { 0m };
        yield return new object[] { 0.5m };
        yield return new object[] { 1m };
        yield return new object[] { 2m };
        yield return new object[] { 5m };
        yield return new object[] { 10m };
        yield return new object[] { -0.5m };
        yield return new object[] { -1m };
        yield return new object[] { -2m };
        yield return new object[] { -5m };
        yield return new object[] { -10m };
    }

    public static IEnumerable<object[]> NonNegativeValues()
    {
        yield return new object[] { 0m };
        yield return new object[] { 2m };
        yield return new object[] { 50m };
        yield return new object[] { 0.25m };
        yield return new object[] { 123456.789m };
    }

    public static IEnumerable<object[]> RealValues()
    {
        yield return new object[] { 0m };
        yield return new object[] { 8m };
        yield return new object[] { -8m };
        yield return new object[] { 50m };
        yield return new object[] { -50m };
    }

    public static IEnumerable<object[]> AngleValues()
    {
        yield return new object[] { 0m };
        yield return new object[] { 0.5m };
        yield return new object[] { 1m };
        yield return new object[] { -2m };
        yield return new object[] { 10m };
        yield return new object[] { -100m };
    }

    public static IEnumerable<object[]> NarrowAngleValues()
    {
        yield return new object[] { 0m };
        yield return new object[] { 0.5m };
        yield return new object[] { -0.5m };
        yield return new object[] { 1m };
        yield return new object[] { -1m };
        yield return new object[] { 1.3m };
    }
}
