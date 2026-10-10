namespace GWGUI.Emulation.Apple.Emulators.AppleWin.Constants;

internal static class EmulatorConstants
{
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.AppleWin, Id, LibraryName, DllName, SourceUri, InspectedRevision,
        MachineModel.AppleII, MachineModel.AppleIIPlus, MachineModel.AppleIIJPlus,
        MachineModel.AppleIIe, MachineModel.AppleIIeEnhanced)
        with { DescriptionResourceKey = DescriptionResourceKey };
    internal const string Id = "applewin";
    internal const string LibraryName = "AppleWin";
    internal const string DllName = "applewin_libretro.dll";
    internal const string SourceUri = "https://github.com/audetto/AppleWin";
    internal const string InspectedRevision = "7062ae417896da6050e2e39030215062bb4672ee";
    internal const string DescriptionResourceKey = "Emulation.Emulator.applewin.Description";
}
