using System.IO;
using GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;

namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Functions;

internal static class RuntimeFunctions
{
    internal static bool RequiresMediaRestart(EmulationMediaSlot slot, string? path) => slot == EmulationMediaSlot.HardDisk0 || path is not null && string.Equals(Path.GetExtension(path), SessionMediaConstants.PlaylistExtension, StringComparison.OrdinalIgnoreCase);
    internal static bool RequiresOptionRestart(string key) => key is OptionConstants.Machine or OptionConstants.Slot3 or OptionConstants.Slot4 or OptionConstants.Slot5 or OptionConstants.Slot7 or OptionConstants.VideoRefreshRate or OptionConstants.VideoStyle;
}
