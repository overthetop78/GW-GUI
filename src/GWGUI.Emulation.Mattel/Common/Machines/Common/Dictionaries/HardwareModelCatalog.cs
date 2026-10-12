namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => model == MachineModel.Intellivision
        ? new(HardwareConstants.CpuResource,HardwareConstants.CpuFrequencyHz,HardwareConstants.MainMemoryBytes, HardwareConstants.OutputWidth,HardwareConstants.OutputHeight,HardwareConstants.DisplayMemoryBytes)
        : throw new ArgumentOutOfRangeException(nameof(model));
}
