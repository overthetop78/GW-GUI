namespace GWGUI.Emulation.Snk.Emulators.FbAlpha.Constants;

internal static class RuntimeOptionConstants
{
    internal const int NativeTrackTableEntries = 100;
    internal const int NativeMaximumTrackNumber = NativeTrackTableEntries - 1;
    internal const int SubchannelQOffset = 12;
    internal const int SubchannelTrackOffset = SubchannelQOffset + 1;
    internal const int SubchannelIndexOffset = SubchannelQOffset + 2;
    internal const byte SubchannelAddressMask = 1;
    internal const int BcdDigitBits = 4;
    internal const int BcdLowDigitMask = 15;
    internal const int BcdDecimalBase = 10;
    internal const string CueExtension = ".cue";
    internal const string CcdExtension = ".ccd";
    internal const string CcdImageExtension = ".img";
    internal const string CcdSubchannelExtension = ".sub";
    internal const string AllowDepth32 = "fba-allow-depth-32";
    internal const string VerticalMode = "fba-vertical-mode";
    internal const string Frameskip = "fba-frameskip";
    internal const string CpuSpeedAdjust = "fba-cpu-speed-adjust";
    internal const string DiagnosticInput = "fba-diagnostic-input";
    internal const string Hiscores = "fba-hiscores";
    internal const string Samplerate = "fba-samplerate";
    internal const string SampleInterpolation = "fba-sample-interpolation";
    internal const string FmInterpolation = "fba-fm-interpolation";
    internal const string AnalogSpeed = "fba-analog-speed";
    internal const string NeoGeoMode = "fba-neogeo-mode";
    internal const string DipSwitchPrefix = "fba-dipswitch-";
    internal const string Aes = "AES";
    internal const string Mvs = "MVS";
    internal const string UniverseBios = "UNIBIOS";
    internal const string DipSwitch = "DIPSWITCH";
    internal const string SystemArchive = "neogeo.zip";
    internal const string CdArchive = "neocdz.zip";
    internal const bool NeedsFullPath = true;
    internal const uint CdSubsystem = 11;
    internal const nuint CdContentCount = 1;
    internal const uint ClassicControllerDevice = InputConstants.AnalogDevice;
    internal const string CdDriver = "neocdz";
    internal static readonly IReadOnlyList<string> CartridgeExtensions = Array.AsReadOnly(new[] { ".zip", ".7z" });
    internal static readonly IReadOnlyList<string> CdExtensions = Array.AsReadOnly(new[] { ".cue", ".ccd" });
}
