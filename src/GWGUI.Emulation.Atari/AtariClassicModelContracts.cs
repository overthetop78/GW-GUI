namespace GWGUI.Emulation.Atari;

public enum AtariClassicCpu
{
    Mos6502C,
    Mos6507,
    Sally6502C,
    Wdc65Sc02,
    Motorola68000,
    TomGraphicsProcessor,
    JerrySignalProcessor
}

public enum AtariClassicRegion
{
    Ntsc,
    Pal,
    RegionFree
}

public enum AtariClassicVideoCapability
{
    Antic,
    Ctia,
    Gtia,
    Tia,
    Maria,
    Suzy,
    Mikey,
    Tom
}

public enum AtariClassicAudioCapability
{
    Pokey,
    Tia,
    CartridgePokey,
    Mikey,
    Jerry
}

public enum AtariClassicStorageCapability
{
    Floppy,
    Cassette,
    Cartridge,
    ExecutableFile,
    CompactDisc
}

public enum AtariClassicPortCapability
{
    Keyboard,
    Joystick,
    AnalogJoystick,
    Paddle,
    DrivingController,
    NumericKeypad,
    LightGun,
    ProLineController,
    EnhancedController
}

public sealed record AtariClassicPortDefinition(AtariClassicPortCapability Capability, int Count);

public sealed record AtariClassicModelDefinition(
    AtariMachineModel Model,
    string StableModelId,
    string DisplayNameResourceKey,
    AtariCoreKind Core,
    IReadOnlyList<AtariClassicCpu> Cpus,
    long DefaultCpuFrequencyHz,
    long MainMemoryBytes,
    IReadOnlyList<AtariClassicRegion> Regions,
    IReadOnlyList<AtariClassicVideoCapability> Video,
    IReadOnlyList<AtariClassicAudioCapability> Audio,
    IReadOnlyList<AtariClassicStorageCapability> Storage,
    IReadOnlyList<AtariClassicPortDefinition> Ports,
    IReadOnlyList<AtariFirmwareKind> Firmware,
    IReadOnlyList<AtariMediaKind> Media)
{
    public AtariClassicCpu DefaultCpu => Cpus.First();
    public AtariClassicRegion DefaultRegion => Regions.First();
}
