namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Dictionaries;

internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => model switch
    {
        MachineModel.CassetteVision or MachineModel.CassetteVisionJr =>
            new(HardwareConstants.PdCpuResource, null, null, HardwareConstants.PdDisplayWidth, HardwareConstants.PdDisplayHeight),
        MachineModel.SuperCassetteVision or MachineModel.SuperCassetteVisionLady =>
            new(HardwareConstants.ScvCpuResource, HardwareConstants.ScvCpuFrequencyHz, HardwareConstants.ScvInternalMemoryBytes,
                HardwareConstants.ScvDisplayWidth, HardwareConstants.ScvDisplayHeight, HardwareConstants.ScvVideoMemoryBytes),
        MachineModel.SuperCassetteVisionYeno =>
            new(HardwareConstants.ScvCpuResource, null, HardwareConstants.ScvInternalMemoryBytes,
                HardwareConstants.YenoDisplayWidth, HardwareConstants.YenoDisplayHeight, HardwareConstants.ScvVideoMemoryBytes),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}
