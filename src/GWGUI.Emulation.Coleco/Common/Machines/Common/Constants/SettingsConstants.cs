namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Constants;

internal static class SettingsConstants
{
    internal const string AudioEnabled = "coleco_audio_enabled";
    internal const string AudioOutput = "gwgui_coleco_audio_output";
    internal const string AudioLatency = "gwgui_coleco_audio_latency";
    internal const string DefaultOutput = "default";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor";
    internal const string ResourceCpuModel = "Emulation.Cpu.Model";
    internal const string ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal";
    internal const string ResourceMemoryMain = "Emulation.Memory.Main";
    internal const string ResourceDisplayMemory = "Emulation.Coleco.Memory.Display";
    internal const string ResourceVideoResolution = "Emulation.Video.Resolution";
    internal const string CpuModel = "coleco_cpu_model";
    internal const string CpuFrequency = "coleco_cpu_frequency";
    internal const string MainMemory = "coleco_main_memory";
    internal const string DisplayMemory = "coleco_display_memory";
    internal const string DisplayResolution = "coleco_display_resolution";
    internal const int BytesPerKibibyte = 1024;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const double HertzPerKilohertz = 1_000;
    internal const string MegahertzUnit = "MHz", KilohertzUnit = "kHz", ByteUnit = "B", KibibyteUnit = "KiB";
    internal const string ResourceFirmware = "Emulation.Firmware.Rom.System";
    internal const string ResourceNativeVideo = "Emulation.Tab.Video";
    internal const string ResourceNativeControls = "Emulation.Coleco.Controls.Native";
    internal const string ResourceNativeSystem = "Emulation.Coleco.System.Native";
    internal const string ResourceAudio = "Emulation.Audio";
    internal const string ResourceAudioEnabled = "Emulation.Audio.Enabled";
    internal const string ResourceAudioOutput = "Emulation.Audio.Output";
    internal const string ResourceAudioLatency = "Emulation.Audio.Latency";
    internal const string ResourceAudioVolume = "Explorer.Volume";
    internal const string ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const string MillisecondsUnit = "ms", PercentUnit = "%";
    internal static readonly IReadOnlyList<string> CartridgeExtensions = [".col", ".cv", ".bin", ".rom", ".zip"];
    internal static readonly IReadOnlyList<string> FloppyExtensions = [".dsk"];
    internal static readonly IReadOnlyList<string> DataPackExtensions = [".ddp"];
}
