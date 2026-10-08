namespace GWGUI.Emulation.Sega.Emulators.Kronos.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> Saturn { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Saturn };
    internal static IReadOnlySet<string> StV { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.StV };
}
