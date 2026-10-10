namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => Enum.IsDefined(model)
        ? new(HardwareConstants.CpuResource, null, HardwareConstants.ScratchpadMemoryBytes,
            HardwareConstants.DisplayWidth, HardwareConstants.DisplayHeight, HardwareConstants.VideoMemoryBytes)
        : throw new ArgumentOutOfRangeException(nameof(model));
}
