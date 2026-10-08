namespace GWGUI.Emulation.Nintendo.Emulators.MesenS.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> GameBoy { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.GameBoy, ModelConstants.GameBoyColor };
    internal static IReadOnlySet<string> Snes { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.Snes };
}
