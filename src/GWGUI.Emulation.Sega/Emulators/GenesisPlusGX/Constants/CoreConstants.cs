using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Constants;

internal static class CoreConstants
{
    internal const string Id = "genesisplusgx";
    internal const string DisplayName = "Genesis Plus GX";
    internal const string LibraryName = "Genesis Plus GX";
    internal const string LibraryFileName = "genesis_plus_gx_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/genesis_plus_gx_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.genesisplusgx.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".m3u", ".mdx", ".md", ".smd", ".gen", ".bin", ".cue", ".iso", ".chd", ".bms", ".sms", ".gg", ".sg", ContentConstants.Sc3000SourceExtension, ".68k", ".sgd"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Sg1000, ModelConstants.Sc3000, ModelConstants.MarkIII, ModelConstants.MasterSystem, ModelConstants.GameGear, ModelConstants.MegaDrive, ModelConstants.MegaCd, ModelConstants.Pico }), MachineOptions: MachineOptionConstants.All, PrepareContent: Functions.ContentFunctions.Prepare);
}

