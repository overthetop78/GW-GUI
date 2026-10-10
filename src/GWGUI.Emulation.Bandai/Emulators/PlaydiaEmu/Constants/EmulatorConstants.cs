namespace GWGUI.Emulation.Bandai.Emulators.PlaydiaEmu.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.PlaydiaEmu, Id, LibraryName, DllName, SourceUri, InspectedRevision,
        MachineModel.Playdia)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "playdiaemu";
    internal const string LibraryName = "PlaydiaEmu";
    internal const string DllName = "playdiaemu_libretro.dll";
    internal const string SourceUri = "https://github.com/AloysHF/PlaydiaEmu";
    internal const string InspectedRevision = "6e758408ddffafe407282473f3904c1699a4cea8";
    internal const string DescriptionResourceKey = "Emulation.Emulator.playdiaemu.Description";
}
