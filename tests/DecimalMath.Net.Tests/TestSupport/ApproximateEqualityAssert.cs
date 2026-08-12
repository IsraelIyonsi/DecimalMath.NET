namespace DecimalMath.Tests.TestSupport;

internal static class ApproximateEqualityAssert
{
    public static void WithinTolerance(decimal expected, decimal actual, decimal tolerance)
    {
        decimal difference = Math.Abs(expected - actual);
        Assert.True(
            difference <= tolerance,
            $"Expected {actual} to be within {tolerance} of {expected}, but differed by {difference}.");
    }

    /// <summary>
    /// Asserts that <paramref name="actual"/> is close to <paramref name="expected"/>
    /// within a tolerance proportional to the magnitude of <paramref name="expected"/>,
    /// falling back to <paramref name="absoluteFloor"/> when that magnitude is small.
    /// Appropriate when comparing against a <see cref="double"/> reference, whose own
    /// precision is relative rather than absolute.
    /// </summary>
    public static void WithinRelativeTolerance(decimal expected, decimal actual, decimal relativeTolerance, decimal absoluteFloor)
    {
        decimal magnitudeBasedTolerance = Math.Abs(expected) * relativeTolerance;
        decimal tolerance = magnitudeBasedTolerance > absoluteFloor ? magnitudeBasedTolerance : absoluteFloor;
        WithinTolerance(expected, actual, tolerance);
    }
}
