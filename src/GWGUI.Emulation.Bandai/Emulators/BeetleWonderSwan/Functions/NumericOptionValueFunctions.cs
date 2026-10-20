using System.Globalization;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Bandai.Common.Machines.Common.Constants;
using GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;

namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Functions;

internal static class NumericOptionValueFunctions
{
    internal static IReadOnlyList<CoreOptionValue> IntegerRange(EmulationSettingsNumericRange range, bool invariantDisplay) =>
        Enumerable.Range(0, checked((int)((range.Maximum - range.Minimum) / range.Step) + 1)).Select(index =>
        {
            var value = (range.Minimum + index * range.Step).ToString("0", CultureInfo.InvariantCulture);
            return new CoreOptionValue(value, value, InvariantDisplayValue: invariantDisplay ? value + range.UnitSuffix : null);
        }).ToArray();

    internal static IReadOnlyList<CoreOptionValue> AudioSampleRates() => OptionConstants.AudioSampleRatesHz.Select(rate =>
    {
        var value = rate.ToString(CultureInfo.InvariantCulture);
        var display = (rate / SettingsConstants.HertzPerKilohertz).ToString("0.###", CultureInfo.InvariantCulture) + " " + SettingsConstants.KilohertzUnit;
        return new CoreOptionValue(value, value, InvariantDisplayValue: display);
    }).ToArray();
}
