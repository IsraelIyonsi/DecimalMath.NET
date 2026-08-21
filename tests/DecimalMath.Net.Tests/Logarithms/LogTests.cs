using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Logarithms;

public class LogTests
{
    [Fact]
    public void LnOfOneIsZero()
    {
        Assert.Equal(0m, DecimalMathOps.Ln(1m));
    }

    [Theory]
    [MemberData(nameof(LnReferenceCases))]
    public void LnMatchesDoubleReference(decimal input)
    {
        decimal actual = DecimalMathOps.Ln(input);
        decimal expected = (decimal)Math.Log((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> LnReferenceCases()
    {
        yield return new object[] { 2m };
        yield return new object[] { 10m };
        yield return new object[] { 0.5m };
        yield return new object[] { 100m };
        yield return new object[] { 0.001m };
        yield return new object[] { 1234.5678m };
        yield return new object[] { 1000000m };
    }

    [Theory]
    [InlineData(100, 2)]
    [InlineData(1000, 3)]
    [InlineData(1, 0)]
    [InlineData(10, 1)]
    [InlineData(0.01, -2)]
    public void Log10ReturnsExactValueForPowersOfTen(decimal input, decimal expected)
    {
        decimal actual = DecimalMathOps.Log10(input);
        ApproximateEqualityAssert.WithinTolerance(expected, actual, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [InlineData(8, 3)]
    [InlineData(1024, 10)]
    [InlineData(1, 0)]
    [InlineData(0.5, -1)]
    public void Log2ReturnsExactValueForPowersOfTwo(decimal input, decimal expected)
    {
        decimal actual = DecimalMathOps.Log2(input);
        ApproximateEqualityAssert.WithinTolerance(expected, actual, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [InlineData(8, 2, 3)]
    [InlineData(81, 3, 4)]
    [InlineData(625, 5, 4)]
    [InlineData(1, 7, 0)]
    public void LogWithArbitraryBaseReturnsExactValue(decimal x, decimal newBase, decimal expected)
    {
        decimal actual = DecimalMathOps.Log(x, newBase);
        ApproximateEqualityAssert.WithinTolerance(expected, actual, TestTolerances.DecimalIdentityTolerance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void LnThrowsForZeroOrNegativeInput(decimal input)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Ln(input));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-2)]
    public void LogThrowsForInvalidBase(decimal invalidBase)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Log(10m, invalidBase));
    }
}
