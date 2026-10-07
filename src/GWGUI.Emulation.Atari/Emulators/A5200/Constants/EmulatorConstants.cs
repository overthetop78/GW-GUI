namespace GWGUI.Emulation.Atari.Emulators.A5200.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "a5200";
    internal const string LibraryName = "a5200";
    internal const string LibraryFile = "a5200_libretro.dll";
    internal const string SourceUrl = "https://github.com/libretro/a5200";
    internal const string InspectedRevision = "40c6f2f1ad4a3145b328d5baaf010fae6c7e752b";
    internal const string CartridgeExtension = "a52";
    internal const string BinaryExtension = "bin";
    internal const bool SupportsCartridgeRegion = false;
    internal static readonly IReadOnlySet<string> CartridgeExtensions =
        new HashSet<string>([CartridgeExtension, BinaryExtension], StringComparer.OrdinalIgnoreCase);
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.A5200, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Atari5200);
}
