namespace DecimalMath.Tests.Allocation;

public class AllocationAdversarialTests
{
    [Fact]
    public void PennyAcrossThreePartsSumsExactly()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.AllocateEvenly(0.01m, 3);
        AssertSumsTo(0.01m, parts);
    }

    [Fact]
    public void NegativePennyAcrossThreePartsSumsExactly()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.AllocateEvenly(-0.01m, 3);
        AssertSumsTo(-0.01m, parts);
    }

    [Fact]
    public void SomeZeroWeightsStillSumExactly()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(1000.01m, new[] { 5m, 0m, 3m, 0m, 2m });
        AssertSumsTo(1000.01m, parts);
        Assert.Equal(0.00m, parts[1]);
        Assert.Equal(0.00m, parts[3]);
    }

    [Fact]
    public void SinglePartSumsExactlyRegardlessOfWeight()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(12345.67m, new[] { 999m });
        AssertSumsTo(12345.67m, parts);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(17)]
    [InlineData(101)]
    public void OneCentSplitAcrossManyPartsAlwaysSumsExactly(int count)
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.AllocateEvenly(0.01m, count);
        AssertSumsTo(0.01m, parts);
    }

    [Theory]
    [InlineData(-0.03)]
    [InlineData(-1234.56)]
    [InlineData(-1000000.01)]
    public void LargeNegativeAmountsSumExactly(decimal amount)
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(amount, new[] { 1m, 3m, 5m, 7m });
        AssertSumsTo(amount, parts);
    }

    [Fact]
    public void HighlyUnevenWeightsStillSumExactly()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(0.07m, new[] { 1m, 1000000m, 1m });
        AssertSumsTo(0.07m, parts);
    }

    private static void AssertSumsTo(decimal expectedTotal, IReadOnlyList<decimal> parts)
    {
        decimal sum = 0m;
        foreach (decimal part in parts)
        {
            sum += part;
        }

        Assert.Equal(expectedTotal, sum);
    }
}
