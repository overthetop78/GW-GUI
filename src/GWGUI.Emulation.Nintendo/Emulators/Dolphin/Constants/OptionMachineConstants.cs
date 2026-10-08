namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> GameCube { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.GameCube };
    internal static IReadOnlySet<string> Wii { get; } = new HashSet<string>(StringComparer.Ordinal)
        { ModelConstants.Wii };
}
