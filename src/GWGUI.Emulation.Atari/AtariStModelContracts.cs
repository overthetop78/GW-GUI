namespace GWGUI.Emulation.Atari;

public enum AtariStCpu
{
    Motorola68000,
    Motorola68030
}

public enum AtariStFpu
{
    None,
    Motorola68881,
    Motorola68882
}

public enum AtariStCpuPrecision
{
    Compatible,
    CycleExact
}

public enum AtariStRegion
{
    UnitedStates,
    Germany,
    France,
    UnitedKingdom,
    Spain,
    Italy,
    Sweden,
    Switzerland,
    Finland,
    Norway,
    CzechRepublic,
    Russia,
    Greece,
    Multilingual
}

public enum AtariStVideoCapability
{
    Pal,
    Ntsc,
    Monochrome,
    Blitter,
    EnhancedPalette,
    HardwareScrolling,
    TtShifter,
    Videl
}

public enum AtariStAudioCapability
{
    ProgrammableSoundGenerator,
    StereoDma,
    Microwire,
    DigitalSignalProcessor,
    Microphone
}

public enum AtariStStorageCapability
{
    FloppyDoubleDensity,
    FloppyHighDensity,
    Acsi,
    Ide,
    Scsi,
    GemdosDirectory
}

public enum AtariStPortCapability
{
    Keyboard,
    Mouse,
    Joystick,
    EnhancedJoystick,
    Midi,
    Parallel,
    Serial,
    Cartridge,
    LocalAreaNetwork,
    Vme
}

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
