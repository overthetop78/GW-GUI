using GWGUI.Emulation.Philips.Emulators.Common.Contracts;
using GWGUI.Emulation.Philips.Emulators.Common.Functions;
namespace GWGUI.Emulation.Philips.Emulators.O2em.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "o2em";
    internal const string LibraryName = "O2EM";
    internal const string DllName = "o2em_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/libretro-o2em";
    internal const string InspectedRevision = "679d6fec04963f6e70a7ec217e3d0ebb1fe472fc";
    internal const string DescriptionResourceKey = "Emulation.Emulator.o2em.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.O2em, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.Odyssey2, MachineModel.VideopacG7000)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
