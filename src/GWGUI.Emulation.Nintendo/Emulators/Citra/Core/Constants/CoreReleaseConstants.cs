using GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;

internal static class CoreReleaseConstants
{
    internal const string HttpsBuildbotLibretroComNightlyWindowsX8664LatestCitraLibretroDllZip = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/citra_libretro.dll.zip";
    internal const string OptionLibretroDll = "citra_libretro.dll";
    internal const string CoreJson = "core.json";
    internal const string Unknown = "unknown";
    internal const string Version = "version";
    internal const string YyyyMMddHHmm = "yyyyMMdd-HHmm";
    internal const string Latest = "latest";
    internal const string CitraLatest = "Citra · latest";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string X64 = "x64";
}
