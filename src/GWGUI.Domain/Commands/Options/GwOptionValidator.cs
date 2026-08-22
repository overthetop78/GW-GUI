using System.Globalization;
using System.Text.RegularExpressions;

namespace GWGUI.Domain.Commands.Options;

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
            if (option.Argument is "--tracks" or "--out-tracks") ValidateTrackSpec(option.Value);
            if (option.Argument == "--pll") ValidatePllSpec(option.Value);
            if (option.Argument == "--precomp") ValidatePrecompSpec(option.Value);
            if (option.Argument is "--fake-index" or "--adjust-speed") ValidateSpeed(option.Value);
        }
    }

    public static void ValidateTrackSpec(string value)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal); var hasCylinders = false; var hasHeads = false;
        foreach (var element in Elements(value))
        {
            if (element == "hswap") { Unique(keys, element); continue; }
            var (key, argument) = Assignment(element); Unique(keys, key);
            if (key == "c") { ValidateCylinderSet(argument); hasCylinders = true; }
            else if (key == "h") { ValidateHeadSet(argument); hasHeads = true; }
            else if (key == "step") { if (!Regex.IsMatch(argument, @"^(?:[1-9]\d*|1/[1-9]\d*)$")) throw new ArgumentException("Invalid TSPEC step."); }
            else if (Regex.IsMatch(key, @"^h[01]\.off$")) { if (!Regex.IsMatch(argument, @"^[+-]\d+$")) throw new ArgumentException("Invalid TSPEC head offset."); }
            else throw new ArgumentException($"Unknown TSPEC element '{key}'.");
        }
        if (!hasCylinders || !hasHeads) throw new ArgumentException("TSPEC must specify cylinders and heads.");
    }

    public static void ValidatePllSpec(string value)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var element in Elements(value))
        {
            var (key, argument) = Assignment(element); Unique(keys, key);
            if (key is "period" or "phase") { if (!int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)) throw new ArgumentException($"Invalid PLL {key} percentage."); }
            else if (key == "lowpass") { if (!double.TryParse(argument, NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold) || threshold <= 0) throw new ArgumentException("Invalid PLL lowpass threshold."); }
            else throw new ArgumentException($"Unknown PLL element '{key}'.");
        }
    }

    public static void ValidatePrecompSpec(string value)
    {
        var keys = new HashSet<string>(StringComparer.Ordinal); var thresholds = 0;
        foreach (var element in Elements(value))
        {
            var (key, argument) = Assignment(element); Unique(keys, key);
            if (key == "type") { if (argument is not ("mfm" or "fm" or "gcr")) throw new ArgumentException("Precomp type must be mfm, fm or gcr."); }
            else { if (!int.TryParse(key, NumberStyles.None, CultureInfo.InvariantCulture, out _) || !int.TryParse(argument, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)) throw new ArgumentException("Invalid precompensation threshold."); thresholds++; }
        }
        if (thresholds == 0) throw new ArgumentException("Precompensation requires at least one cylinder threshold.");
    }

    public static void ValidateSpeed(string value)
    {
        var match = Regex.Match(value, @"^(\d+(?:\.\d+)?|\.\d+)(rpm|ms|us|ns|scp)?$");
        if (!match.Success || !double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || number <= 0) throw new ArgumentException("Invalid speed value.");
    }

    private static void ValidateCylinderSet(string value) { foreach (var item in value.Split(',')) if (!ValidRange(item, false)) throw new ArgumentException("Invalid TSPEC cylinder set."); }
    private static void ValidateHeadSet(string value) { foreach (var item in value.Split(',')) if (!ValidRange(item, true)) throw new ArgumentException("Invalid TSPEC head set."); }
    private static bool ValidRange(string value, bool head)
    {
        var match = Regex.Match(value, head ? @"^([01])(?:-([01]))?$" : @"^(\d+)(?:-(\d+)(?:/([1-9]\d*))?)?$"); if (!match.Success) return false;
        var start = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture); var end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : start; return start <= end;
    }
    private static string[] Elements(string value) { var elements = value.Split(':', StringSplitOptions.RemoveEmptyEntries); if (elements.Length == 0 || elements.Length != value.Count(character => character == ':') + 1) throw new ArgumentException("Specification contains an empty element."); return elements; }
    private static (string Key, string Value) Assignment(string element) { var separator = element.IndexOf('='); if (separator <= 0 || separator == element.Length - 1 || element.IndexOf('=', separator + 1) >= 0) throw new ArgumentException("Invalid specification assignment."); return (element[..separator], element[(separator + 1)..]); }
    private static void Unique(HashSet<string> keys, string key) { if (!keys.Add(key)) throw new ArgumentException($"Duplicate specification element '{key}'."); }

    private static void Exclusive(IReadOnlySet<string> names, string first, string second)
    {
        if (names.Contains(first) && names.Contains(second)) throw new ArgumentException($"{first} and {second} are mutually exclusive.");
    }
}
