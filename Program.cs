using System.Globalization;

enum DeliveryType
{
    Pickup,
    Courier,
    DoorToDoor
}

enum DeliveryZone
{
    City,
    OutsideCity,
    Remote
}

class Program
{
    static void Main()
    {
        if (!decimal.TryParse(Ask("Base delivery price: "), CultureInfo.InvariantCulture, out var basePrice)
            || basePrice <= 0 || basePrice > 1_000_000_000m)
        {
            Console.WriteLine("Error: Base price must be a number greater than 0 (and at most 1000000000).");
            return;
        }

        if (!int.TryParse(Ask("Number of items: "), out var items) || items < 1)
        {
            Console.WriteLine("Error: Number of items must be a whole number, 1 or more.");
            return;
        }

        if (!bool.TryParse(Ask("Express delivery (true/false): "), out var express))
        {
            Console.WriteLine("Error: Express delivery must be true or false.");
            return;
        }

        if (!Enum.TryParse<DeliveryType>(Ask("Delivery type (Pickup, Courier, DoorToDoor): "), true, out var type)
            || !Enum.IsDefined(type))
        {
            Console.WriteLine("Error: Delivery type must be Pickup, Courier or DoorToDoor.");
            return;
        }

        if (!Enum.TryParse<DeliveryZone>(Ask("Delivery zone (City, OutsideCity, Remote): "), true, out var zone)
            || !Enum.IsDefined(zone))
        {
            Console.WriteLine("Error: Delivery zone must be City, OutsideCity or Remote.");
            return;
        }

        var finalPrice = CalculatePrice(basePrice, items, type, zone, express);
        var text = finalPrice.ToString("F2", CultureInfo.InvariantCulture);
        Console.WriteLine($"Final delivery price: {text}");
    }

    static string? Ask(string question)
    {
        Console.Write(question);
        return Console.ReadLine();
    }

    static decimal ApplyRule(decimal price, Func<decimal, decimal> rule) => rule(price);

    static decimal ItemsFactor(int items) => items >= 8 ? 1.20m : items >= 4 ? 1.10m : 1.00m;

    static decimal TypeFactor(DeliveryType type) => type switch
    {
        DeliveryType.Pickup => 0.80m,
        DeliveryType.DoorToDoor => 1.15m,
        _ => 1.00m
    };

    static decimal ZoneFactor(DeliveryZone zone) => zone == DeliveryZone.OutsideCity ? 1.25m : 1.00m;

    static decimal CalculatePrice(decimal basePrice, int items, DeliveryType type, DeliveryZone zone, bool express)
    {
        Func<decimal, decimal> itemsRule = price => price * ItemsFactor(items);
        Func<decimal, decimal> typeRule = price => price * TypeFactor(type);
        Func<decimal, decimal> zoneRule = price => price * ZoneFactor(zone);
        Func<decimal, decimal> expressRule = price => express ? price * 1.30m : price;
        Func<decimal, decimal> roundRule = price => Math.Round(price, 2, MidpointRounding.AwayFromZero);

        var afterItems = ApplyRule(basePrice, itemsRule);
        var afterType = ApplyRule(afterItems, typeRule);
        var afterZone = ApplyRule(afterType, zoneRule);
        var afterExpress = ApplyRule(afterZone, expressRule);
        return ApplyRule(afterExpress, roundRule);
    }
}
