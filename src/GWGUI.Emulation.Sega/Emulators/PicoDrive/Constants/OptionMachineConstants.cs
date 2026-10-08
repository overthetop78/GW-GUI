namespace GWGUI.Emulation.Sega.Emulators.PicoDrive.Constants;

internal static class OptionMachineConstants
{
    internal static IReadOnlySet<string> MasterSystem { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MasterSystem };
    internal static IReadOnlySet<string> GameGear { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.GameGear };
    internal static IReadOnlySet<string> MegaCd { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MegaCd };
    internal static IReadOnlySet<string> MegaDriveFamily { get; } = new HashSet<string>(StringComparer.Ordinal)
    { ModelConstants.MegaDrive, ModelConstants.MegaCd, ModelConstants.ThirtyTwoX };
}
