# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-08-12

### Added

- `DecimalMath` static class: `Sqrt`, `Cbrt`, `Pow`, `Exp`, `Ln`, `Log10`, `Log2`, `Log(x, newBase)`, and the constants `Pi` and `E`.
- Trigonometry on `DecimalMath`: `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Atan2`.
- `DecimalAllocator` static class: `Allocate(amount, weights)` and `AllocateEvenly(amount, count)`, both using the largest-remainder method so the returned parts always sum to exactly the input amount, and `Prorate(amount, unitsUsed, totalUnits)`.
- `Sqrt` and `Cbrt` computed via Newton's method; `Exp` and `Ln` via range reduction plus power series; `Sin`, `Cos`, `Asin` via power series and Newton's method; `Atan` and `Atan2` composed from `Asin`. All converge until successive terms differ by less than `1e-28`, the smallest positive value `decimal` can represent.
- `Pi` and `E` computed once at type initialization from convergent series (Machin's formula and `e = sum(1/n!)`), not hard-coded digit strings; accurate to within a relative error on the order of `1e-27`.
- `DecimalAllocator.Allocate` and `AllocateEvenly` throw `ArgumentException` when `amount` carries more decimal places than `decimalPlaces`, since no set of parts can then both round correctly and sum back to exactly `amount`; and normalize weights by their total before multiplying by `amount`, so large amounts paired with large weights no longer overflow `decimal` despite every individual share being in range.
- Verified against exact results for inputs with an algebraically exact answer, against `System.Math`'s `double` implementations as a reference oracle, against decimal-precision algebraic identities (`sqrt(x)^2 == x`, `exp(ln(x)) == x`, `pow(x, 0.5) == sqrt(x)`, `sin(x)^2 + cos(x)^2 == 1`), and against 28-digit reference fixtures for `Pi`, `E`, `Sqrt(2)`, `Ln(2)`, `Ln(10)`, `Exp(1)`, `Exp(5)` and known trig points computed independently via arbitrary-precision (`BigInteger` fixed-point) arithmetic.
- Allocation verified exact-sum for adversarial cases: a single cent split across many parts, negative amounts, weights of zero, a single part, and large amounts paired with large weights.
- Zero runtime dependencies.
- SourceLink (GitHub), deterministic CI builds and `.snupkg` symbol packages.
