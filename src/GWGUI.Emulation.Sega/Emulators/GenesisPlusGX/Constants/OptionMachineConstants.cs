namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> MasterSystem { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MarkIII, ModelConstants.MasterSystem };
    internal static IReadOnlySet<string> GameGear { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.GameGear };
    internal static IReadOnlySet<string> MegaDrive { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MegaDrive };
    internal static IReadOnlySet<string> MegaCd { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MegaCd };
    internal static IReadOnlySet<string> MegaDriveAndCd { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MegaDrive, ModelConstants.MegaCd };
    internal static IReadOnlySet<string> PsgSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.Sg1000, ModelConstants.Sc3000, ModelConstants.MarkIII, ModelConstants.MasterSystem, ModelConstants.GameGear, ModelConstants.MegaDrive, ModelConstants.MegaCd };
    internal static IReadOnlySet<string> FmSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MarkIII, ModelConstants.MasterSystem, ModelConstants.MegaDrive, ModelConstants.MegaCd };
    internal static IReadOnlySet<string> LightgunSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MarkIII, ModelConstants.MasterSystem, ModelConstants.MegaDrive, ModelConstants.MegaCd };
    internal static IReadOnlySet<string> BootRomSystems { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MarkIII, ModelConstants.MasterSystem, ModelConstants.MegaDrive, ModelConstants.MegaCd };
}
