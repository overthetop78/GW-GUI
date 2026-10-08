using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Gearsystem.Constants;

internal static class CoreConstants
{
    internal const string Id = "gearsystem";
    internal const string DisplayName = "Gearsystem";
    internal const string LibraryName = "Gearsystem";
    internal const string LibraryFileName = "gearsystem_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/gearsystem_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.gearsystem.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".sms", ".gg", ".sg", ".mv", ".bin", ".rom"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Sg1000, ModelConstants.MarkIII, ModelConstants.MasterSystem, ModelConstants.GameGear }), MachineOptions: MachineOptionConstants.All);
}
