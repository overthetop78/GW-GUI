using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.PokeMini.Constants;

internal static class CoreConstants
{
    internal const string Id = "pokemini";
    internal const string DisplayName = "PokeMini";
    internal const string LibraryName = "PokeMini";
    internal const string LibraryFileName = "pokemini_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pokemini_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pokemini.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".min"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PokemonMini }));
}
