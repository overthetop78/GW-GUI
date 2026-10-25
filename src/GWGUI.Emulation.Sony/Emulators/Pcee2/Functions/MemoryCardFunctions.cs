using System.Globalization;
using Options = GWGUI.Emulation.Sony.Emulators.Pcee2.Constants.OptionConstants;
using CommonCards = GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions.MemoryCardFunctions;

namespace GWGUI.Emulation.Sony.Emulators.Pcee2.Functions;

internal static class MemoryCardFunctions
{
    internal static IReadOnlyList<int> ActiveSlots(MachineConfiguration configuration)
    {
        var mode = configuration.Options?.GetValueOrDefault(Options.MultitapKey);
        return [0, 1,
            .. (mode is Options.MultitapPort1 or Options.MultitapBoth ? new[] { 2, 3, 4 } : Array.Empty<int>()),
            .. (mode is Options.MultitapPort2 or Options.MultitapBoth ? new[] { 5, 6, 7 } : Array.Empty<int>())];
    }

    internal static string NativeFileName(int slot) => slot < 2
        ? "Mcd" + (slot + 1).ToString("000", CultureInfo.InvariantCulture) + ".ps2"
        : "Mcd-Multitap" + (slot < 5 ? "1" : "2") + "-Slot"
            + (slot < 5 ? slot : slot - 3).ToString("00", CultureInfo.InvariantCulture) + ".ps2";

    internal static string DisplayLabel(int slot) => "MC " + (slot < 2
        ? (slot + 1).ToString(CultureInfo.InvariantCulture)
        : (slot < 5 ? "1" : "2") + (char)('B' + (slot < 5 ? slot - 2 : slot - 5)));

    internal static IReadOnlyList<string> BuildSettings(MachineConfiguration configuration, IEnumerable<string> existing)
    {
        var lines = existing.ToList();
        var active = ActiveSlots(configuration);
        for (var slot = 2; slot < 8; slot++)
        {
            var prefix = "Multitap" + (slot < 5 ? "1" : "2") + "_Slot"
                + (slot < 5 ? slot : slot - 3).ToString(CultureInfo.InvariantCulture);
            IniSettingsFunctions.Set(lines, "MemoryCards", prefix + "_Enable", active.Contains(slot) && !string.IsNullOrWhiteSpace(CommonCards.SelectedPath(configuration, slot)) ? "true" : "false");
            IniSettingsFunctions.Set(lines, "MemoryCards", prefix + "_Filename", NativeFileName(slot));
        }
        return lines;

    }
}
