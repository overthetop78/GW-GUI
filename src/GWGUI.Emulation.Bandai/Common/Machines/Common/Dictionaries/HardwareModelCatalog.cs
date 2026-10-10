namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Dictionaries;

public static class HardwareModelCatalog
{
    private const string WonderSwanCpuResource = "Emulation.Bandai.Cpu.V30Mz";
    private const string TamagotchiCpuResource = "Emulation.Bandai.Cpu.E0C6S46";
    private const long WonderSwanClockHz = 3_072_000;
    private const long WonderSwanMemoryBytes = 16_384;
    private const long WonderSwanColorMemoryBytes = 65_536;
    private const int WonderSwanDisplayWidth = 224;
    private const int WonderSwanDisplayHeight = 144;
    private const long TamagotchiClockHz = 32_768;
    private const long TamagotchiMemoryBytes = 320;
    private const long TamagotchiDisplayMemoryBytes = 80;
    private const int TamagotchiDisplayWidth = 32;
    private const int TamagotchiDisplayHeight = 16;

    // Physical specifications, not the size of the buffers allocated by a core.
    // WonderSwan: wonderful.asie.pl/wiki/doku.php?id=wswan:platform_overview
    // Tamagotchi: Epson E0C6S46 technical manual, sections 1.1 and 2.3;
    // LCD dimensions and MCU identity: github.com/jcrona/tamalib.
    public static HardwareModelDefinition? Get(MachineModel model) => model switch
    {
        MachineModel.WonderSwan => WonderSwan(WonderSwanMemoryBytes),
        MachineModel.WonderSwanColor or MachineModel.SwanCrystal => WonderSwan(WonderSwanColorMemoryBytes),
        MachineModel.TamagotchiP1 => new(TamagotchiCpuResource, TamagotchiClockHz,
            TamagotchiMemoryBytes, TamagotchiDisplayWidth, TamagotchiDisplayHeight,
            TamagotchiDisplayMemoryBytes),
        // PlaydiaEmu plays discs through HLE; it does not emulate the physical CPUs/RAM.
        MachineModel.Playdia => null,
        _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
    };

    private static HardwareModelDefinition WonderSwan(long memoryBytes) =>
        new(WonderSwanCpuResource, WonderSwanClockHz, memoryBytes,
            WonderSwanDisplayWidth, WonderSwanDisplayHeight);
}
