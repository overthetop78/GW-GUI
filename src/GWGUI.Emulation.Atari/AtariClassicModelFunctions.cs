using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari;

internal static class AtariClassicModelFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    internal static IReadOnlyDictionary<AtariMachineModel, AtariClassicModelDefinition> Index(
        IReadOnlyList<AtariClassicModelDefinition> definitions)
    {
        var result = definitions.ToDictionary(definition => definition.Model);
        if (result.Count != definitions.Count)
            throw new InvalidOperationException(AtariErrorMessages.DuplicateClassicModelDefinition);
        return result;
    }

    internal static AtariClassicModelDefinition Create(
        AtariMachineModel model,
        string stableModelId,
        string displayNameResourceKey,
        AtariCoreKind core,
        long cpuFrequencyHz,
        long mainMemoryBytes,
        IReadOnlyList<AtariClassicCpu> cpus,
        IReadOnlyList<AtariClassicRegion> regions,
        IReadOnlyList<AtariClassicVideoCapability> video,
        IReadOnlyList<AtariClassicAudioCapability> audio,
        IReadOnlyList<AtariClassicStorageCapability> storage,
        IReadOnlyList<AtariClassicPortDefinition> ports,
        IReadOnlyList<AtariFirmwareKind> firmware,
        IReadOnlyList<AtariMediaKind> media) =>
        new(model, stableModelId, displayNameResourceKey, core, cpus, cpuFrequencyHz, mainMemoryBytes,
            regions, video, audio, storage, ports, firmware, media);

    internal static AtariClassicModelDefinition CreateEightBit(
        AtariMachineModel model,
        string modelId,
        string resourceKey,
        long memoryBytes,
        IReadOnlyList<AtariClassicCpu> cpus,
        IReadOnlyList<AtariClassicRegion> regions,
        IReadOnlyList<AtariClassicVideoCapability> video,
        IReadOnlyList<AtariClassicAudioCapability> audio,
        IReadOnlyList<AtariClassicStorageCapability> storage,
        IReadOnlyList<AtariClassicPortDefinition> ports,
        IReadOnlyList<AtariFirmwareKind> firmware,
        IReadOnlyList<AtariMediaKind> media) =>
        Create(model, modelId, resourceKey, AtariCoreKind.Atari800,
            AtariClassicModelConstants.Atari8BitCpuFrequencyHz, memoryBytes, cpus, regions,
            video, audio, storage, ports, firmware, media);

    internal static AtariClassicModelDefinition CreateJaguar(
        AtariMachineModel model,
        string modelId,
        string resourceKey,
        IReadOnlyList<AtariClassicCpu> cpus,
        IReadOnlyList<AtariClassicRegion> regions,
        IReadOnlyList<AtariClassicVideoCapability> video,
        IReadOnlyList<AtariClassicAudioCapability> audio,
        IReadOnlyList<AtariClassicPortDefinition> ports,
        IReadOnlyList<AtariFirmwareKind> firmware,
        IReadOnlyList<AtariClassicStorageCapability> storage,
        IReadOnlyList<AtariMediaKind> media) =>
        Create(model, modelId, resourceKey, AtariCoreKind.VirtualJaguar,
            AtariClassicModelConstants.JaguarCpuFrequencyHz, AtariClassicModelConstants.TwoMibibytes,
            cpus, regions, video, audio, storage, ports, firmware, media);

    internal static bool IsFirmwareCompatible(AtariClassicModelDefinition definition, AtariFirmwareKind kind) =>
        definition.Firmware.Contains(kind);

    internal static bool IsMediaCompatible(AtariClassicModelDefinition definition, AtariMediaKind kind,
        EmulationMediaSlot slot) => definition.Media.Contains(kind) && IsSlotCompatible(kind, slot);

    private static bool IsSlotCompatible(AtariMediaKind kind, EmulationMediaSlot slot) => kind switch
    {
        AtariMediaKind.Floppy => slot is >= EmulationMediaSlot.Floppy0 and <= EmulationMediaSlot.Floppy3,
        AtariMediaKind.Cassette => slot == EmulationMediaSlot.Cassette0,
        AtariMediaKind.Cartridge => slot == EmulationMediaSlot.Cartridge0,
        AtariMediaKind.CompactDisc => slot == EmulationMediaSlot.Cd0,
        _ => false
    };
}
