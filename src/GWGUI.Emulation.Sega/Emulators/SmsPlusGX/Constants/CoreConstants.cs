using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.SmsPlusGX.Constants;

internal static class CoreConstants
{
    internal const string Id = "smsplus";
    internal const string DisplayName = "SMS Plus GX";
    internal const string LibraryName = "SMS Plus GX";
    internal const string LibraryFileName = "smsplus_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/smsplus_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.smsplus.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sms", ".bin", ".rom", ".col", ".gg", ".sg"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MasterSystem, ModelConstants.GameGear }));
}
