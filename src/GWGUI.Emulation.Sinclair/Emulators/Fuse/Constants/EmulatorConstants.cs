namespace GWGUI.Emulation.Sinclair.Emulators.Fuse.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "fuse";
    internal const string LibraryName = "Fuse";
    internal const string DllName = "fuse_libretro.dll";
    internal const string SourceUri = "https://github.com/libretro/fuse-libretro";
    internal const string InspectedRevision = "e997e2bc32c888348f862f69f2c53babfedf7791";
    internal const string ArchiveUri = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fuse_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fuse.Description";
    internal static readonly EmulatorCatalogEntry Entry = EmulatorCatalogFunctions.Create(Emulator.Fuse, Id, LibraryName, DllName, new Uri(ArchiveUri, UriKind.Absolute), SourceUri, InspectedRevision, Enum.GetValues<MachineModel>().Where(model => ConfigurationFunctions.Family(model) == MachineFamily.Spectrum).ToArray()) with { DescriptionResourceKey = DescriptionResourceKey };
}
