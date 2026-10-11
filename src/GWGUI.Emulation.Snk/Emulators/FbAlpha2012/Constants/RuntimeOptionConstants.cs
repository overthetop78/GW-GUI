namespace GWGUI.Emulation.Snk.Emulators.FbAlpha2012.Constants;

internal static class RuntimeOptionConstants
{
    internal const string AllowDepth32 = "fbneo-allow-depth-32";
    internal const string VerticalMode = "fbneo-vertical-mode";
    internal const string Frameskip = "fbneo-frameskip";
    internal const string CpuSpeedAdjust = "fbneo-cpu-speed-adjust";
    internal const string DiagnosticInput = "fbneo-diagnostic-input";
    internal const string Hiscores = "fbneo-hiscores";
    internal const string Samplerate = "fbneo-samplerate";
    internal const string SampleInterpolation = "fbneo-sample-interpolation";
    internal const string FmInterpolation = "fbneo-fm-interpolation";
    internal const string AnalogSpeed = "fbneo-analog-speed";
    internal const string NeoGeoMode = "fbneo-neogeo-mode";
    internal const string DipSwitchPrefix = "fbalpha2012_dipswitch_";
    internal const string Aes = "AES";
    internal const string Mvs = "MVS";
    internal const string UniverseBios = "UNIBIOS";
    internal const string DipSwitch = "DIPSWITCH";
    internal const string SystemArchive = "neogeo.zip";
    internal const bool NeedsFullPath = true;
    internal const uint ClassicControllerDevice = InputConstants.AnalogDevice;
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
}
