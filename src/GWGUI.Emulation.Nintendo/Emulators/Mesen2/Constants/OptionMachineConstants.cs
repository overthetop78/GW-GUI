namespace GWGUI.Emulation.Nintendo.Emulators.Mesen2.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> FamicomDisk { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.FamicomDisk };
    internal static IReadOnlySet<string> GameBoy { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.GameBoy, ModelConstants.GameBoyColor };
    internal static IReadOnlySet<string> Nes { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.Nes, ModelConstants.FamicomDisk };
    internal static IReadOnlySet<string> Snes { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.Snes };
}
