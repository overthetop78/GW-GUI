namespace GWGUI.Emulation.Sega.Emulators.Flycast.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> Dreamcast { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Dreamcast };
    internal static IReadOnlySet<string> Arcade { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Naomi, ModelConstants.Naomi2, ModelConstants.Atomiswave, ModelConstants.SystemSp };
    internal static IReadOnlySet<string> OpticalSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Dreamcast, ModelConstants.Naomi, ModelConstants.Naomi2 };
    internal static IReadOnlySet<string> NetworkSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Dreamcast, ModelConstants.Naomi, ModelConstants.Naomi2 };
}
