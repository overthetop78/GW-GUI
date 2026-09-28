using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Dolphin.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Constants;

internal static class CoreReleaseConstants
{
    internal const string HttpsBuildbotLibretroComNightlyWindowsX8664LatestDolphinLibretroDllZip = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/dolphin_libretro.dll.zip";
    internal const string OptionLibretroDll = "dolphin_libretro.dll";
    internal const string CoreJson = "core.json";
    internal const string Unknown = "unknown";
    internal const string Version = "version";
    internal const string YyyyMMddHHmm = "yyyyMMdd-HHmm";
    internal const string Latest = "latest";
    internal const string DolphinLatest = "Dolphin · latest";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string X64 = "x64";
}
