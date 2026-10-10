namespace GWGUI.Emulation.Philips.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => model switch
    {
        MachineModel.PhilipsCdi => new(HardwareConstants.CdiCpuResource, null, HardwareConstants.CdiRamBytes,
            null, null, HardwareConstants.CdiPlaneRamBytes),
        MachineModel.P2000T => new(HardwareConstants.P2000CpuResource, HardwareConstants.P2000CpuFrequencyHz,
            HardwareConstants.P2000RamBytes, HardwareConstants.P2000OutputWidth,
            HardwareConstants.P2000OutputHeight, HardwareConstants.P2000VideoMemoryBytes),
        MachineModel.Odyssey2 => new(HardwareConstants.VideopacCpuResource, HardwareConstants.OdysseyCpuFrequencyHz,
            HardwareConstants.VideopacRamBytes, HardwareConstants.VideopacOutputWidth, HardwareConstants.VideopacOutputHeight),
        MachineModel.VideopacG7000 => new(HardwareConstants.VideopacCpuResource, HardwareConstants.VideopacCpuFrequencyHz,
            HardwareConstants.VideopacRamBytes, HardwareConstants.VideopacOutputWidth, HardwareConstants.VideopacOutputHeight),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
