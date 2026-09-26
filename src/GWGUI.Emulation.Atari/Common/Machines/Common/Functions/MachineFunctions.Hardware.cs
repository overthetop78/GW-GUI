using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class HardwareSettingsFunctions
{
    internal static EmulationSettingsChoice Invariant(string value, string displayValue) =>
        new(value, string.Empty, displayValue);

    internal static EmulationSettingsChoice CpuPrecision(StCpuPrecision value) => value switch
    {
        StCpuPrecision.Compatible =>
            new(value.ToString(), HardwareSettingsConstants.CompatibleResource),
        StCpuPrecision.CycleExact =>
            new(value.ToString(), HardwareSettingsConstants.CycleExactResource),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static EmulationSettingsChoice Fpu(StFpu value) => value == StFpu.None
        ? new(value.ToString(), HardwareSettingsConstants.NoneResource)
        : Invariant(value.ToString(), value.ToString());

    internal static EmulationSettingsChoice StRegionChoice(StRegion value) => value == StRegion.Multilingual
        ? new(value.ToString(), HardwareSettingsConstants.MultilingualResource)
        : Invariant(value.ToString(), CultureInfo.GetCultureInfo(Culture(value)).DisplayName);

    internal static EmulationSettingsChoice HardwareRegionChoice(HardwareRegion value) => value switch
    {
        HardwareRegion.RegionFree => new(value.ToString(), HardwareSettingsConstants.RegionFreeResource),
        _ => Invariant(value.ToString(), value.ToString().ToUpperInvariant())
    };

    internal static EmulationSettingsChoice FrequencyMhz(int value) =>
        Invariant(value.ToString(CultureInfo.InvariantCulture),
            value.ToString(CultureInfo.CurrentCulture) + HardwareSettingsConstants.FrequencyMhzSuffix);

    internal static EmulationSettingsChoice MemoryKib(int value) =>
        Bytes((long)value * HardwareSettingsConstants.BytesPerKibibyte,
            value.ToString(CultureInfo.CurrentCulture) + HardwareSettingsConstants.KibibyteSuffix);

    internal static EmulationSettingsChoice MemoryMib(int value) =>
        Bytes((long)value * HardwareSettingsConstants.BytesPerMebibyte,
            value.ToString(CultureInfo.CurrentCulture) + HardwareSettingsConstants.MebibyteSuffix);

    internal static EmulationSettingsChoice Bytes(long value, string? displayValue = null) =>
        new(value.ToString(CultureInfo.InvariantCulture), string.Empty,
            displayValue ?? FormatBytes(value), value);

    internal static EmulationSettingsChoice Expansion(MemoryExpansionChoice value) =>
        new(value.Value,
            value.AdditionalBytes == 0 ? HardwareSettingsConstants.NoneResource : string.Empty,
            value.AdditionalBytes == 0 ? null : FormatBytes(value.AdditionalBytes), value.AdditionalBytes);

    internal static string FormatBytes(long value)
    {
        if (value % HardwareSettingsConstants.BytesPerMebibyte == 0)
            return value / HardwareSettingsConstants.BytesPerMebibyte
                + HardwareSettingsConstants.MebibyteSuffix;
        if (value % HardwareSettingsConstants.BytesPerKibibyte == 0)
            return value / HardwareSettingsConstants.BytesPerKibibyte
                + HardwareSettingsConstants.KibibyteSuffix;
        return value + HardwareSettingsConstants.ByteSuffix;
    }

    private static string Culture(StRegion value) => value switch
    {
        StRegion.UnitedStates => HardwareSettingsFunctionsConstants.EnUS,
        StRegion.Germany => HardwareSettingsFunctionsConstants.DeDE,
        StRegion.France => HardwareSettingsFunctionsConstants.FrFR,
        StRegion.UnitedKingdom => HardwareSettingsFunctionsConstants.EnGB,
        StRegion.Spain => HardwareSettingsFunctionsConstants.EsES,
        StRegion.Italy => HardwareSettingsFunctionsConstants.ItIT,
        StRegion.Sweden => HardwareSettingsFunctionsConstants.SvSE,
        StRegion.Switzerland => HardwareSettingsFunctionsConstants.DeCH,
        StRegion.Finland => HardwareSettingsFunctionsConstants.FiFI,
        StRegion.Norway => HardwareSettingsFunctionsConstants.NbNO,
        StRegion.CzechRepublic => HardwareSettingsFunctionsConstants.CsCZ,
        StRegion.Russia => HardwareSettingsFunctionsConstants.RuRU,
        StRegion.Greece => HardwareSettingsFunctionsConstants.ElGR,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
