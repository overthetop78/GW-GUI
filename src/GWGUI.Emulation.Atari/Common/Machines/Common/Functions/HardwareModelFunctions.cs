using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static class HardwareModelFunctions
{
    internal static IReadOnlyList<T> Values<T>(params T[] values) => Array.AsReadOnly(values);

    internal static IReadOnlyDictionary<MachineModel, HardwareModelDefinition> Index(
        IReadOnlyList<HardwareModelDefinition> definitions)
    {
        var result = definitions.ToDictionary(definition => definition.Model);
        if (result.Count != definitions.Count)
            throw new InvalidOperationException(ErrorMessages.DuplicateHardwareModelDefinition);
        return result;
    }

    internal static HardwareModelDefinition Create(
        MachineModel model,
        string stableModelId,
        string displayNameResourceKey,
        Emulator emulator,
        long cpuFrequencyHz,
        long mainMemoryBytes,
        IReadOnlyList<HardwareCpu> cpus,
        IReadOnlyList<HardwareRegion> regions,
        IReadOnlyList<HardwareVideoCapability> video,
        IReadOnlyList<HardwareAudioCapability> audio,
        IReadOnlyList<HardwareStorageCapability> storage,
        IReadOnlyList<HardwarePortDefinition> ports,
        IReadOnlyList<FirmwareCategory> firmware,
        IReadOnlyList<MediaCategory> media) =>
        new(model, stableModelId, displayNameResourceKey, emulator, cpus, cpuFrequencyHz,
            mainMemoryBytes, regions, video, audio, storage, ports, firmware, media);

    internal static bool IsFirmwareCompatible(HardwareModelDefinition definition, FirmwareCategory category) =>
        definition.Firmware.Contains(category);

    internal static bool IsMediaCompatible(HardwareModelDefinition definition, MediaCategory category,
        EmulationMediaSlot slot) => definition.Media.Contains(category) && IsSlotCompatible(category, slot);

    private static bool IsSlotCompatible(MediaCategory category, EmulationMediaSlot slot) => category switch
    {
        MediaCategory.Floppy => slot == EmulationMediaSlot.Floppy0 || slot == EmulationMediaSlot.Floppy1
            || slot == EmulationMediaSlot.Floppy2 || slot == EmulationMediaSlot.Floppy3,
        MediaCategory.Cassette => slot == EmulationMediaSlot.Cassette0,
        MediaCategory.Cartridge => slot == EmulationMediaSlot.Cartridge0,
        MediaCategory.CompactDisc => slot == EmulationMediaSlot.Cd0,
        _ => false
    };
}
