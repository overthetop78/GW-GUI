using System.Globalization;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Philips.Common.Contracts;

namespace GWGUI.Emulation.Philips.Emulators.Common.Functions;

internal static class NumericOptionValueFunctions
{
    internal static IReadOnlyList<CoreOptionValue> IntegerChoices(IReadOnlyList<EmulationSettingsNumericRange> ranges, bool labelIncludesUnit) =>
        ranges.SelectMany(range => Enumerable.Range(0, checked((int)Math.Round((range.Maximum - range.Minimum) / range.Step) + 1))
            .Select(index =>
            {
                var value = (range.Minimum + index * range.Step).ToString("0", CultureInfo.InvariantCulture);
                var display = value + range.UnitSuffix;
                return new CoreOptionValue(value, labelIncludesUnit ? display : value, InvariantDisplayValue: display);
            })).ToArray();

    internal static string Resolution(int width, int height) => string.Create(CultureInfo.InvariantCulture, $"{width}x{height}");

    internal static IReadOnlyList<CoreOptionValue> Resolutions(IReadOnlyList<(int Width, int Height)> sizes) => sizes.Select(size =>
    {
        var value = Resolution(size.Width, size.Height);
        return new CoreOptionValue(value, value, InvariantDisplayValue: value);
    }).ToArray();
}
