namespace GWGUI.Emulation.Atari.Common.Machines.AtariClassic.Contracts;

public sealed record ClassicModelDefinition(
    MachineModel Model,
    string StableModelId,
    string DisplayNameResourceKey,
    Emulator Core,
    IReadOnlyList<ClassicCpu> Cpus,
    long DefaultCpuFrequencyHz,
    long MainMemoryBytes,
    IReadOnlyList<ClassicRegion> Regions,
    IReadOnlyList<ClassicVideoCapability> Video,
    IReadOnlyList<ClassicAudioCapability> Audio,
    IReadOnlyList<ClassicStorageCapability> Storage,
    IReadOnlyList<ClassicPortDefinition> Ports,
    IReadOnlyList<FirmwareCategory> Firmware,
    IReadOnlyList<MediaCategory> Media)
{
    public ClassicCpu DefaultCpu => Cpus.First();
    public ClassicRegion DefaultRegion => Regions.First();
}

public sealed record ClassicPortDefinition(ClassicPortCapability Capability, int Count);

