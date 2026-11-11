using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length > 0 && args[0] == "share")
{
    try { ShareReport.Run(args); return 0; }
    catch (Exception e) when (e is FormatException or IOException or ArgumentException or OverflowException or UnauthorizedAccessException)
    { Console.Error.WriteLine(e.Message); return 1; }
}

if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- share study.tsv  |  original format: <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 1 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    var groups = lines.Select(s => Fields(s, 4)).GroupBy(f => (Area: f[0], Service: f[1]));
    foreach (var g in groups.OrderBy(g => g.Key.Area).ThenBy(g => g.Key.Service))
    {
        var demand = g.Sum(f => Whole(f[2], 0));
        var seats = g.Sum(f => Whole(f[3], 0));
        Console.WriteLine($"{g.Key.Area} / {g.Key.Service}: {Math.Min(demand, seats)} potential local coverage, {Math.Max(0, demand - seats)} unfilled, {Math.Max(0, seats - demand)} unused");
    }

    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}

static long Whole(string value, long minimum)
{
    var number = long.Parse(value, CultureInfo.InvariantCulture);
    Check(number >= minimum, $"Expected an integer of at least {minimum}.");
    return number;
}
