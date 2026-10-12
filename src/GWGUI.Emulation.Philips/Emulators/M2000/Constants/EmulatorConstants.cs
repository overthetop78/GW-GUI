using GWGUI.Emulation.Philips.Emulators.Common.Contracts;
using GWGUI.Emulation.Philips.Emulators.Common.Functions;
namespace GWGUI.Emulation.Philips.Emulators.M2000.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "m2000";
    internal const string LibraryName = "M2000";
    internal const string DllName = "m2000_libretro.dll";
    internal const string SourceUri = "https://github.com/p2000t/M2000";
    internal const string InspectedRevision = "da0b8cbef2007c907f5db2166847c3209643865d";
    internal const string DescriptionResourceKey = "Emulation.Emulator.m2000.Description";
    // v0.9.3 declares ten descriptors and omits Libretro's terminating null entry.
    internal const int InputDescriptorCount = 10;
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.M2000, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.P2000T) with { DescriptionResourceKey = DescriptionResourceKey };
}
