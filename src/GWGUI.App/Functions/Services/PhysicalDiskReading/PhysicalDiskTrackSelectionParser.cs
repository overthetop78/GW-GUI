using GWGUI.App.Contracts.Services.PhysicalDiskReading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GWGUI.App.Functions.Services.PhysicalDiskReading;

public static partial class PhysicalDiskTrackSelectionParser
{
    public static IReadOnlyList<PhysicalDiskTrackAddress> Parse(string specification)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(specification);
        int[]? cylinders = null;
        int[]? heads = null;
        foreach (var element in specification.Split(':'))
        {
            var match = Assignment().Match(element);
            if (!match.Success) throw new ArgumentException("The internal reader supports only cylinder and head ranges.", nameof(specification));
            var values = Expand(match.Groups[2].Value, match.Groups[1].Value == "h" ? 1 : 83);
            if (match.Groups[1].Value == "c" && cylinders is null) cylinders = values;
            else if (match.Groups[1].Value == "h" && heads is null) heads = values;
            else throw new ArgumentException("The internal track selection contains a duplicate or unsupported element.", nameof(specification));
        }
        if (cylinders is null || heads is null) throw new ArgumentException("The internal track selection requires cylinders and heads.", nameof(specification));
        return cylinders.SelectMany(cylinder => heads.Select(head => new PhysicalDiskTrackAddress(cylinder, head))).ToArray();
    }

    private static int[] Expand(string value, int maximum)
    {
        var match = Range().Match(value);
        if (!match.Success) throw new ArgumentException("The internal track range is invalid.", nameof(value));
        var start = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var end = match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : start;
        var step = match.Groups[3].Success ? int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture) : 1;
        if (start > end || end > maximum) throw new ArgumentOutOfRangeException(nameof(value));
        return Enumerable.Range(0, (end - start) / step + 1).Select(index => start + index * step).ToArray();
    }

    [GeneratedRegex("^(c|h)=(.+)$", RegexOptions.CultureInvariant)]
    private static partial Regex Assignment();

    [GeneratedRegex("^(\\d+)(?:-(\\d+)(?:/(\\d+))?)?$", RegexOptions.CultureInvariant)]
    private static partial Regex Range();
}
