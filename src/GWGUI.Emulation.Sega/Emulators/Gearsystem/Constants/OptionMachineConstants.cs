namespace GWGUI.Emulation.Sega.Emulators.Gearsystem.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> MasterSystem { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MarkIII, ModelConstants.MasterSystem };
    internal static IReadOnlySet<string> GameGear { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.GameGear };
    internal static IReadOnlySet<string> LightgunSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MarkIII, ModelConstants.MasterSystem };
}
