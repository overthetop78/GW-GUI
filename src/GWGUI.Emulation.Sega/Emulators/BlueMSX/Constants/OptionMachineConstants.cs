namespace GWGUI.Emulation.Sega.Emulators.BlueMSX.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> CartridgeSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Sg1000, ModelConstants.Sc3000, ModelConstants.Sf7000 };
    internal static IReadOnlySet<string> Computers { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Sc3000, ModelConstants.Sf7000 };
}
