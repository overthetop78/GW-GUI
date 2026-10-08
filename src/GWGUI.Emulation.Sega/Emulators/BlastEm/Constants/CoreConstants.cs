using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.BlastEm.Constants;

internal static class CoreConstants
{
    internal const string Id = "blastem";
    internal const string DisplayName = "BlastEm";
    internal const string LibraryName = "BlastEm";
    internal const string LibraryFileName = "blastem_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/blastem_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.blastem.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".md", ".gen", ".smd", ".32x", ".sms", ".gg", ".sg", ".sg1", ".sc", ".sc3", ".sf7", ".col", ".cue", ".toc", ".iso", ".chd", ".vgm", ".vgz", ".flac", ".wav", ".bin", ".rom", ".gz"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaDrive }), MachineOptions: MachineOptionConstants.All);
}
