using GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Constants;

namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Functions;

/// <summary>Résout la RAM physique du Model A indépendamment de son émulateur.</summary>
internal static class BbcMicroMemoryFunctions
{
    /// <summary>Conserve 16 Kio lorsque le réglage est absent ; seules les capacités matérielles 16/32 Kio sont acceptées.</summary>
    internal static int ModelABytes(MachineConfiguration configuration) => configuration.Options.GetValueOrDefault(SettingsConstants.MainMemory, BbcMicroModelAConstants.StandardMemoryKib) switch
    {
        BbcMicroModelAConstants.StandardMemoryKib => (int)BbcMicroModelConstants.SixteenKibibytes,
        BbcMicroModelAConstants.ExpandedMemoryKib => (int)BbcMicroModelConstants.ThirtyTwoKibibytes,
        _ => throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration))
    };
}
