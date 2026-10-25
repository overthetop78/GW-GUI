namespace GWGUI.Emulation.Sharp.Common.Machines.Common.Dictionaries;
internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => model switch
    {
        MachineModel.X68000 => new(HardwareConstants.X68000CpuResource, HardwareConstants.X68000FrequencyHz, HardwareConstants.X68000MemoryBytes, null, null, HardwareConstants.X68000DisplayMemoryBytes),
        MachineModel.X68000Xvi => new(HardwareConstants.XviCpuResource, HardwareConstants.XviFrequencyHz, HardwareConstants.XviMemoryBytes, null, null, HardwareConstants.X68000DisplayMemoryBytes),
        MachineModel.X68030 => new(HardwareConstants.X68030CpuResource, HardwareConstants.X68030FrequencyHz, HardwareConstants.X68030MemoryBytes, null, null, HardwareConstants.X68000DisplayMemoryBytes),
        MachineModel.X1 => new(HardwareConstants.X1CpuResource, HardwareConstants.X1FrequencyHz, HardwareConstants.X1MemoryBytes, null, null, HardwareConstants.X1BitmapMemoryBytes),
        MachineModel.X1Turbo or MachineModel.X1TurboZ => new(HardwareConstants.X1CpuResource, HardwareConstants.X1FrequencyHz, HardwareConstants.X1MemoryBytes, null, null, HardwareConstants.X1TurboBitmapMemoryBytes),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}