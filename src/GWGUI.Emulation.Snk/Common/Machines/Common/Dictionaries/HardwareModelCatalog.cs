namespace GWGUI.Emulation.Snk.Common.Machines.Common.Dictionaries;
internal static class HardwareModelCatalog
{
    internal static HardwareModelDefinition Get(MachineModel model) => model switch
    {
        MachineModel.NeoGeoAes or MachineModel.NeoGeoMvs => new(HardwareConstants.CartridgeCpuResource, HardwareConstants.CartridgeFrequencyHz, HardwareConstants.CartridgeMemoryBytes, HardwareConstants.CartridgeDisplayWidth, HardwareConstants.CartridgeDisplayHeight),
        MachineModel.NeoGeoCd or MachineModel.NeoGeoCdz => new(HardwareConstants.CartridgeCpuResource, HardwareConstants.CartridgeFrequencyHz, HardwareConstants.CdMemoryBytes, HardwareConstants.CartridgeDisplayWidth, HardwareConstants.CartridgeDisplayHeight),
        MachineModel.NeoGeoPocket or MachineModel.NeoGeoPocketColor => new(HardwareConstants.PocketCpuResource, HardwareConstants.PocketFrequencyHz, HardwareConstants.PocketMemoryBytes, HardwareConstants.PocketDisplayWidth, HardwareConstants.PocketDisplayHeight),
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };
}