namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;

public sealed record HardwareModelDefinition(
    MachineModel Model,
    string StableModelId,
    string DisplayNameResourceKey,
    Emulator Emulator,
    IReadOnlyList<HardwareCpu> Cpus,
    long DefaultCpuFrequencyHz,
    long MainMemoryBytes,
    IReadOnlyList<HardwareRegion> Regions,
    IReadOnlyList<HardwareVideoCapability> Video,
    IReadOnlyList<HardwareAudioCapability> Audio,
    IReadOnlyList<HardwareStorageCapability> Storage,
    IReadOnlyList<HardwarePortDefinition> Ports,
    IReadOnlyList<FirmwareCategory> Firmware,
    IReadOnlyList<MediaCategory> Media)
{
    public HardwareCpu DefaultCpu => Cpus.First();
    public HardwareRegion DefaultRegion => Regions.First();
}

public sealed record HardwarePortDefinition(HardwarePortCapability Capability, int Count);
