using System.Globalization;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;

internal static class NumericOptionValueFunctions
{
    internal static IReadOnlyList<CoreOptionValue> MemoryBudgetChoices(IReadOnlyList<int> values) =>
        values.Select(value =>
        {
            var native = value.ToString(CultureInfo.InvariantCulture);
            var label = value >= OptionNumericConstants.BinaryMemoryUnitScale
                ? ((decimal)value / OptionNumericConstants.BinaryMemoryUnitScale).ToString(CultureInfo.InvariantCulture) + OptionNumericConstants.GigabytesSuffix
                : native + OptionNumericConstants.MegabytesSuffix;
            return new CoreOptionValue(native, label);
        }).ToArray();

    internal static IReadOnlyList<CoreOptionValue> Choices(IReadOnlyList<int> values, string displaySuffix, IReadOnlyDictionary<string, string>? labelOverrides = null) =>
        values.Select(value =>
        {
            var native = value.ToString(CultureInfo.InvariantCulture);
            return new CoreOptionValue(native, labelOverrides?.GetValueOrDefault(native) ?? native + displaySuffix);
        }).ToArray();

    internal static IReadOnlyList<CoreOptionValue> Choices(IReadOnlyList<EmulationSettingsNumericRange> ranges, string format) =>
        ranges.SelectMany(range => Enumerable.Range(0, checked((int)Math.Round((range.Maximum - range.Minimum) / range.Step) + 1))
            .Select(index =>
            {
                var native = (range.Minimum + index * range.Step).ToString(format, CultureInfo.InvariantCulture) + range.UnitSuffix;
                return new CoreOptionValue(native, native);
            })).ToArray();
}
