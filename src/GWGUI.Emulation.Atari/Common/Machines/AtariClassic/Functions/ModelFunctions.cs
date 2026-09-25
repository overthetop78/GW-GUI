using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.AtariClassic.Functions;

internal static class ClassicModelFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    internal static IReadOnlyDictionary<MachineModel, ClassicModelDefinition> Index(
        IReadOnlyList<ClassicModelDefinition> definitions)
    {
        var result = definitions.ToDictionary(definition => definition.Model);
        if (result.Count != definitions.Count)
            throw new InvalidOperationException(ErrorMessages.DuplicateClassicModelDefinition);
        return result;
    }

    internal static ClassicModelDefinition Create(
        MachineModel model,
        string stableModelId,
        string displayNameResourceKey,
        Emulator core,
        long cpuFrequencyHz,
        long mainMemoryBytes,
        IReadOnlyList<ClassicCpu> cpus,
        IReadOnlyList<ClassicRegion> regions,
        IReadOnlyList<ClassicVideoCapability> video,
        IReadOnlyList<ClassicAudioCapability> audio,
        IReadOnlyList<ClassicStorageCapability> storage,
        IReadOnlyList<ClassicPortDefinition> ports,
        IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<MediaCategory> media) =>
        new(model, stableModelId, displayNameResourceKey, core, cpus, cpuFrequencyHz, mainMemoryBytes,
            regions, video, audio, storage, ports, firmware, media);

    internal static ClassicModelDefinition CreateEightBit(
        MachineModel model,
        string modelId,
        string resourceKey,
        long memoryBytes,
        IReadOnlyList<ClassicCpu> cpus,
        IReadOnlyList<ClassicRegion> regions,
        IReadOnlyList<ClassicVideoCapability> video,
        IReadOnlyList<ClassicAudioCapability> audio,
        IReadOnlyList<ClassicStorageCapability> storage,
        IReadOnlyList<ClassicPortDefinition> ports,
        IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<MediaCategory> media) =>
        Create(model, modelId, resourceKey, Emulator.Atari800,
            ClassicModelConstants.Atari8BitCpuFrequencyHz, memoryBytes, cpus, regions,
            video, audio, storage, ports, firmware, media);

    internal static ClassicModelDefinition CreateJaguar(
        MachineModel model,
        string modelId,
        string resourceKey,
        IReadOnlyList<ClassicCpu> cpus,
        IReadOnlyList<ClassicRegion> regions,
        IReadOnlyList<ClassicVideoCapability> video,
        IReadOnlyList<ClassicAudioCapability> audio,
        IReadOnlyList<ClassicPortDefinition> ports,
        IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<ClassicStorageCapability> storage,
        IReadOnlyList<MediaCategory> media) =>
        Create(model, modelId, resourceKey, Emulator.VirtualJaguar,
            ClassicModelConstants.JaguarCpuFrequencyHz, ClassicModelConstants.TwoMibibytes,
            cpus, regions, video, audio, storage, ports, firmware, media);

    internal static bool IsFirmwareCompatible(ClassicModelDefinition definition, FirmwareCategory category) =>
        definition.Firmware.Contains(category);

    internal static bool IsMediaCompatible(ClassicModelDefinition definition, MediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Contains(category) && IsSlotCompatible(category, slot);

    private static bool IsSlotCompatible(MediaCategory category, EmulationMediaSlot slot) => category switch
    {
        MediaCategory.Floppy => slot.Category == EmulationMediaCategory.FloppyDrive && slot.Index is >= 0 and <= 3,
        MediaCategory.Cassette => slot == EmulationMediaSlot.Cassette0,
        MediaCategory.Cartridge => slot == EmulationMediaSlot.Cartridge0,
        MediaCategory.CompactDisc => slot == EmulationMediaSlot.Cd0,
        _ => false
    };
}
