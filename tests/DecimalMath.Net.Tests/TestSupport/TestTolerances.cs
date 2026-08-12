namespace DecimalMath.Tests.TestSupport;

internal static class TestTolerances
{
    /// <summary>
    /// Relative tolerance used when cross-checking against <see cref="System.Math"/>
    /// (IEEE 754 double), which carries roughly 15-17 significant decimal
    /// digits. Expressed as a fraction of the expected value's magnitude
    /// because double's precision is relative, not absolute.
    /// </summary>
    public const decimal DoubleOracleRelativeTolerance = 1e-10m;

    /// <summary>
    /// Absolute floor applied alongside <see cref="DoubleOracleRelativeTolerance"/>
    /// so comparisons near zero (where relative tolerance collapses to nothing)
    /// still allow for double's own rounding.
    /// </summary>
    public const decimal DoubleOracleAbsoluteFloor = 1e-10m;

    /// <summary>
    /// Tolerance used for identities that are checked entirely in
    /// <see cref="decimal"/> arithmetic (no double round-trip), where the only
    /// error source is the library's own series truncation.
    /// </summary>
    public const decimal DecimalIdentityTolerance = 1e-20m;

    /// <summary>
    /// Tolerance used when checking transcendental results against
    /// arbitrary-precision reference fixtures (see
    /// <see cref="Constants.ConstantFixtureTests"/>): 1e-25, roughly two to
    /// three orders of magnitude looser than the library's actual observed
    /// error (on the order of 1e-27 to 1e-28) against those references, and
    /// fifteen orders of magnitude tighter than <see cref="DoubleOracleRelativeTolerance"/>.
    /// This is the tolerance that actually exercises digits 11 through 28 -
    /// the entire reason this library exists over <see cref="double"/>.
    /// </summary>
    public const decimal HighPrecisionFixtureTolerance = 1e-25m;
}
