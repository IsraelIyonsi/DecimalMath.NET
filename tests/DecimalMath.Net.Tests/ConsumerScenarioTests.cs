using DecimalMath;

// Regression: the operations type must be reachable via 'using DecimalMath;' + simple name (namespace==class collision).
namespace ConsumerScenario;

public class ConsumerScenarioTests
{
    private const decimal SqrtComparisonTolerance = 1e-10m;

    [Fact]
    public void SqrtIsReachableBySimpleNameThroughUsingDirective()
    {
        decimal expected = (decimal)Math.Sqrt(2d);
        decimal actual = DecimalMathOps.Sqrt(2m);
        Assert.True(Math.Abs(actual - expected) < SqrtComparisonTolerance);
    }

    [Fact]
    public void PiConstantIsReachableBySimpleNameThroughUsingDirective()
    {
        Assert.True(DecimalMathOps.Pi > 3.14m && DecimalMathOps.Pi < 3.15m);
    }
}
