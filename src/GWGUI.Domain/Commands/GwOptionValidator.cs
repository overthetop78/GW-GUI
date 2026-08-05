using GWGUI.Domain.Read;

namespace GWGUI.Domain.Commands;

public static class GwOptionValidator
{
    private static readonly HashSet<string> Flags = new(StringComparer.Ordinal)
    {
        "--raw", "--hard-sectors", "--reverse", "--gen-tg43", "--no-clobber",
        "--pre-erase", "--erase-empty", "--no-verify"
    };

    private static readonly HashSet<string> NonNegativeIntegers = new(StringComparer.Ordinal)
    {
        "--retries", "--seek-retries"
    };

    private static readonly HashSet<string> PositiveIntegers = new(StringComparer.Ordinal)
    {
        "--revs"
    };

    public static void Validate(IReadOnlyList<EnabledOption> options)
    {
        var names = options.Select(option => option.Argument).ToHashSet(StringComparer.Ordinal);
        Exclusive(names, "--fake-index", "--hard-sectors");
        Exclusive(names, "--densel", "--gen-tg43");

        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option.Argument) || !option.Argument.StartsWith("--", StringComparison.Ordinal))
                throw new ArgumentException("Optional arguments must start with '--'.");
            if (Flags.Contains(option.Argument))
            {
                if (!string.IsNullOrWhiteSpace(option.Value)) throw new ArgumentException($"{option.Argument} does not accept a value.");
                continue;
            }
            if (string.IsNullOrWhiteSpace(option.Value)) throw new ArgumentException($"{option.Argument} requires a value.");
            if (NonNegativeIntegers.Contains(option.Argument) && (!int.TryParse(option.Value, out var nonNegative) || nonNegative < 0))
                throw new ArgumentException($"{option.Argument} requires a non-negative integer.");
            if (PositiveIntegers.Contains(option.Argument) && (!int.TryParse(option.Value, out var positive) || positive < 1))
                throw new ArgumentException($"{option.Argument} requires a positive integer.");
            if (option.Argument == "--densel" && option.Value is not ("H" or "L"))
                throw new ArgumentException("--densel accepts H or L.");
        }
    }

    private static void Exclusive(IReadOnlySet<string> names, string first, string second)
    {
        if (names.Contains(first) && names.Contains(second)) throw new ArgumentException($"{first} and {second} are mutually exclusive.");
    }
}
