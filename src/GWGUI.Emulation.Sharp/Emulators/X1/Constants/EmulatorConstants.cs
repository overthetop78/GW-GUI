using GWGUI.Emulation.Sharp.Emulators.Common.Contracts;
using GWGUI.Emulation.Sharp.Emulators.Common.Functions;
namespace GWGUI.Emulation.Sharp.Emulators.X1.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "x1";
    internal const string LibraryName = "x1";
    internal const string DllName = "x1_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/xmil-libretro";
    internal const string InspectedRevision = "3106aa54ffb244cca4019a6e95dacaa698781bc3";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/x1_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.x1.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.X1, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.X1, MachineModel.X1Turbo, MachineModel.X1TurboZ)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}