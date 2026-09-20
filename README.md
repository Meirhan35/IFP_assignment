# FunctionalDeliveryCalculator

Console app in C# that calculates a delivery price. It reads five values, checks them with `TryParse`, and applies the pricing rules in the required order.

## How to run

Requires the .NET SDK 8.0 or newer.

```bash
cd FunctionalDeliveryCalculator
dotnet run
```

Enter these values, one per line:

1. Base delivery price (number greater than 0, use `.` for decimals)
2. Number of items (whole number, 1 or more)
3. Express delivery (`true` or `false`)
4. Delivery type (`Pickup`, `Courier`, `DoorToDoor`)
5. Delivery zone (`City`, `OutsideCity`, `Remote`)

Example:

```
Base delivery price: 5000
Number of items: 5
Express delivery (true/false): true
Delivery type (Pickup, Courier, DoorToDoor): DoorToDoor
Delivery zone (City, OutsideCity, Remote): OutsideCity
Final delivery price: 10278.13
```

Input can also be piped: `printf "5000\n5\ntrue\nDoorToDoor\nOutsideCity\n" | dotnet run`

## Pricing rules

Order: base price → items → delivery type → zone → express → rounding to 2 decimals.

| Rule | Factor |
| --- | --- |
| 1–3 items / 4–7 items / 8+ items | ×1.00 / ×1.10 / ×1.20 |
| Pickup / Courier / DoorToDoor | ×0.80 / ×1.00 / ×1.15 |
| City / OutsideCity | ×1.00 / ×1.25 |
| Express | ×1.30 |

The `Remote` zone has no rule in the assignment, so it does not change the price. A price of 0 or less and fewer than 1 item are treated as invalid.

## Test cases

Input order: price, items, express, type, zone.

| # | Input | Expected result |
| --- | --- | --- |
| 1 | 5000, 2, false, Courier, City | `Final delivery price: 5000.00` |
| 2 | 5000, 5, false, Courier, City | `Final delivery price: 5500.00` |
| 3 | 5000, 9, false, Pickup, City | `Final delivery price: 4800.00` |
| 4 | 5000, 2, false, DoorToDoor, OutsideCity | `Final delivery price: 7187.50` |
| 5 | 5000, 3, true, Courier, City | `Final delivery price: 6500.00` |
| 6 | 5000, 5, true, DoorToDoor, OutsideCity | `Final delivery price: 10278.13` (10278.125 is rounded up) |
| 7 | price `-100` | `Error: Base price must be a number greater than 0 (and at most 1000000000).` |
| 8 | price `0` | same error as test 7 |
| 9 | price empty | same error as test 7 |
| 10 | no input at all (`null`) | same error as test 7 |
| 11 | price `5000`, items `abc` | `Error: Number of items must be a whole number, 1 or more.` |
| 12 | price `5000`, items `2`, express `maybe` | `Error: Express delivery must be true or false.` |
| 13 | ..., type `Drone` | `Error: Delivery type must be Pickup, Courier or DoorToDoor.` |
| 14 | ..., zone `Mars` | `Error: Delivery zone must be City, OutsideCity or Remote.` |

When the input is invalid, the program prints one error message and stops. It never crashes.

## Questions

**Which parts of your program handle user input and output?**
`Main` and the small `Ask` method. They are the only code that uses `Console`.

**Which functions perform only delivery price calculations?**
`ApplyRule`, `ItemsFactor`, `TypeFactor`, `ZoneFactor` and `CalculatePrice`. They only take values and return values. They do not use `Console` or global variables.

**How is Func<...> used to apply delivery pricing rules?**
In `CalculatePrice`, each rule is a `Func<decimal, decimal>` written as a lambda, for example `price => price * ZoneFactor(zone)`. The method `ApplyRule(price, rule)` takes such a function and runs it. The calculation is a chain of `ApplyRule` calls, and each call returns the new price for the next one.

**Why is TryParse useful when processing delivery data entered by the user?**
Users can type empty text, letters instead of numbers, or unknown values. `TryParse` does not throw an exception. It returns `false`, so the program can show a clear error message and stop normally.
