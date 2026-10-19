using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Exceptions;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

internal static class BeetlePcfxConstants
{
    internal const string Id = "beetle_pcfx_fast";
    internal const string DisplayName = "Beetle PC-FX";
    internal const string LibraryName = "Beetle PC-FX";
    internal const string DescriptionResourceKey = "Emulation.Emulator.beetle_pcfx_fast.Description";
    internal const string CoreHostCommand = "--nec-beetle-pcfx-core-host";
    internal const string InstalledLibraryName = "beetle_pcfx_fast_libretro.dll";
    internal const string ArchiveLibraryName = "mednafen_pcfx_libretro.dll";
    internal const string OfficialArchiveUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_pcfx_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialArchiveUrl), ArchiveLibraryName, InstalledLibraryName, DisplayName);
    internal static CoreDefinition CoreDefinition { get; } = new(
        Id, LibraryName, InstalledLibraryName,
        (media, contentDirectory, _) => BeetlePcfxStorageFunctions.PrepareContentPath(media, contentDirectory),
        FirmwareFileName: FirmwareCatalogConstants.PcFxBiosFileName,
        ErrorResourcePrefix: "Emulation.Error.BeetlePcfx",
        FullContentPathsRequired: BeetlePcfxExceptions.FullContentPathsRequired,
        LoadSaveRam: (data, size, directory, configuration) => PcFxBackupMemoryFunctions.Load(data, size, directory, BeetlePcfxStorageFunctions.ExternalCardPath(configuration)),
        SaveSaveRam: (data, size, directory, configuration) => PcFxBackupMemoryFunctions.Save(data, size, directory, BeetlePcfxStorageFunctions.ExternalCardPath(configuration)),
        TransportDisplayName: DisplayName);
}
