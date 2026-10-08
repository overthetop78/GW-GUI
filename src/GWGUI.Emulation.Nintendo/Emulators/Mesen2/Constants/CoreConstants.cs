using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Mesen2.Constants;

internal static class CoreConstants
{
    internal const string Id = "mesen2";
    internal const string DisplayName = "Mesen2";
    internal const string LibraryName = "Mesen2";
    internal const string LibraryFileName = "mesen2_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mesen2_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.mesen2.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nes", ".fds", ".unf", ".unif", ".sfc", ".smc", ".gb", ".gbc", ".gba", ".pce", ".sgx", ".cue", ".sms", ".gg", ".ws", ".wsc"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes, ModelConstants.GameBoy, ModelConstants.Snes, ModelConstants.GameBoyColor, ModelConstants.FamicomDisk }));
}
