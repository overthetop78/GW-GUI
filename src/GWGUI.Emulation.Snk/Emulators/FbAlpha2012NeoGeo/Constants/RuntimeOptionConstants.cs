using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012NeoGeo.Constants;

internal static class RuntimeOptionConstants
{
    internal const string Aspect = "fbalpha2012_neogeo_aspect";
    internal const string CpuSpeedAdjust = "fbalpha2012_neogeo_cpu_speed_adjust";
    internal const string DiagnosticInput = "fbalpha2012_neogeo_diagnostic_input";
    internal const string LowpassFilter = "fbalpha2012_neogeo_lowpass_filter";
    internal const string LowpassRange = "fbalpha2012_neogeo_lowpass_range";
    internal const string Frameskip = "fbalpha2012_neogeo_frameskip";
    internal const string FrameskipThreshold = "fbalpha2012_neogeo_frameskip_threshold";
    internal const string NeoGeoMode = "fbalpha2012_neogeo_neogeo_mode";
    internal const string DipSwitchPrefix = "fbalpha2012_neogeo_dipswitch_";
    internal const string Aes = "AES";
    internal const string Mvs = "MVS";
    internal const string UniverseBios = "UNIBIOS";
    internal const string DipSwitch = "DIPSWITCH";
    internal const string ManualFrameskip = "Manual";
    internal const string FilterEnabled = "enabled";
    internal const string SystemArchive = "neogeo.zip";
    internal const bool NeedsFullPath = true;
    internal const uint ClassicControllerDevice = InputConstants.JoypadDevice;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
}
