namespace GWGUI.Emulation.Atari.Common.Machines.Atari2600.Dictionaries;

public static class Atari2600ModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } =
        HardwareModelFunctions.Values(
            HardwareModelFunctions.Create(MachineModel.Atari2600, Atari2600ModelConstants.ModelId,
                Atari2600ModelConstants.DisplayNameResource, Emulator.Stella,
                Atari2600ModelConstants.CpuFrequencyHz, Atari2600ModelConstants.MainMemoryBytes,
                HardwareModelFunctions.Values(HardwareCpu.Mos6507),
                HardwareModelFunctions.Values(HardwareRegion.Ntsc, HardwareRegion.Pal),
                HardwareModelFunctions.Values(HardwareVideoCapability.Tia),
                HardwareModelFunctions.Values(HardwareAudioCapability.Tia),
                HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),
                HardwareModelFunctions.Values(
                    new HardwarePortDefinition(HardwarePortCapability.Joystick, Atari2600ModelConstants.TwoPorts),
                    new HardwarePortDefinition(HardwarePortCapability.Paddle, Atari2600ModelConstants.TwoPorts),
                    new HardwarePortDefinition(HardwarePortCapability.DrivingController,
                        Atari2600ModelConstants.TwoPorts)),
                HardwareModelFunctions.Values<FirmwareCategory>(),
                HardwareModelFunctions.Values(MediaCategory.Cartridge)));
}
