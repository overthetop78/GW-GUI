namespace GWGUI.Emulation.Atari.Emulators.Handy.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "handy";
    internal const string LibraryName = "Handy";
    internal const string LibraryFile = "handy_libretro.dll";
    internal const string SourceUrl = "https://github.com/libretro/libretro-handy";
    internal const string InspectedRevision = "bc55d462f0b2d6b073ea93dc552ebd73cec60fd1";
    internal static readonly IReadOnlySet<string> CartridgeExtensions = new HashSet<string>(
        [AtariLynxModelConstants.LnxExtension, AtariLynxModelConstants.LyxExtension,
            AtariLynxModelConstants.ObjectExtension], StringComparer.OrdinalIgnoreCase);
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Handy, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Lynx)
        with { DescriptionResourceKey = AtariLynxModelConstants.EmulatorDescriptionResourceKey };
}
