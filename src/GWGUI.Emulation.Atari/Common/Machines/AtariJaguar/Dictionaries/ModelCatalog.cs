namespace GWGUI.Emulation.Atari.Common.Machines.AtariJaguar.Dictionaries;

public static class AtariJaguarModelCatalog
{
    private static readonly IReadOnlyList<HardwareCpu> Cpus = HardwareModelFunctions.Values(
        HardwareCpu.Motorola68000, HardwareCpu.TomGraphicsProcessor,
        HardwareCpu.JerrySignalProcessor);
    private static readonly IReadOnlyList<HardwareRegion> Regions =
        HardwareModelFunctions.Values(HardwareRegion.Ntsc, HardwareRegion.Pal);
    private static readonly IReadOnlyList<HardwareVideoCapability> Video =
        HardwareModelFunctions.Values(HardwareVideoCapability.Tom);
    private static readonly IReadOnlyList<HardwareAudioCapability> Audio =
        HardwareModelFunctions.Values(HardwareAudioCapability.Jerry);
    private static readonly IReadOnlyList<HardwarePortDefinition> Ports =
        HardwareModelFunctions.Values(new HardwarePortDefinition(
            HardwarePortCapability.EnhancedController, AtariJaguarModelConstants.TwoPorts));

    public static IReadOnlyList<HardwareModelDefinition> All { get; } =
        HardwareModelFunctions.Values(
            Create(MachineModel.Jaguar, AtariJaguarModelConstants.JaguarModelId,
                AtariJaguarModelConstants.JaguarDisplayNameResource,
                HardwareModelFunctions.Values<FirmwareCategory>(),
                HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge),
                HardwareModelFunctions.Values(MediaCategory.Cartridge)),
            Create(MachineModel.JaguarCd, AtariJaguarModelConstants.JaguarCdModelId,
                AtariJaguarModelConstants.JaguarCdDisplayNameResource,
                HardwareModelFunctions.Values(FirmwareCategory.JaguarCdBios),
                HardwareModelFunctions.Values(HardwareStorageCapability.Cartridge,
                    HardwareStorageCapability.CompactDisc),
                HardwareModelFunctions.Values(MediaCategory.Cartridge, MediaCategory.CompactDisc)));

    private static HardwareModelDefinition Create(MachineModel model, string modelId,
        string resourceKey, IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<HardwareStorageCapability> storage, IReadOnlyList<MediaCategory> media) =>
        HardwareModelFunctions.Create(model, modelId, resourceKey, Emulator.VirtualJaguar,
            AtariJaguarModelConstants.CpuFrequencyHz, AtariJaguarModelConstants.MainMemoryBytes,
            Cpus, Regions, Video, Audio, storage, Ports, firmware, media);
}
