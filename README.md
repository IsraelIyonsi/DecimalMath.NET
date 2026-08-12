# DecimalMath.NET

Decimal-precision math for money: square and cube roots, powers, exponentials, logarithms, trigonometry, and penny-exact allocation, all operating on `System.Decimal` instead of `double`. Zero external dependencies.

`System.Math` only speaks `double`. The moment you call `Math.Sqrt`, `Math.Pow`, or `Math.Log` on a `decimal` amount, it round-trips through binary floating point and back, and you have quietly reintroduced the exact class of rounding bug that `decimal` exists to prevent. The one widely used package that filled this gap for .NET, DecimalEx, has not seen a functional release in years and its allocation helpers do not guarantee an exact-sum result. DecimalMath.NET is built for the common case that actually shows up in financial code: split an invoice into equal installments, prorate a subscription fee across a partial period, compute compound interest, without a single cent going missing or appearing from nowhere.

## Install

```
dotnet add package DecimalMath.Net
```

## Usage

### Split an invoice so every kobo is accounted for

```csharp
using DecimalMath;

decimal invoiceTotal = 100.00m;
IReadOnlyList<decimal> installments = DecimalAllocator.AllocateEvenly(invoiceTotal, count: 3);
// [33.34, 33.33, 33.33] -- sums to exactly 100.00, not 99.99
```

`Allocate` and `AllocateEvenly` use the largest-remainder method: every part is rounded down first, then the leftover cents are handed out one at a time to the parts with the largest fractional remainder. The result always sums to exactly the input amount, for any weights, any count, negative amounts included -- provided the amount is itself representable at the target `decimalPlaces` (a two-decimal-place amount when allocating to cents, say). An amount with a smaller unit than that, like `Allocate(10.555m, weights)` at the default two decimal places, cannot be split into cent-rounded parts that also sum back to exactly 10.555, so both methods throw `ArgumentException` rather than silently lose or manufacture the sub-cent fraction.

### Prorate a subscription fee

```csharp
using DecimalMath;

decimal monthlyFee = 29.99m;
int daysInMonth = 30;
int daysUsed = 12;

decimal proratedCharge = DecimalAllocator.Prorate(monthlyFee, daysUsed, daysInMonth);
// 11.996 rounded to 12.00
```

### Compound interest without a double round-trip

```csharp
using DecimalMath;

decimal principal = 5000.00m;
decimal annualRate = 0.045m;
int years = 10;

decimal futureValue = principal * DecimalMath.Pow(1m + annualRate, years);
// computed entirely in decimal arithmetic, no double in the pipeline
```

## What is in the box

Roots and powers: `Sqrt`, `Cbrt`, `Pow`.

Exponentials and logarithms: `Exp`, `Ln`, `Log10`, `Log2`, `Log(x, newBase)`, plus the constants `Pi` and `E`.

Trigonometry: `Sin`, `Cos`, `Tan`, `Asin`, `Acos`, `Atan`, `Atan2`.

Allocation: `DecimalAllocator.Allocate(amount, weights)`, `DecimalAllocator.AllocateEvenly(amount, count)`, `DecimalAllocator.Prorate(amount, unitsUsed, totalUnits)`.

## How the transcendental functions work

Every function stays in `decimal` arithmetic from input to output. `Sqrt` and `Cbrt` use Newton's method, seeded from a fast `double` approximation and refined in `decimal` until convergence. `Exp` and `Ln` use range reduction (powers of two) followed by a Taylor or Newton-friendly power series. `Sin`, `Cos`, `Asin`, `Atan` follow the same pattern, with `Atan` built on the identity `atan(x) = asin(x / sqrt(1 + x^2))` and `Atan2` composed from `Atan`. `Pi` and `E` are computed once, at load time, from convergent series (Machin's formula and `e = sum(1/n!)`) rather than hard-coded digit strings.

Every series and Newton iteration runs until successive terms differ by less than `1e-28`, the smallest positive value `decimal` can represent, so the result is accurate to within a relative error on the order of `1e-27` to `1e-28` -- the last one or two of `decimal`'s ~28-29 significant digits may differ from the true value; everything before that is exact. The test suite checks this four ways: exact results for inputs with an exact answer (perfect squares, integer powers, `sin(pi/2)`), cross-checks against `System.Math`'s `double` implementations as a reference oracle (good to `double`'s own ~15-17 digits), algebraic identities (`sqrt(x) squared == x`, `exp(ln(x)) == x`, `pow(x, 0.5) == sqrt(x)`, `sin(x)^2 + cos(x)^2 == 1`) verified entirely in `decimal` arithmetic, and -- the layer that actually exercises digits 11 through 28, the reason this library exists over `double` -- 28-digit reference fixtures for `Pi`, `E`, `Sqrt(2)`, `Ln(2)`, `Ln(10)`, `Exp(1)`, `Exp(5)` and known trig points, computed independently via arbitrary-precision (`BigInteger` fixed-point) arithmetic rather than by this library's own series.

## Zero dependencies, AOT-friendly

No runtime NuGet dependencies. No reflection, no dynamic code generation, nothing that needs a JIT. Every function is a static method built from arithmetic and loops, so the package works unmodified under Native AOT and trimming.

## License

MIT. See [LICENSE](LICENSE).
