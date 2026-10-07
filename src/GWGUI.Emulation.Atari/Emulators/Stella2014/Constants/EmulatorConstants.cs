namespace GWGUI.Emulation.Atari.Emulators.Stella2014.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "stella2014";
    internal const string LibraryName = "Stella 2014";
    internal const string LibraryFile = "stella2014_libretro.dll";
    internal const string SourceUrl = "https://github.com/libretro/stella2014-libretro";
    internal const string InspectedRevision = "7d1361e407e63f29e52892655069e5fb4096e691";
    internal const bool SupportsCartridgeRegion = true;
    internal const string CartridgeExtension = "a26";
    internal const string BinaryExtension = "bin";
    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>([CartridgeExtension, BinaryExtension], StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Stella2014, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Atari2600)
        with { DescriptionResourceKey = Atari2600ModelConstants.EmulatorDescriptionResourceKey };
}
