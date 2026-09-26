namespace GWGUI.Emulation.Atari.Common.Machines.AtariLynx.Dictionaries;

public static class AtariLynxModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } =
        HardwareModelFunctions.Values(
            HardwareModelFunctions.Create(MachineModel.Lynx, AtariLynxModelConstants.ModelId,
                AtariLynxModelConstants.DisplayNameResource, Emulator.BeetleLynx,
                AtariLynxModelConstants.CpuFrequencyHz, AtariLynxModelConstants.MainMemoryBytes,
                HardwareModelFunctions.Values(HardwareCpu.Wdc65Sc02),
                HardwareModelFunctions.Values(HardwareRegion.RegionFree),
                HardwareModelFunctions.Values(HardwareVideoCapability.Suzy, HardwareVideoCapability.Mikey),
                HardwareModelFunctions.Values(HardwareAudioCapability.Mikey),
                HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),
                HardwareModelFunctions.Values(
                    new HardwarePortDefinition(HardwarePortCapability.EnhancedController,
                        AtariLynxModelConstants.OnePort)),
                HardwareModelFunctions.Values(FirmwareCategory.LynxBootRom),
                HardwareModelFunctions.Values(MediaCategory.Cartridge)));
}
