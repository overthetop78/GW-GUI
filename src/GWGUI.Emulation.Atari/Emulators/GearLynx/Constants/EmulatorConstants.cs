namespace GWGUI.Emulation.Atari.Emulators.GearLynx.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "gearlynx";
    internal const string LibraryName = "Gearlynx";
    internal const string LibraryFile = "gearlynx_libretro.dll";
    internal const string SourceUrl = "https://github.com/drhelius/Gearlynx";
    internal const string InspectedRevision = "c125b33850abf394abb3049b956535a2184d4fe9";
    internal static readonly IReadOnlySet<string> CartridgeExtensions = new HashSet<string>(
        [AtariLynxModelConstants.LnxExtension, AtariLynxModelConstants.LyxExtension,
            AtariLynxModelConstants.ObjectExtension, AtariLynxModelConstants.BinaryExtension], StringComparer.OrdinalIgnoreCase);
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.GearLynx, Id, LibraryName, LibraryFile, SourceUrl, InspectedRevision, MachineModel.Lynx)
        with { DescriptionResourceKey = AtariLynxModelConstants.EmulatorDescriptionResourceKey };
}
