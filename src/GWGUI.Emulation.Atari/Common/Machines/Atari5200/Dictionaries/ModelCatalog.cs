namespace GWGUI.Emulation.Atari.Common.Machines.Atari5200.Dictionaries;

public static class Atari5200ModelCatalog
{
    public static IReadOnlyList<HardwareModelDefinition> All { get; } =
        HardwareModelFunctions.Values(
            HardwareModelFunctions.Create(MachineModel.Atari5200, Atari5200ModelConstants.ModelId,
                Atari5200ModelConstants.DisplayNameResource, Emulator.Atari800,
                Atari5200ModelConstants.CpuFrequencyHz, Atari5200ModelConstants.MainMemoryBytes,
                HardwareModelFunctions.Values(HardwareCpu.Mos6502C),
                HardwareModelFunctions.Values(HardwareRegion.Ntsc, HardwareRegion.Pal),
                HardwareModelFunctions.Values(HardwareVideoCapability.Antic, HardwareVideoCapability.Ctia,
                    HardwareVideoCapability.Gtia),
                HardwareModelFunctions.Values(HardwareAudioCapability.Pokey),
                HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),
                HardwareModelFunctions.Values(
                    new HardwarePortDefinition(HardwarePortCapability.AnalogJoystick,
                        Atari5200ModelConstants.FourPorts),
                    new HardwarePortDefinition(HardwarePortCapability.NumericKeypad,
                        Atari5200ModelConstants.FourPorts)),
                HardwareModelFunctions.Values(FirmwareCategory.Atari5200Bios),
                HardwareModelFunctions.Values(MediaCategory.Cartridge)));
}
