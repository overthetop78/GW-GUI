using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.PicoDrive.Constants;

internal static class CoreConstants
{
    internal const string Id = "picodrive";
    internal const string DisplayName = "PicoDrive";
    internal const string LibraryName = "PicoDrive";
    internal const string LibraryFileName = "picodrive_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/picodrive_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.picodrive.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".bin", ".gen", ".smd", ".md", ".32x", ".cue", ".iso", ".chd", ".m3u", ".sms", ".gg", ".sg", ".sc", ".68k", ".sgd", ".pco", ".vgm", ".vgz"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Sg1000, ModelConstants.Sc3000, ModelConstants.MasterSystem, ModelConstants.GameGear, ModelConstants.MegaDrive, ModelConstants.MegaCd, ModelConstants.ThirtyTwoX, ModelConstants.Pico }));
}
