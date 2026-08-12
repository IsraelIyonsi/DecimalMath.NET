using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Exponential;

public class ExpTests
{
    [Fact]
    public void ExpOfZeroIsOne()
    {
        Assert.Equal(1m, DecimalMath.Exp(0m));
    }

    [Theory]
    [MemberData(nameof(ReferenceCases))]
    public void MatchesDoubleReference(decimal input)
    {
        decimal actual = DecimalMath.Exp(input);
        decimal expected = (decimal)Math.Exp((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> ReferenceCases()
    {
        yield return new object[] { 1m };
        yield return new object[] { -1m };
        yield return new object[] { 2m };
        yield return new object[] { -2m };
        yield return new object[] { 0.5m };
        yield return new object[] { 10m };
        yield return new object[] { -10m };
        yield return new object[] { 0.0001m };
        yield return new object[] { 15.75m };
    }

    [Fact]
    public void ExpOfOneMatchesEulersNumber()
    {
        ApproximateEqualityAssert.WithinTolerance(DecimalMath.E, DecimalMath.Exp(1m), TestTolerances.DecimalIdentityTolerance);
    }
}
