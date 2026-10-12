using GWGUI.Emulation.Sharp.Emulators.Common.Contracts;
using GWGUI.Emulation.Sharp.Emulators.Common.Functions;
namespace GWGUI.Emulation.Sharp.Emulators.Px68k.Constants;
internal static class EmulatorConstants
{
    internal const string Id = "px68k";
    internal const string LibraryName = "PX68K";
    internal const string DllName = "px68k_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/px68k-libretro";
    internal const string InspectedRevision = "0ad84d7058a12b7db4f7f7a906e87fad4e2f26f6";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/px68k_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.px68k.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Px68k, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.X68000, MachineModel.X68000Xvi)with
    {
        DescriptionResourceKey = DescriptionResourceKey
    };
}