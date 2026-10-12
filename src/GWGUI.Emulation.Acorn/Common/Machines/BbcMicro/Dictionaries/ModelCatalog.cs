namespace GWGUI.Emulation.Acorn.Common.Machines.BbcMicro.Dictionaries;

public static class BbcMicroModelCatalog
{
    private static readonly IReadOnlyList<HardwareRegion> Regions = HardwareModelFunctions.Values(HardwareRegion.Pal);
    private static readonly IReadOnlyList<HardwareVideoCapability> Video = HardwareModelFunctions.Values(HardwareVideoCapability.Mc6845, HardwareVideoCapability.VideoUla, HardwareVideoCapability.Saa5050);
    private static readonly IReadOnlyList<HardwareAudioCapability> Audio = HardwareModelFunctions.Values(HardwareAudioCapability.Sn76489);
    private static readonly IReadOnlyList<HardwarePortDefinition> KeyboardPorts = HardwareModelFunctions.Values(new HardwarePortDefinition(HardwarePortCapability.Keyboard, BbcMicroModelConstants.OnePort));
    private static readonly IReadOnlyList<HardwarePortDefinition> ComputerPorts =
        HardwareModelFunctions.Values(new HardwarePortDefinition(HardwarePortCapability.Keyboard, BbcMicroModelConstants.OnePort), new HardwarePortDefinition(HardwarePortCapability.AnalogJoystick, BbcMicroModelConstants.TwoPorts));

    public static IReadOnlyList<HardwareModelDefinition> All { get; } = HardwareModelFunctions.Values(
        Create(MachineModel.BbcMicroModelA, BbcMicroModelConstants.ModelAId,
            BbcMicroModelConstants.ModelADisplayNameResource, BbcMicroModelConstants.SixteenKibibytes,
            HardwareCpu.Mos6502A, KeyboardPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Cassette),
            HardwareModelFunctions.Values(FirmwareCategory.OperatingSystem, FirmwareCategory.Basic),
            HardwareModelFunctions.Values(MediaCategory.Cassette)),
        Create(MachineModel.BbcMicroModelB, BbcMicroModelConstants.ModelBId,
            BbcMicroModelConstants.ModelBDisplayNameResource, BbcMicroModelConstants.ThirtyTwoKibibytes,
            HardwareCpu.Mos6502A, ComputerPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette),
            HardwareModelFunctions.Values(FirmwareCategory.OperatingSystem, FirmwareCategory.Basic, FirmwareCategory.DiscFilingSystem),
            HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette)),
        Create(MachineModel.BbcMaster128, BbcMicroModelConstants.Master128Id,
            BbcMicroModelConstants.Master128DisplayNameResource, BbcMicroModelConstants.OneHundredTwentyEightKibibytes,
            HardwareCpu.Rockwell65C12, ComputerPorts,
            HardwareModelFunctions.Values(HardwareStorageCapability.Floppy, HardwareStorageCapability.Cassette, HardwareStorageCapability.Cartridge),
            HardwareModelFunctions.Values(FirmwareCategory.MasterMos, FirmwareCategory.Basic, FirmwareCategory.DiscFilingSystem),
            HardwareModelFunctions.Values(MediaCategory.Floppy, MediaCategory.Cassette, MediaCategory.Cartridge)));

    private static HardwareModelDefinition Create(MachineModel model, string modelId, string resourceKey,
        long memoryBytes, HardwareCpu cpu, IReadOnlyList<HardwarePortDefinition> ports,
        IReadOnlyList<HardwareStorageCapability> storage, IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<MediaCategory> media) =>
        HardwareModelFunctions.Create(model, modelId, resourceKey, Emulator.B2, BbcMicroModelConstants.CpuFrequencyHz, memoryBytes, HardwareModelFunctions.Values(cpu), Regions, Video, Audio, storage, ports, firmware, media);
}
