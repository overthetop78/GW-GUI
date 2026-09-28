using GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;

internal static class CoreReleaseConstants
{
    internal const string HttpsBuildbotLibretroComNightlyWindowsX8664LatestMgbaLibretroDllZip = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mgba_libretro.dll.zip";
    internal const string OptionLibretroDll = "mgba_libretro.dll";
    internal const string CoreJson = "core.json";
    internal const string Unknown = "unknown";
    internal const string Version = "version";
    internal const string YyyyMMddHHmm = "yyyyMMdd-HHmm";
    internal const string Latest = "latest";
    internal const string MgbaLatest = "mGBA · latest";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string X64 = "x64";
}
