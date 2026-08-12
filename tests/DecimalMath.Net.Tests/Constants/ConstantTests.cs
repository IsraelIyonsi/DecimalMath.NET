using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Constants;

public class ConstantTests
{
    [Fact]
    public void PiMatchesDoubleReference()
    {
        ApproximateEqualityAssert.WithinRelativeTolerance((decimal)Math.PI, DecimalMath.Pi, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    [Fact]
    public void EMatchesDoubleReference()
    {
        ApproximateEqualityAssert.WithinRelativeTolerance((decimal)Math.E, DecimalMath.E, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    [Fact]
    public void PiIsBetweenThreePointOneFourAndThreePointOneFive()
    {
        Assert.True(DecimalMath.Pi > 3.14m && DecimalMath.Pi < 3.15m);
    }

    [Fact]
    public void EIsBetweenTwoPointSevenOneAndTwoPointSevenTwo()
    {
        Assert.True(DecimalMath.E > 2.71m && DecimalMath.E < 2.72m);
    }
}
