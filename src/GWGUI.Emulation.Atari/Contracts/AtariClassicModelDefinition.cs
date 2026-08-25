namespace GWGUI.Emulation.Atari.Contracts;

public sealed record AtariClassicModelDefinition(
    AtariMachineModel Model,
    string StableModelId,
    string DisplayNameResourceKey,
    AtariEmulator Core,
    IReadOnlyList<AtariClassicCpu> Cpus,
    long DefaultCpuFrequencyHz,
    long MainMemoryBytes,
    IReadOnlyList<AtariClassicRegion> Regions,
    IReadOnlyList<AtariClassicVideoCapability> Video,
    IReadOnlyList<AtariClassicAudioCapability> Audio,
    IReadOnlyList<AtariClassicStorageCapability> Storage,
    IReadOnlyList<AtariClassicPortDefinition> Ports,
    IReadOnlyList<AtariFirmwareCategory> Firmware,
    IReadOnlyList<AtariMediaCategory> Media)
{
    public AtariClassicCpu DefaultCpu => Cpus.First();
    public AtariClassicRegion DefaultRegion => Regions.First();
}
