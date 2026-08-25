using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

internal static class AtariHardwareSettingsFunctions
{
    internal static EmulationSettingsChoice Invariant(string value, string displayValue) =>
        new(value, string.Empty, displayValue);

    internal static EmulationSettingsChoice CpuPrecision(AtariStCpuPrecision value) => value switch
    {
        AtariStCpuPrecision.Compatible =>
            new(value.ToString(), AtariHardwareSettingsConstants.CompatibleResource),
        AtariStCpuPrecision.CycleExact =>
            new(value.ToString(), AtariHardwareSettingsConstants.CycleExactResource),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    internal static EmulationSettingsChoice Fpu(AtariStFpu value) => value == AtariStFpu.None
        ? new(value.ToString(), AtariHardwareSettingsConstants.NoneResource)
        : Invariant(value.ToString(), value.ToString());

    internal static EmulationSettingsChoice StRegion(AtariStRegion value) => value == AtariStRegion.Multilingual
        ? new(value.ToString(), AtariHardwareSettingsConstants.MultilingualResource)
        : Invariant(value.ToString(), CultureInfo.GetCultureInfo(Culture(value)).DisplayName);

    internal static EmulationSettingsChoice ClassicRegion(AtariClassicRegion value) => value switch
    {
        AtariClassicRegion.RegionFree => new(value.ToString(), AtariHardwareSettingsConstants.RegionFreeResource),
        _ => Invariant(value.ToString(), value.ToString().ToUpperInvariant())
    };

    internal static EmulationSettingsChoice FrequencyMhz(int value) =>
        Invariant(value.ToString(CultureInfo.InvariantCulture),
            value.ToString(CultureInfo.CurrentCulture) + AtariHardwareSettingsConstants.FrequencyMhzSuffix);

    internal static EmulationSettingsChoice MemoryKib(int value) =>
        Bytes((long)value * AtariHardwareSettingsConstants.BytesPerKibibyte,
            value.ToString(CultureInfo.CurrentCulture) + AtariHardwareSettingsConstants.KibibyteSuffix);

    internal static EmulationSettingsChoice MemoryMib(int value) =>
        Bytes((long)value * AtariHardwareSettingsConstants.BytesPerMebibyte,
            value.ToString(CultureInfo.CurrentCulture) + AtariHardwareSettingsConstants.MebibyteSuffix);

    internal static EmulationSettingsChoice Bytes(long value, string? displayValue = null) =>
        new(value.ToString(CultureInfo.InvariantCulture), string.Empty,
            displayValue ?? FormatBytes(value), value);

    internal static EmulationSettingsChoice Expansion(AtariMemoryExpansionChoice value) =>
        new(value.Value,
            value.AdditionalBytes == 0 ? AtariHardwareSettingsConstants.NoneResource : string.Empty,
            value.AdditionalBytes == 0 ? null : FormatBytes(value.AdditionalBytes), value.AdditionalBytes);

    internal static string FormatBytes(long value)
    {
        if (value % AtariHardwareSettingsConstants.BytesPerMebibyte == 0)
            return value / AtariHardwareSettingsConstants.BytesPerMebibyte
                + AtariHardwareSettingsConstants.MebibyteSuffix;
        if (value % AtariHardwareSettingsConstants.BytesPerKibibyte == 0)
            return value / AtariHardwareSettingsConstants.BytesPerKibibyte
                + AtariHardwareSettingsConstants.KibibyteSuffix;
        return value + AtariHardwareSettingsConstants.ByteSuffix;
    }

    private static string Culture(AtariStRegion value) => value switch
    {
        AtariStRegion.UnitedStates => AtariHardwareSettingsFunctionsConstants.EnUS,
        AtariStRegion.Germany => AtariHardwareSettingsFunctionsConstants.DeDE,
        AtariStRegion.France => AtariHardwareSettingsFunctionsConstants.FrFR,
        AtariStRegion.UnitedKingdom => AtariHardwareSettingsFunctionsConstants.EnGB,
        AtariStRegion.Spain => AtariHardwareSettingsFunctionsConstants.EsES,
        AtariStRegion.Italy => AtariHardwareSettingsFunctionsConstants.ItIT,
        AtariStRegion.Sweden => AtariHardwareSettingsFunctionsConstants.SvSE,
        AtariStRegion.Switzerland => AtariHardwareSettingsFunctionsConstants.DeCH,
        AtariStRegion.Finland => AtariHardwareSettingsFunctionsConstants.FiFI,
        AtariStRegion.Norway => AtariHardwareSettingsFunctionsConstants.NbNO,
        AtariStRegion.CzechRepublic => AtariHardwareSettingsFunctionsConstants.CsCZ,
        AtariStRegion.Russia => AtariHardwareSettingsFunctionsConstants.RuRU,
        AtariStRegion.Greece => AtariHardwareSettingsFunctionsConstants.ElGR,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}
