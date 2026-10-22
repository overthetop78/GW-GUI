using GWGUI.Emulation.Fairchild.Emulators.Common.Contracts;
using GWGUI.Emulation.Fairchild.Common.Functions;
namespace GWGUI.Emulation.Fairchild.Emulators.FreeChaF.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "freechaf";
    internal const string LibraryName = "FreeChaF";
    internal const string DllName = "freechaf_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/FreeChaF";
    internal const string InspectedRevision = "76c7a84f1f7e80f3e6f2bba96fe100cb24e99124";
    internal const string DescriptionResourceKey = "Emulation.Emulator.freechaf.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.FreeChaF, Id, LibraryName, DllName, SourceUri, InspectedRevision, MachineModel.ChannelF, MachineModel.ChannelFSystemII)
        with { DescriptionResourceKey = DescriptionResourceKey };
}
