using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class CoreReleaseConstants
{
    internal const string HttpsBuildbotLibretroComNightlyWindowsX8664LatestPcsx2LibretroDllZip = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pcsx2_libretro.dll.zip";
    internal const string OptionLibretroDll = "pcsx2_libretro.dll";
    internal const string CoreJson = "core.json";
    internal const string Unknown = "unknown";
    internal const string Version = "version";
    internal const string YyyyMMddHHmm = "yyyyMMdd-HHmm";
    internal const string Latest = "latest";
    internal const string Pcsx2Latest = "PCSX2 · latest";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string X64 = "x64";
}
