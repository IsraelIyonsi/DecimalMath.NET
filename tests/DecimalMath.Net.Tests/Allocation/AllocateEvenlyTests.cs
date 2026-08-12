namespace DecimalMath.Tests.Allocation;

public class AllocateEvenlyTests
{
    [Theory]
    [MemberData(nameof(EvenSplitCases))]
    public void SplitsIntoEqualPartsWithRemainderToEarliestIndices(decimal amount, int count, decimal[] expected)
    {
        IReadOnlyList<decimal> actual = DecimalAllocator.AllocateEvenly(amount, count);
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> EvenSplitCases()
    {
        yield return new object[] { 100m, 4, new[] { 25.00m, 25.00m, 25.00m, 25.00m } };
        yield return new object[] { 100m, 3, new[] { 33.34m, 33.33m, 33.33m } };
        yield return new object[] { 0.01m, 3, new[] { 0.01m, 0.00m, 0.00m } };
        yield return new object[] { 10m, 1, new[] { 10.00m } };
        yield return new object[] { -10m, 3, new[] { -3.34m, -3.33m, -3.33m } };
    }

    [Theory]
    [MemberData(nameof(CountsAndAmounts))]
    public void PartsAlwaysSumToInputAmount(decimal amount, int count)
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.AllocateEvenly(amount, count);
        decimal sum = 0m;
        foreach (decimal part in parts)
        {
            sum += part;
        }

        Assert.Equal(amount, sum);
    }

    public static IEnumerable<object[]> CountsAndAmounts()
    {
        yield return new object[] { 100m, 7 };
        yield return new object[] { 0.05m, 6 };
        yield return new object[] { 1000000.01m, 13 };
        yield return new object[] { -55.55m, 4 };
        yield return new object[] { 0.02m, 100 };
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ThrowsForNonPositiveCount(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalAllocator.AllocateEvenly(100m, count));
    }

    [Fact]
    public void ThrowsWhenAmountHasMoreDecimalPlacesThanRequested()
    {
        // 0.015 has three decimal places but decimalPlaces defaults to two:
        // splitting it into cent-rounded parts that sum back to exactly 0.015
        // is mathematically impossible.
        Assert.Throws<ArgumentException>(() => DecimalAllocator.AllocateEvenly(0.015m, 2));
    }

    [Fact]
    public void ThrowsForAnotherSubQuantumAmount()
    {
        Assert.Throws<ArgumentException>(() => DecimalAllocator.AllocateEvenly(0.019m, 2));
    }
}
