using System.Globalization;
using System.Text.RegularExpressions;

namespace GWGUI.App.Services.PhysicalDiskReading;

public static partial class PhysicalDiskIndexPeriodParser
{
    public static TimeSpan Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var match = Period().Match(value.Trim());
        if (!match.Success || !double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || number <= 0)
            throw new ArgumentException("The internal fake-index period is invalid.", nameof(value));
        return match.Groups[2].Value switch
        {
            "rpm" => TimeSpan.FromSeconds(60 / number),
            "ms" => TimeSpan.FromMilliseconds(number),
            "us" => TimeSpan.FromTicks(checked((long)Math.Round(number * TimeSpan.TicksPerMicrosecond))),
            "ns" => TimeSpan.FromTicks(checked((long)Math.Max(1, Math.Round(number / 100)))),
            _ => throw new ArgumentException("The internal fake-index unit is unsupported.", nameof(value))
        };
    }

    [GeneratedRegex("^(\\d+(?:\\.\\d+)?|\\.\\d+)(rpm|ms|us|ns)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex Period();
}
