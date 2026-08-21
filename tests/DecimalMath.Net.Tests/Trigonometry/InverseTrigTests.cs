using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Trigonometry;

public class InverseTrigTests
{
    [Theory]
    [MemberData(nameof(AsinAcosCases))]
    public void AsinMatchesDoubleReference(decimal input)
    {
        decimal actual = DecimalMathOps.Asin(input);
        decimal expected = (decimal)Math.Asin((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    [Theory]
    [MemberData(nameof(AsinAcosCases))]
    public void AcosMatchesDoubleReference(decimal input)
    {
        decimal actual = DecimalMathOps.Acos(input);
        decimal expected = (decimal)Math.Acos((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> AsinAcosCases()
    {
        yield return new object[] { 0m };
        yield return new object[] { 0.5m };
        yield return new object[] { -0.5m };
        yield return new object[] { 1m };
        yield return new object[] { -1m };
        yield return new object[] { 0.9999m };
        yield return new object[] { 0.1234m };
    }

    [Theory]
    [InlineData(1.1)]
    [InlineData(-1.1)]
    public void AsinThrowsOutsideDomain(decimal input)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Asin(input));
    }

    [Theory]
    [InlineData(1.1)]
    [InlineData(-1.1)]
    public void AcosThrowsOutsideDomain(decimal input)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalMathOps.Acos(input));
    }

    [Theory]
    [MemberData(nameof(AtanCases))]
    public void AtanMatchesDoubleReference(decimal input)
    {
        decimal actual = DecimalMathOps.Atan(input);
        decimal expected = (decimal)Math.Atan((double)input);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    public static IEnumerable<object[]> AtanCases()
    {
        yield return new object[] { 0m };
        yield return new object[] { 1m };
        yield return new object[] { -1m };
        yield return new object[] { 100m };
        yield return new object[] { -100m };
        yield return new object[] { 0.001m };
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, -1)]
    [InlineData(-1, -1)]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(0, -1)]
    [InlineData(1, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    public void Atan2MatchesDoubleReference(decimal y, decimal x)
    {
        decimal actual = DecimalMathOps.Atan2(y, x);
        decimal expected = (decimal)Math.Atan2((double)y, (double)x);
        ApproximateEqualityAssert.WithinRelativeTolerance(expected, actual, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }
}
