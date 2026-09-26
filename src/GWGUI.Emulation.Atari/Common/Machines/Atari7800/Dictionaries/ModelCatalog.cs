namespace GWGUI.Emulation.Atari.Common.Machines.Atari7800.Dictionaries;

public static class Atari7800ModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } =
        HardwareModelFunctions.Values(
            HardwareModelFunctions.Create(MachineModel.Atari7800, Atari7800ModelConstants.ModelId,
                Atari7800ModelConstants.DisplayNameResource, Emulator.ProSystem,
                Atari7800ModelConstants.CpuFrequencyHz, Atari7800ModelConstants.MainMemoryBytes,
                HardwareModelFunctions.Values(HardwareCpu.Sally6502C),
                HardwareModelFunctions.Values(HardwareRegion.Ntsc, HardwareRegion.Pal),
                HardwareModelFunctions.Values(HardwareVideoCapability.Maria),
                HardwareModelFunctions.Values(HardwareAudioCapability.Tia,
                    HardwareAudioCapability.CartridgePokey),
                HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),
                HardwareModelFunctions.Values(
                    new HardwarePortDefinition(HardwarePortCapability.ProLineController,
                        Atari7800ModelConstants.TwoPorts),
                    new HardwarePortDefinition(HardwarePortCapability.LightGun,
                        Atari7800ModelConstants.TwoPorts)),
                HardwareModelFunctions.Values(FirmwareCategory.Atari7800Bios),
                HardwareModelFunctions.Values(MediaCategory.Cartridge)));
}
