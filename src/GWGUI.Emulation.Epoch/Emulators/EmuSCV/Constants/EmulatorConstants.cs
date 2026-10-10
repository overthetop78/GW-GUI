namespace GWGUI.Emulation.Epoch.Emulators.EmuSCV.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "emuscv";
    internal const string LibraryName = "Libretro-EmuSCV";
    internal const string DllName = "emuscv_libretro.dll";
    internal const string SourceUri = "https://gitlab.com/MaaaX-EmuSCV/libretro-emuscv";
    internal const string InspectedRevision = "099feb250ad2d6c304aa79c39ea6223f0b1c9f08";
    internal const string DescriptionResourceKey = "Emulation.Emulator.emuscv.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(
        Emulator.EmuSCV, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.SuperCassetteVision, MachineModel.SuperCassetteVisionYeno, MachineModel.SuperCassetteVisionLady)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
