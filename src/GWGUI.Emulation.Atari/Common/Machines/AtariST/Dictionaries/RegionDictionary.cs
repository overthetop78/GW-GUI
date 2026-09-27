using System.Globalization;

namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Dictionaries;

internal static class RegionDictionary
{
    internal static readonly IReadOnlyDictionary<string, StRegion> ByLanguage =
        new Dictionary<string, StRegion>(StringComparer.OrdinalIgnoreCase)
        {
            [Language(HardwareSettingsFunctionsConstants.CsCZ)] = StRegion.CzechRepublic,
            [Language(HardwareSettingsFunctionsConstants.DeDE)] = StRegion.Germany,
            [Language(HardwareSettingsFunctionsConstants.ElGR)] = StRegion.Greece,
            [Language(HardwareSettingsFunctionsConstants.EsES)] = StRegion.Spain,
            [Language(HardwareSettingsFunctionsConstants.FiFI)] = StRegion.Finland,
            [Language(HardwareSettingsFunctionsConstants.FrFR)] = StRegion.France,
            [Language(HardwareSettingsFunctionsConstants.ItIT)] = StRegion.Italy,
            [Language(HardwareSettingsFunctionsConstants.NbNO)] = StRegion.Norway,
            [Language(HardwareSettingsFunctionsConstants.RuRU)] = StRegion.Russia,
            [Language(HardwareSettingsFunctionsConstants.SvSE)] = StRegion.Sweden
        };

    private static string Language(string cultureName) =>
        CultureInfo.GetCultureInfo(cultureName).TwoLetterISOLanguageName;
}
