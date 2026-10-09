using System.Globalization;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class NumericOptionValueFunctions
{
    internal static IReadOnlyList<CoreOptionValue> Choices(
        IReadOnlyList<EmulationSettingsNumericRange> ranges, string format) =>
        ranges.SelectMany(range => Enumerable.Range(0,
            checked((int)Math.Round((range.Maximum - range.Minimum) / range.Step) + 1))
            .Select(index =>
            {
                var native = (range.Minimum + index * range.Step)
                    .ToString(format, CultureInfo.InvariantCulture) + range.UnitSuffix;
                return new CoreOptionValue(native, native);
            })).ToArray();
}
