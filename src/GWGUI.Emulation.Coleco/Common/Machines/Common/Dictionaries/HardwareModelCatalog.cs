namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => model switch
    {
        MachineModel.ColecoVision => new(HardwareConstants.Z80Resource, HardwareConstants.CpuFrequencyHz,
            HardwareConstants.ColecoVisionRamBytes, HardwareConstants.DisplayWidth,
            HardwareConstants.DisplayHeight, HardwareConstants.VideoRamBytes),
        MachineModel.Adam => new(HardwareConstants.Z80Resource, HardwareConstants.CpuFrequencyHz,
            HardwareConstants.AdamRamBytes, HardwareConstants.DisplayWidth,
            HardwareConstants.DisplayHeight, HardwareConstants.VideoRamBytes),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
