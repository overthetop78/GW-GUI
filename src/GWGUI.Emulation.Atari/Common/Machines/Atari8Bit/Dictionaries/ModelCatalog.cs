namespace GWGUI.Emulation.Atari.Common.Machines.Atari8Bit.Dictionaries;

public static class Atari8BitModelCatalog
{
    private static readonly IReadOnlyList<HardwareCpu> EightBitCpu =
        HardwareModelFunctions.Values(HardwareCpu.Mos6502C);
    private static readonly IReadOnlyList<HardwareCpu> OriginalEightBitCpu =
        HardwareModelFunctions.Values(HardwareCpu.Mos6502B);
    private static readonly IReadOnlyList<HardwareRegion> TelevisionRegions =
        HardwareModelFunctions.Values(HardwareRegion.Ntsc, HardwareRegion.Pal);
    private static readonly IReadOnlyList<HardwareVideoCapability> Video =
        HardwareModelFunctions.Values(HardwareVideoCapability.Antic, HardwareVideoCapability.Ctia,
            HardwareVideoCapability.Gtia);
    private static readonly IReadOnlyList<HardwareAudioCapability> Audio =
        HardwareModelFunctions.Values(HardwareAudioCapability.Pokey);
    private static readonly IReadOnlyList<HardwareStorageCapability> Storage =
        HardwareModelFunctions.Values(HardwareStorageCapability.Floppy,
            HardwareStorageCapability.Cassette, HardwareStorageCapability.Cartridge);
    private static readonly IReadOnlyList<HardwarePortDefinition> FourPortComputerPorts =
        HardwareModelFunctions.Values(
            new HardwarePortDefinition(HardwarePortCapability.Keyboard, Atari8BitModelConstants.OnePort),
            new HardwarePortDefinition(HardwarePortCapability.Joystick, Atari8BitModelConstants.FourPorts),
            new HardwarePortDefinition(HardwarePortCapability.Paddle, Atari8BitModelConstants.FourPorts));
    private static readonly IReadOnlyList<HardwarePortDefinition> TwoPortComputerPorts =
        HardwareModelFunctions.Values(
            new HardwarePortDefinition(HardwarePortCapability.Keyboard, Atari8BitModelConstants.OnePort),
            new HardwarePortDefinition(HardwarePortCapability.Joystick, Atari8BitModelConstants.TwoPorts),
            new HardwarePortDefinition(HardwarePortCapability.Paddle, Atari8BitModelConstants.TwoPorts));
    private static readonly IReadOnlyList<FirmwareCategory> Atari400And800Firmware =
        HardwareModelFunctions.Values(FirmwareCategory.AtariOsA, FirmwareCategory.AtariOsB,
            FirmwareCategory.AtariBasic);
    private static readonly IReadOnlyList<FirmwareCategory> Atari400Firmware =
        HardwareModelFunctions.Values(FirmwareCategory.AtariSystemOs);
    private static readonly IReadOnlyList<FirmwareCategory> XlXeFirmware =
        HardwareModelFunctions.Values(FirmwareCategory.AtariXlOs, FirmwareCategory.AtariBasic);
    private static readonly IReadOnlyList<MediaCategory> Media =
        HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette,
            MediaCategory.Cartridge);

    public static IReadOnlyList<HardwareModelDefinition> All { get; } =
        HardwareModelFunctions.Values(
            Create(MachineModel.Atari400, Atari8BitModelConstants.Atari400And800ModelId,
                Atari8BitModelConstants.Atari400DisplayNameResource, Atari8BitModelConstants.FortyEightKibibytes,
                OriginalEightBitCpu, FourPortComputerPorts, Atari400Firmware),
            Create(MachineModel.Atari800, Atari8BitModelConstants.Atari400And800ModelId,
                Atari8BitModelConstants.Atari800DisplayNameResource, Atari8BitModelConstants.FortyEightKibibytes,
                OriginalEightBitCpu, FourPortComputerPorts, Atari400And800Firmware),
            Create(MachineModel.Atari800Xl, Atari8BitModelConstants.Atari800XlModelId,
                Atari8BitModelConstants.Atari800XlDisplayNameResource, Atari8BitModelConstants.SixtyFourKibibytes,
                EightBitCpu, TwoPortComputerPorts, XlXeFirmware),
            Create(MachineModel.Atari130Xe, Atari8BitModelConstants.Atari130XeModelId,
                Atari8BitModelConstants.Atari130XeDisplayNameResource,
                Atari8BitModelConstants.OneHundredTwentyEightKibibytes, EightBitCpu,
                TwoPortComputerPorts, XlXeFirmware),
            Create(MachineModel.XlXe, Atari8BitModelConstants.XlXeModelId,
                Atari8BitModelConstants.XlXeDisplayNameResource,
                Atari8BitModelConstants.ThreeHundredTwentyKibibytes, EightBitCpu,
                TwoPortComputerPorts, XlXeFirmware),
            Create(MachineModel.Xegs, Atari8BitModelConstants.XegsModelId,
                Atari8BitModelConstants.XegsDisplayNameResource, Atari8BitModelConstants.SixtyFourKibibytes,
                EightBitCpu, TwoPortComputerPorts,
                HardwareModelFunctions.Values(FirmwareCategory.AtariXlOs, FirmwareCategory.AtariBasic,
                    FirmwareCategory.AtariXegsBios)));

    private static HardwareModelDefinition Create(MachineModel model, string modelId, string resourceKey,
        long memoryBytes, IReadOnlyList<HardwareCpu> cpus, IReadOnlyList<HardwarePortDefinition> ports,
        IReadOnlyList<FirmwareCategory> firmware) =>
        HardwareModelFunctions.Create(model, modelId, resourceKey, Emulator.Atari800,
            Atari8BitModelConstants.CpuFrequencyHz, memoryBytes, cpus, TelevisionRegions,
            Video, Audio, Storage, ports, firmware, Media);
}
