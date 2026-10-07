namespace GWGUI.Emulation.Atari.Emulators.Stella2023.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "stella2023";
    internal const string LibraryName = "Stella 2023";
    internal const string LibraryFile = "stella2023_libretro.dll";
    internal const string SourceUrl = "https://github.com/libretro/stella2023";
    internal const string InspectedRevision = "ba52c43b9eda950eb0c0eec69cda9b17dee8c39b";
    internal const bool SupportsCartridgeRegion = true;
    internal const string CartridgeExtension = "a26";
    internal const string BinaryExtension = "bin";
    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>([CartridgeExtension, BinaryExtension], StringComparer.OrdinalIgnoreCase);

    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Stella2023, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Atari2600)
        with { DescriptionResourceKey = Atari2600ModelConstants.EmulatorDescriptionResourceKey };
}
