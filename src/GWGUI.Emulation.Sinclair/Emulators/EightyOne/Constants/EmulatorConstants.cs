using EmulatorCatalogFunctions = GWGUI.Emulation.Sinclair.Common.Functions.EmulatorCatalogFunctions;
using GWGUI.Emulation.Sinclair.Emulators.Common.Functions;
using GWGUI.Emulation.Sinclair.Emulators.Common.Contracts;
namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "81";
    internal const string LibraryName = "EightyOne";
    internal const string DllName = "81_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/81-libretro";
    internal const string InspectedRevision = "fa7094910d040baa5fd8b11dbf6a1a618330ecd9";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/81_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.81.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.EightyOne, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, MachineModel.Zx81) with { DescriptionResourceKey = DescriptionResourceKey };
}
