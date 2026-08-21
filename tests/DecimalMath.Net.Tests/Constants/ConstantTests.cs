using DecimalMath.Tests.TestSupport;

namespace DecimalMath.Tests.Constants;

public class ConstantTests
{
    [Fact]
    public void PiMatchesDoubleReference()
    {
        ApproximateEqualityAssert.WithinRelativeTolerance((decimal)Math.PI, DecimalMathOps.Pi, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    [Fact]
    public void EMatchesDoubleReference()
    {
        ApproximateEqualityAssert.WithinRelativeTolerance((decimal)Math.E, DecimalMathOps.E, TestTolerances.DoubleOracleRelativeTolerance, TestTolerances.DoubleOracleAbsoluteFloor);
    }

    [Fact]
    public void PiIsBetweenThreePointOneFourAndThreePointOneFive()
    {
        Assert.True(DecimalMathOps.Pi > 3.14m && DecimalMathOps.Pi < 3.15m);
    }

    [Fact]
    public void EIsBetweenTwoPointSevenOneAndTwoPointSevenTwo()
    {
        Assert.True(DecimalMathOps.E > 2.71m && DecimalMathOps.E < 2.72m);
    }
}
