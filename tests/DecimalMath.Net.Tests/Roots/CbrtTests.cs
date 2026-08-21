using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Roots;

public class CbrtTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(8, 2)]
    [InlineData(27, 3)]
    [InlineData(-8, -2)]
    [InlineData(-27, -3)]
    [InlineData(0.125, 0.5)]
    [InlineData(1000000, 100)]
    public void ReturnsExactValueForPerfectCubes(decimal input, decimal expected)
    {
        decimal actual = DecimalMathOps.Cbrt(input);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(IrrationalCases))]
    public void MatchesDoubleReferenceForIrrationalInputs(decimal input)
    {
        decimal actual = DecimalMathOps.Cbrt(input);
        decimal expected = (decimal)Math.Cbrt((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> IrrationalCases()
    {
        yield return new object[] { 2m };
        yield return new object[] { -2m };
        yield return new object[] { 100m };
        yield return new object[] { -100m };
        yield return new object[] { 0.001m };
        yield return new object[] { 123456.789m };
    }
}
