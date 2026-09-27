using System.Globalization;

namespace GWGUI.Emulation.Atari.Common.Machines.AtariST.Contracts;

public sealed record StModelDefinition(
    MachineModel Model,
    string TechnicalMachineId,
    string DisplayNameResourceKey,
    StCpu DefaultCpu,
    IReadOnlyList<StCpu> Cpus,
    IReadOnlyList<StFpu> Fpus,
    IReadOnlyList<int> CpuFrequenciesMhz,
    IReadOnlyList<StCpuPrecision> CpuPrecisions,
    IReadOnlyList<int> MainMemoryKib,
    IReadOnlyList<int> AlternateMemoryMib,
    IReadOnlyList<string> TosVersions,
    IReadOnlyList<StRegion> Regions,
    IReadOnlyList<StVideoCapability> Video,
    IReadOnlyList<StAudioCapability> Audio,
    IReadOnlyList<StStorageCapability> Storage,
    IReadOnlyList<StPortCapability> Ports)
{
    public StFpu DefaultFpu => Fpus.First();
    public int DefaultCpuFrequencyMhz => CpuFrequenciesMhz.First();
    public StCpuPrecision DefaultCpuPrecision => CpuPrecisions.First();
    public int DefaultMainMemoryKib => MainMemoryKib.First();
    public int DefaultAlternateMemoryMib => AlternateMemoryMib.First();
    public string RecommendedTosVersion => TosVersions.Last();
    public StRegion DefaultRegion => StModelFunctions.Region(CultureInfo.CurrentUICulture);
}

internal sealed record TosHeader(string Version, StRegion Region, TosVariant Variant,
    long ImageSize);
