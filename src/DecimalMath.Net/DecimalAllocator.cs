namespace DecimalMath;

/// <summary>
/// Penny-exact allocation of a decimal amount into weighted or evenly-sized
/// parts, using the largest-remainder method. Every method in this class
/// guarantees that the returned parts sum to exactly the input amount: no cent
/// is ever lost to rounding, and no cent is ever manufactured.
/// </summary>
/// <remarks>
/// That guarantee only holds when <c>amount</c> is itself representable at
/// <c>decimalPlaces</c> (e.g. a two-decimal-place amount when allocating to
/// cents). An amount with a smaller unit than <c>decimalPlaces</c> -
/// <c>Allocate(10.555m, weights, decimalPlaces: 2)</c>, say - cannot be split
/// into cent-rounded parts that also sum back to exactly 10.555; there is no
/// way to both round every part and lose nothing in between. Rather than
/// silently manufacture or drop that sub-cent fraction, <see cref="Allocate"/>
/// and <see cref="AllocateEvenly"/> throw <see cref="ArgumentException"/> in
/// that case. Round the amount to <c>decimalPlaces</c> first, or pass a
/// <c>decimalPlaces</c> at least as large as the amount's own scale.
/// </remarks>
public static class DecimalAllocator
{
    /// <summary>
    /// The number of decimal places used for allocation and rounding when the
    /// caller does not specify one. Two, matching the minor unit of most
    /// currencies (cents, pence, kobo).
    /// </summary>
    public const int DefaultCurrencyDecimalPlaces = 2;

    private const int MinDecimalPlaces = 0;
    private const int MaxDecimalPlaces = 28;
    private const decimal Ten = 10m;

    /// <summary>
    /// Splits <paramref name="amount"/> into parts proportional to
    /// <paramref name="weights"/>. Each part is rounded down to
    /// <paramref name="decimalPlaces"/>, and the leftover smallest units are
    /// distributed one at a time, largest fractional remainder first, to
    /// indices earlier in <paramref name="weights"/> breaking ties. The
    /// returned parts always sum to exactly <paramref name="amount"/>.
    /// </summary>
    /// <param name="amount">The amount to split. May be negative; the sign is factored out and reapplied to every part.</param>
    /// <param name="weights">The non-negative weight of each part. At least one weight must be greater than zero when there is more than one part.</param>
    /// <param name="decimalPlaces">The number of decimal places to round each part to. Defaults to <see cref="DefaultCurrencyDecimalPlaces"/>.</param>
    /// <returns>Parts, in the same order as <paramref name="weights"/>, summing exactly to <paramref name="amount"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="weights"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="weights"/> is empty, every weight is zero while there is
    /// more than one part, or <paramref name="amount"/> has more decimal places
    /// than <paramref name="decimalPlaces"/> and so cannot be split into parts
    /// that both round to <paramref name="decimalPlaces"/> and sum back to
    /// exactly <paramref name="amount"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">A weight is negative, or <paramref name="decimalPlaces"/> is outside [0, 28].</exception>
    public static IReadOnlyList<decimal> Allocate(decimal amount, IReadOnlyList<decimal> weights, int decimalPlaces = DefaultCurrencyDecimalPlaces)
    {
        ValidateWeights(weights);
        ValidateDecimalPlaces(decimalPlaces);
        ValidateAmountIsRepresentable(amount, decimalPlaces);

        if (weights.Count == 1)
        {
            return new[] { amount };
        }

        int sign = amount < 0m ? -1 : 1;
        decimal absoluteAmount = Math.Abs(amount);
        decimal[] shares = AllocateNonNegative(absoluteAmount, weights, decimalPlaces);

        if (sign < 0)
        {
            for (int i = 0; i < shares.Length; i++)
            {
                shares[i] = -shares[i];
            }
        }

        return shares;
    }

    /// <summary>
    /// Splits <paramref name="amount"/> into <paramref name="count"/> equal
    /// parts using the same largest-remainder distribution as
    /// <see cref="Allocate"/>. The returned parts always sum to exactly
    /// <paramref name="amount"/>.
    /// </summary>
    /// <param name="amount">The amount to split. May be negative.</param>
    /// <param name="count">The number of equal parts, which must be greater than zero.</param>
    /// <param name="decimalPlaces">The number of decimal places to round each part to. Defaults to <see cref="DefaultCurrencyDecimalPlaces"/>.</param>
    /// <returns><paramref name="count"/> parts summing exactly to <paramref name="amount"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is not greater than zero, or <paramref name="decimalPlaces"/> is outside [0, 28].</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="amount"/> has more decimal places than <paramref name="decimalPlaces"/>
    /// and so cannot be split into parts that both round to <paramref name="decimalPlaces"/>
    /// and sum back to exactly <paramref name="amount"/>.
    /// </exception>
    public static IReadOnlyList<decimal> AllocateEvenly(decimal amount, int count, int decimalPlaces = DefaultCurrencyDecimalPlaces)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero.");
        }

        decimal[] equalWeights = new decimal[count];
        for (int i = 0; i < count; i++)
        {
            equalWeights[i] = 1m;
        }

        return Allocate(amount, equalWeights, decimalPlaces);
    }

    /// <summary>
    /// Computes the proportional share of <paramref name="amount"/> attributable
    /// to <paramref name="unitsUsed"/> out of <paramref name="totalUnits"/>,
    /// rounded to <paramref name="decimalPlaces"/>. Typical uses are prorating a
    /// subscription fee across days used in a billing period, or a rent amount
    /// across days occupied in a month.
    /// </summary>
    /// <param name="amount">The full-period amount to prorate. May be negative.</param>
    /// <param name="unitsUsed">The number of units (days, hours) actually used, between zero and <paramref name="totalUnits"/> inclusive.</param>
    /// <param name="totalUnits">The total number of units in the full period, which must be greater than zero.</param>
    /// <param name="decimalPlaces">The number of decimal places to round the result to. Defaults to <see cref="DefaultCurrencyDecimalPlaces"/>.</param>
    /// <returns>The proportional share of <paramref name="amount"/>, rounded to <paramref name="decimalPlaces"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="totalUnits"/> is not greater than zero, <paramref name="unitsUsed"/> is
    /// outside [0, totalUnits], or <paramref name="decimalPlaces"/> is outside [0, 28].
    /// </exception>
    public static decimal Prorate(decimal amount, int unitsUsed, int totalUnits, int decimalPlaces = DefaultCurrencyDecimalPlaces)
    {
        if (totalUnits <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalUnits), totalUnits, "Total units must be greater than zero.");
        }

        if (unitsUsed < 0 || unitsUsed > totalUnits)
        {
            throw new ArgumentOutOfRangeException(nameof(unitsUsed), unitsUsed, "Units used must be between zero and total units.");
        }

        ValidateDecimalPlaces(decimalPlaces);

        decimal exactShare = amount * unitsUsed / totalUnits;
        return Math.Round(exactShare, decimalPlaces, MidpointRounding.AwayFromZero);
    }

    private static decimal[] AllocateNonNegative(decimal amount, IReadOnlyList<decimal> weights, int decimalPlaces)
    {
        int count = weights.Count;
        decimal totalWeight = 0m;
        for (int i = 0; i < count; i++)
        {
            totalWeight += weights[i];
        }

        if (totalWeight <= 0m)
        {
            throw new ArgumentException("At least one weight must be greater than zero.", nameof(weights));
        }

        decimal scale = ScaleFor(decimalPlaces);
        decimal[] flooredShares = new decimal[count];
        decimal[] fractionalRemainders = new decimal[count];
        decimal flooredTotal = 0m;

        for (int i = 0; i < count; i++)
        {
            // Normalize the weight to a totalWeight-relative fraction before
            // multiplying by amount, rather than computing amount * weights[i]
            // first. weights[i] / totalWeight is always in [0, 1], so this
            // cannot overflow decimal's range even when amount and weights are
            // both large; amount * weights[i] can, despite every individual
            // share being well within range.
            decimal weightFraction = weights[i] / totalWeight;
            decimal exactScaledShare = amount * weightFraction * scale;
            decimal flooredScaledShare = Math.Floor(exactScaledShare);
            fractionalRemainders[i] = exactScaledShare - flooredScaledShare;
            flooredShares[i] = flooredScaledShare / scale;
            flooredTotal += flooredShares[i];
        }

        decimal remainderScaled = Math.Round((amount - flooredTotal) * scale, 0, MidpointRounding.AwayFromZero);

        // remainderScaled is mathematically guaranteed to land in [0, count]:
        // each floored share is within one smallest unit of its exact share,
        // so the total shortfall is within count smallest units. The clamp
        // below guards purely against a hypothetical rounding excursion at the
        // 28th significant decimal digit; it should never actually trigger.
        int remainderUnits = (int)Math.Clamp(remainderScaled, 0m, (decimal)count);

        int[] orderedIndices = OrderByDescendingRemainder(fractionalRemainders);
        decimal smallestUnit = 1m / scale;

        for (int i = 0; i < remainderUnits; i++)
        {
            flooredShares[orderedIndices[i]] += smallestUnit;
        }

        return flooredShares;
    }

    private static int[] OrderByDescendingRemainder(decimal[] remainders)
    {
        int[] indices = new int[remainders.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        Array.Sort(indices, (a, b) =>
        {
            int comparison = remainders[b].CompareTo(remainders[a]);
            return comparison != 0 ? comparison : a.CompareTo(b);
        });

        return indices;
    }

    private static decimal ScaleFor(int decimalPlaces)
    {
        decimal scale = 1m;
        for (int i = 0; i < decimalPlaces; i++)
        {
            scale *= Ten;
        }

        return scale;
    }

    private static void ValidateWeights(IReadOnlyList<decimal> weights)
    {
        if (weights is null)
        {
            throw new ArgumentNullException(nameof(weights));
        }

        if (weights.Count == 0)
        {
            throw new ArgumentException("At least one weight is required.", nameof(weights));
        }

        for (int i = 0; i < weights.Count; i++)
        {
            if (weights[i] < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(weights), weights[i], "Weights must not be negative.");
            }
        }
    }

    private static void ValidateDecimalPlaces(int decimalPlaces)
    {
        if (decimalPlaces < MinDecimalPlaces || decimalPlaces > MaxDecimalPlaces)
        {
            throw new ArgumentOutOfRangeException(nameof(decimalPlaces), decimalPlaces, "Decimal places must be between 0 and 28.");
        }
    }

    /// <summary>
    /// Guards the exact-sum guarantee at its source. If <paramref name="amount"/>
    /// carries more decimal places than <paramref name="decimalPlaces"/>, no set
    /// of parts rounded to <paramref name="decimalPlaces"/> can both look
    /// correctly rounded and sum back to exactly <paramref name="amount"/>; one
    /// of those two promises would have to be manufactured or lost. Rather than
    /// silently pick one, this fails fast.
    /// </summary>
    private static void ValidateAmountIsRepresentable(decimal amount, int decimalPlaces)
    {
        decimal roundedAmount = Math.Round(amount, decimalPlaces, MidpointRounding.AwayFromZero);
        if (roundedAmount != amount)
        {
            throw new ArgumentException(
                $"Amount {amount} has more decimal places than decimalPlaces ({decimalPlaces}) allows. " +
                "An exact split that both rounds to decimalPlaces and sums back to amount is not possible. " +
                $"Round the amount to {decimalPlaces} decimal places first, or pass a larger decimalPlaces.",
                nameof(amount));
        }
    }
}
