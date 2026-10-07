namespace GWGUI.Emulation.Atari.Emulators.Stella.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "stella";
    internal const string LibraryName = "Stella";
    internal const string LibraryFile = "stella_libretro.dll";
    internal const string SourceUrl = "https://github.com/stella-emu/stella";
    internal const string InspectedRevision = "3e88432179e66b96841d2326a063eded019c3779";
    internal const bool SupportsCartridgeRegion = true;
    internal const string CartridgeExtension = "a26";
    internal const string BinaryExtension = "bin";
    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>([CartridgeExtension, BinaryExtension], StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Stella, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Atari2600)
        with { DescriptionResourceKey = Atari2600ModelConstants.EmulatorDescriptionResourceKey };
}
