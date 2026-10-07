namespace GWGUI.Emulation.Atari.Emulators.Holani.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "holani";
    internal const string LibraryName = "Holani";
    internal const string LibraryFile = "holani_libretro.dll";
    internal const string SourceUrl = "https://github.com/LLeny/holani-retro";
    internal const string InspectedRevision = "d0d79b4928e3d32695294142784c033ce7aeda9b";
    internal static readonly IReadOnlySet<string> CartridgeExtensions = new HashSet<string>(
        [AtariLynxModelConstants.LnxExtension, AtariLynxModelConstants.ObjectExtension], StringComparer.OrdinalIgnoreCase);
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.Holani, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Lynx)
        with { DescriptionResourceKey = AtariLynxModelConstants.EmulatorDescriptionResourceKey };
}
