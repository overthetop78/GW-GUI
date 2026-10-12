namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Constants;
internal static class SettingsConstants
{
    internal const string AudioEnabled = "wenquxing_audio_enabled", AudioOutput = "gwgui_wenquxing_audio_output", AudioLatency = "gwgui_wenquxing_audio_latency";
    internal const string CpuModel = "wenquxing_cpu_model", CpuFrequency = "wenquxing_cpu_frequency", MainMemory = "wenquxing_main_memory";
    internal const string VideoDisplay = "wenquxing_display", Resolution = "wenquxing_resolution", ResourceDisplay = "Emulation.Video.Settings.Display", ResourceResolution = "Emulation.Video.Resolution";
    internal const string DisplayResolution = "160 × 80";
    internal const string DefaultOutput = "default";
    internal const string ResourceAudio = "Emulation.Audio", ResourceAudioEnabled = "Emulation.Audio.Enabled", ResourceAudioOutput = "Emulation.Audio.Output", ResourceAudioLatency = "Emulation.Audio.Latency", ResourceAudioVolume = "Explorer.Volume", ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor", ResourceCpuModel = "Emulation.Cpu.Model", ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal", ResourceMemoryMain = "Emulation.Memory.Main";
    internal const int BytesPerKibibyte = 1024, DefaultLatency = 50, DefaultVolume = 100;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const string MegahertzUnit = "MHz", KibibyteUnit = "KiB", MillisecondsUnit = "ms", PercentUnit = "%";
    internal static readonly IReadOnlyList<int> Latencies = [20, 30, 40, 50, 75, 100, 150, 200];
    internal static readonly IReadOnlyList<int> Volumes = [0, 25, 50, 75, 100];
}