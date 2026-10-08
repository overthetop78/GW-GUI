namespace GWGUI.Emulation.Sega.Emulators.BlastEm.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> MegaDrive { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MegaDrive };
}
