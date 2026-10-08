using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.ParallelN64.Constants;

internal static class CoreConstants
{
    internal const string Id = "parallel_n64";
    internal const string DisplayName = "ParaLLEl N64";
    internal const string LibraryName = "ParaLLEl N64";
    internal const string LibraryFileName = "parallel_n64_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/parallel_n64_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.parallel_n64.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".n64", ".v64", ".z64", ".bin", ".u1", ".ndd", ".zip"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nintendo64 }));
}
