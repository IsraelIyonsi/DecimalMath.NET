using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Powers;

public class PowTests
{
    [Theory]
    [InlineData(2, 0, 1)]
    [InlineData(0, 0, 1)]
    [InlineData(5, 1, 5)]
    [InlineData(2, 10, 1024)]
    [InlineData(2, -1, 0.5)]
    [InlineData(2, -2, 0.25)]
    [InlineData(10, 3, 1000)]
    [InlineData(0, 5, 0)]
    [InlineData(-2, 2, 4)]
    [InlineData(-2, 3, -8)]
    [InlineData(-3, 4, 81)]
    [InlineData(1.5, 2, 2.25)]
    [InlineData(1, 1000000, 1)]
    public void ReturnsExactValueForIntegerExponents(decimal x, decimal y, decimal expected)
    {
        decimal actual = DecimalMathOps.Pow(x, y);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(NonIntegerExponentCases))]
    public void MatchesDoubleReferenceForNonIntegerExponents(decimal x, decimal y)
    {
        decimal actual = DecimalMathOps.Pow(x, y);
        decimal expected = (decimal)Math.Pow((double)x, (double)y);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> NonIntegerExponentCases()
    {
        yield return new object[] { 2m, 0.5m };
        yield return new object[] { 4m, 1.5m };
        yield return new object[] { 9m, 0.5m };
        yield return new object[] { 2m, 10.5m };
        yield return new object[] { 100m, 0.25m };
        yield return new object[] { 2.5m, 3.3m };
    }

    [Fact]
    public void MatchesSqrtForOneHalfExponent()
    {
        decimal[] inputs = { 2m, 3m, 100m, 0.5m, 12345.6789m };
        foreach (decimal x in inputs)
        {
            decimal viaPow = DecimalMathOps.Pow(x, 0.5m);
            decimal viaSqrt = DecimalMathOps.Sqrt(x);
            ApproximateEqualityAssert.WithinTolerance(viaSqrt, viaPow, TestTolerances.DecimalIdentityTolerance);
        }
    }

    [Fact]
    public void ThrowsWhenZeroRaisedToNegativePower()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Pow(0m, -1m));
    }

    [Fact]
    public void ThrowsWhenNegativeBaseRaisedToNonIntegerPower()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Pow(-4m, 0.5m));
    }
}
