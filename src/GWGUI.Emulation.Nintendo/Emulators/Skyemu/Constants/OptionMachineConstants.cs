namespace GWGUI.Emulation.Nintendo.Emulators.Skyemu.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> GameBoy { get; } = new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoy, ModelConstants.GameBoyColor };
    internal static IReadOnlySet<string> GameBoyAdvance { get; } = new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance };
    internal static IReadOnlySet<string> NintendoDs { get; } = new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs };
}
