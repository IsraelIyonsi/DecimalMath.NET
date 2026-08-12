namespace DecimalMath.Tests.Allocation;

public class AllocateTests
{
    [Theory]
    [MemberData(nameof(WeightedCases))]
    public void SplitsProportionallyToWeights(decimal amount, decimal[] weights, decimal[] expected)
    {
        IReadOnlyList<decimal> actual = DecimalAllocator.Allocate(amount, weights);
        Assert.Equal(expected, actual);
    }

    public static IEnumerable<object[]> WeightedCases()
    {
        yield return new object[] { 100m, new[] { 1m, 1m, 1m }, new[] { 33.34m, 33.33m, 33.33m } };
        yield return new object[] { 100m, new[] { 1m, 2m, 3m }, new[] { 16.67m, 33.33m, 50.00m } };
        yield return new object[] { 10m, new[] { 1m, 0m, 1m }, new[] { 5.00m, 0.00m, 5.00m } };
        yield return new object[] { 100m, new[] { 1m, 0m, 0m }, new[] { 100.00m, 0.00m, 0.00m } };
        yield return new object[] { 50m, new[] { 3m, 1m }, new[] { 37.50m, 12.50m } };
    }

    [Theory]
    [MemberData(nameof(SumsExactlyCases))]
    public void PartsAlwaysSumToInputAmount(decimal amount, decimal[] weights)
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(amount, weights);
        decimal sum = 0m;
        foreach (decimal part in parts)
        {
            sum += part;
        }

        Assert.Equal(amount, sum);
    }

    public static IEnumerable<object[]> SumsExactlyCases()
    {
        yield return new object[] { 0.01m, new[] { 1m, 1m, 1m } };
        yield return new object[] { 0.01m, new[] { 1m, 2m, 7m } };
        yield return new object[] { 100.00m, new[] { 7m, 11m, 3m, 1m } };
        yield return new object[] { 99.99m, new[] { 1m, 1m, 1m, 1m, 1m, 1m, 1m } };
        yield return new object[] { 0.03m, new[] { 1m, 1m, 1m } };
        yield return new object[] { 1000000.01m, new[] { 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m, 1m } };
        yield return new object[] { -0.01m, new[] { 1m, 1m, 1m } };
        yield return new object[] { -100.00m, new[] { 1m, 2m, 3m } };
        yield return new object[] { 0.00m, new[] { 1m, 1m, 1m } };
    }

    [Fact]
    public void PennyAcrossThreePartsGivesFirstPartTheExtraPenny()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(0.01m, new[] { 1m, 1m, 1m });
        Assert.Equal(new[] { 0.01m, 0.00m, 0.00m }, parts);
    }

    [Fact]
    public void NegativeAmountAllocatesNegativeParts()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(-0.01m, new[] { 1m, 1m, 1m });
        Assert.Equal(new[] { -0.01m, 0.00m, 0.00m }, parts);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(-10)]
    [InlineData(0)]
    public void SinglePartReceivesTheEntireAmountRegardlessOfWeight(decimal amount)
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(amount, new[] { 0m });
        Assert.Equal(new[] { amount }, parts);
    }

    [Fact]
    public void ZeroAmountAllocatesAllZeroParts()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(0m, new[] { 1m, 5m, 10m });
        Assert.Equal(new[] { 0.00m, 0.00m, 0.00m }, parts);
    }

    [Fact]
    public void RespectsCustomDecimalPlaces()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(1m, new[] { 1m, 1m, 1m }, decimalPlaces: 4);
        Assert.Equal(new[] { 0.3334m, 0.3333m, 0.3333m }, parts);
        Assert.Equal(1m, parts[0] + parts[1] + parts[2]);
    }

    [Fact]
    public void ThrowsWhenWeightsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => DecimalAllocator.Allocate(100m, null!));
    }

    [Fact]
    public void ThrowsWhenWeightsIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => DecimalAllocator.Allocate(100m, Array.Empty<decimal>()));
    }

    [Fact]
    public void ThrowsWhenAnyWeightIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalAllocator.Allocate(100m, new[] { 1m, -1m }));
    }

    [Fact]
    public void ThrowsWhenAllWeightsAreZeroWithMultipleParts()
    {
        Assert.Throws<ArgumentException>(() => DecimalAllocator.Allocate(100m, new[] { 0m, 0m }));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(29)]
    public void ThrowsForOutOfRangeDecimalPlaces(int decimalPlaces)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => DecimalAllocator.Allocate(100m, new[] { 1m, 1m }, decimalPlaces));
    }

    [Fact]
    public void ThrowsWhenAmountHasMoreDecimalPlacesThanRequested()
    {
        // 10.555 has three decimal places but decimalPlaces defaults to two:
        // no set of cent-rounded parts can sum back to exactly 10.555 without
        // either losing or manufacturing half a cent.
        Assert.Throws<ArgumentException>(() => DecimalAllocator.Allocate(10.555m, new[] { 1m, 1m }));
    }

    [Fact]
    public void ThrowsWhenAmountHasMoreDecimalPlacesThanRequestedEvenWithSinglePart()
    {
        Assert.Throws<ArgumentException>(() => DecimalAllocator.Allocate(10.555m, new[] { 1m }));
    }

    [Fact]
    public void DoesNotThrowWhenAmountIsExactlyRepresentableAtDecimalPlaces()
    {
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(10.550m, new[] { 1m, 1m });
        Assert.Equal(new[] { 5.28m, 5.27m }, parts);
    }

    [Fact]
    public void LargeAmountAndLargeWeightsDoNotOverflow()
    {
        // amount * weights[i] would overflow decimal before the division by
        // totalWeight ever runs, even though every individual share is well
        // within decimal's range once the weights are normalized first.
        IReadOnlyList<decimal> parts = DecimalAllocator.Allocate(10_000_000_000m, new[] { 1e19m, 1e19m });
        Assert.Equal(new[] { 5_000_000_000m, 5_000_000_000m }, parts);
    }
}
