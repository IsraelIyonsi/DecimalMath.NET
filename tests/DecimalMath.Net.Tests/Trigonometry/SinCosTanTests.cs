using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Trigonometry;

public class SinCosTanTests
{
    [Fact]
    public void SinOfZeroIsZero()
    {
        Assert.Equal(0m, DecimalMathOps.Sin(0m));
    }

    [Fact]
    public void CosOfZeroIsOne()
    {
        Assert.Equal(1m, DecimalMathOps.Cos(0m));
    }

    [Fact]
    public void SinOfPiOverTwoIsOne()
    {
        ApproximateEqualityAssert.WithinTolerance(1m, DecimalMathOps.Sin(DecimalMathOps.Pi / 2m), TestTolerances.DecimalIdentityTolerance);
    }

    [Fact]
    public void CosOfPiOverTwoIsZero()
    {
        ApproximateEqualityAssert.WithinTolerance(0m, DecimalMathOps.Cos(DecimalMathOps.Pi / 2m), TestTolerances.DecimalIdentityTolerance);
    }

    [Fact]
    public void SinOfPiIsZero()
    {
        ApproximateEqualityAssert.WithinTolerance(0m, DecimalMathOps.Sin(DecimalMathOps.Pi), TestTolerances.DecimalIdentityTolerance);
    }

    [Fact]
    public void CosOfPiIsMinusOne()
    {
        ApproximateEqualityAssert.WithinTolerance(-1m, DecimalMathOps.Cos(DecimalMathOps.Pi), TestTolerances.DecimalIdentityTolerance);
    }

    [Fact]
    public void TanOfPiOverFourIsOne()
    {
        ApproximateEqualityAssert.WithinTolerance(1m, DecimalMathOps.Tan(DecimalMathOps.Pi / 4m), TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [MemberData(nameof(AngleCases))]
    public void SinMatchesDoubleReference(decimal angle)
    {
        decimal actual = DecimalMathOps.Sin(angle);
        decimal expected = (decimal)Math.Sin((double)angle);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    [Theory]
    [MemberData(nameof(AngleCases))]
    public void CosMatchesDoubleReference(decimal angle)
    {
        decimal actual = DecimalMathOps.Cos(angle);
        decimal expected = (decimal)Math.Cos((double)angle);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> AngleCases()
    {
        yield return new object[] { 0.5m };
        yield return new object[] { 1m };
        yield return new object[] { 2m };
        yield return new object[] { -1.5m };
        yield return new object[] { 10m };
        yield return new object[] { -10m };
        yield return new object[] { 100m };
        yield return new object[] { 0.0001m };
    }

    [Fact]
    public void TanNearPiOverTwoIsVeryLargeInMagnitude()
    {
        const decimal LargeMagnitudeThreshold = 1_000_000_000_000m;
        decimal tanNearAsymptote = DecimalMathOps.Tan(DecimalMathOps.Pi / 2m);
        Assert.True(Math.Abs(tanNearAsymptote) > LargeMagnitudeThreshold);
    }
}
