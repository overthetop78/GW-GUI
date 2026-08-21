namespace GWGUI.Emulation.Atari;

public sealed record AtariStModelDefinition(
    AtariMachineModel Model,
    string TechnicalMachineId,
    string DisplayNameResourceKey,
    AtariStCpu DefaultCpu,
    IReadOnlyList<AtariStCpu> Cpus,
    IReadOnlyList<AtariStFpu> Fpus,
    IReadOnlyList<int> CpuFrequenciesMhz,
    IReadOnlyList<AtariStCpuPrecision> CpuPrecisions,
    IReadOnlyList<int> MainMemoryKib,
    IReadOnlyList<int> AlternateMemoryMib,
    IReadOnlyList<string> TosVersions,
    IReadOnlyList<AtariStRegion> Regions,
    IReadOnlyList<AtariStVideoCapability> Video,
    IReadOnlyList<AtariStAudioCapability> Audio,
    IReadOnlyList<AtariStStorageCapability> Storage,
    IReadOnlyList<AtariStPortCapability> Ports)
{
    public AtariStFpu DefaultFpu => Fpus.First();
    public int DefaultCpuFrequencyMhz => CpuFrequenciesMhz.First();
    public AtariStCpuPrecision DefaultCpuPrecision => CpuPrecisions.First();
    public int DefaultMainMemoryKib => MainMemoryKib.First();
    public int DefaultAlternateMemoryMib => AlternateMemoryMib.First();
    public string RecommendedTosVersion => TosVersions.Last();
    public AtariStRegion DefaultRegion => AtariStRegion.UnitedStates;
}
