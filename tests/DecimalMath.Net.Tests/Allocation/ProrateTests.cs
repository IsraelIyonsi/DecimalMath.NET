namespace DecimalMath.Tests.Allocation;

public class ProrateTests
{
    [Theory]
    [InlineData(100, 15, 30, 50.00)]
    [InlineData(100, 30, 30, 100.00)]
    [InlineData(100, 0, 30, 0.00)]
    [InlineData(30, 10, 30, 10.00)]
    [InlineData(9.99, 1, 3, 3.33)]
    public void ComputesProportionalShare(decimal amount, int unitsUsed, int totalUnits, decimal expected)
    {
        decimal actual = DecimalAllocator.Prorate(amount, unitsUsed, totalUnits);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NegativeAmountProratesNegatively()
    {
        decimal actual = DecimalAllocator.Prorate(-100m, 15, 30);
        Assert.Equal(-50.00m, actual);
    }

    [Fact]
    public void RespectsCustomDecimalPlaces()
    {
        decimal actual = DecimalAllocator.Prorate(10m, 1, 3, decimalPlaces: 4);
        Assert.Equal(3.3333m, actual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ThrowsForNonPositiveTotalUnits(int totalUnits)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalAllocator.Prorate(100m, 1, totalUnits));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(31)]
    public void ThrowsWhenUnitsUsedIsOutsideTotalUnitsRange(int unitsUsed)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalAllocator.Prorate(100m, unitsUsed, 30));
    }
}
