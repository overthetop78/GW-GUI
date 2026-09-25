using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;

internal static class ExternalCoreInstallerConstants
{
    internal const string Value96ebfcfc = "96ebfcfc";
    internal const string HttpsBuildbotLibretroComNightlyWindowsX8664LatestPuaeLibretroDllZip = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/puae_libretro.dll.zip";
    internal const string OptionLibretroDll = "puae_libretro.dll";
    internal const string Download = ".download";
    internal const string Extract = ".extract";
    internal const string TheOfficialAmigaCoreArchiveDoesNotContainPuaeLibretroDll = "The official Amiga core archive does not contain puae_libretro.dll.";
    internal const string X64 = "x64";
    internal const string CoreJson = "core.json";
}
