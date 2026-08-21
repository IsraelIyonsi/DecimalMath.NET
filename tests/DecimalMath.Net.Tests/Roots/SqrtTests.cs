using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Roots;

public class SqrtTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(4, 2)]
    [InlineData(9, 3)]
    [InlineData(16, 4)]
    [InlineData(0.25, 0.5)]
    [InlineData(100, 10)]
    [InlineData(0.0001, 0.01)]
    [InlineData(1000000, 1000)]
    public void ReturnsExactValueForPerfectSquares(decimal input, decimal expected)
    {
        decimal actual = DecimalMathOps.Sqrt(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(IrrationalCases))]
    public void MatchesDoubleReferenceForIrrationalInputs(decimal input)
    {
        decimal actual = DecimalMathOps.Sqrt(input);
        decimal expected = (decimal)Math.Sqrt((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> IrrationalCases()
    {
        yield return new object[] { 2m };
        yield return new object[] { 3m };
        yield return new object[] { 5m };
        yield return new object[] { 1234.5678m };
        yield return new object[] { 0.00073m };
        yield return new object[] { 987654321.123m };
        yield return new object[] { 0.5m };
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.0001)]
    [InlineData(-1000000)]
    public void ThrowsForNegativeInput(decimal input)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Sqrt(input));
    }
}
