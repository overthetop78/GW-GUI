using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Functions;

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
        AtariEmulator core,
        long cpuFrequencyHz,
        long mainMemoryBytes,
        IReadOnlyList<AtariClassicCpu> cpus,
        IReadOnlyList<AtariClassicRegion> regions,
        IReadOnlyList<AtariClassicVideoCapability> video,
        IReadOnlyList<AtariClassicAudioCapability> audio,
        IReadOnlyList<AtariClassicStorageCapability> storage,
        IReadOnlyList<AtariClassicPortDefinition> ports,
        IReadOnlyList<AtariFirmwareCategory> firmware,
        IReadOnlyList<AtariMediaCategory> media) =>
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
        IReadOnlyList<AtariFirmwareCategory> firmware,
        IReadOnlyList<AtariMediaCategory> media) =>
        Create(model, modelId, resourceKey, AtariEmulator.Atari800,
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
        IReadOnlyList<AtariFirmwareCategory> firmware,
        IReadOnlyList<AtariClassicStorageCapability> storage,
        IReadOnlyList<AtariMediaCategory> media) =>
        Create(model, modelId, resourceKey, AtariEmulator.VirtualJaguar,
            AtariClassicModelConstants.JaguarCpuFrequencyHz, AtariClassicModelConstants.TwoMibibytes,
            cpus, regions, video, audio, storage, ports, firmware, media);

    internal static bool IsFirmwareCompatible(AtariClassicModelDefinition definition, AtariFirmwareCategory category) =>
        definition.Firmware.Contains(category);

    internal static bool IsMediaCompatible(AtariClassicModelDefinition definition, AtariMediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Contains(category) && IsSlotCompatible(category, slot);

    private static bool IsSlotCompatible(AtariMediaCategory category, EmulationMediaSlot slot) => category switch
    {
        AtariMediaCategory.Floppy => slot.Category == EmulationMediaCategory.FloppyDrive && slot.Index is >= 0 and <= 3,
        AtariMediaCategory.Cassette => slot == EmulationMediaSlot.Cassette0,
        AtariMediaCategory.Cartridge => slot == EmulationMediaSlot.Cartridge0,
        AtariMediaCategory.CompactDisc => slot == EmulationMediaSlot.Cd0,
        _ => false
    };
}
