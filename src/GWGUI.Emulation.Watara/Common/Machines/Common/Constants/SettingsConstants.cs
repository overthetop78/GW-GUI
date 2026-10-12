namespace GWGUI.Emulation.Watara.Common.Machines.Common.Constants;
internal static class SettingsConstants
{
    internal const string AudioEnabled = "watara_audio_enabled", AudioOutput = "gwgui_watara_audio_output", AudioLatency = "gwgui_watara_audio_latency";
    internal const string CpuModel = "watara_cpu_model", CpuFrequency = "watara_cpu_frequency", MainMemory = "watara_main_memory";
    internal const string VideoMemory = "watara_video_memory", ResourceMemoryVideo = "Emulation.Watara.Memory.Video";
    internal const string DefaultOutput = "default";
    internal const string ResourceAudio = "Emulation.Audio", ResourceAudioEnabled = "Emulation.Audio.Enabled", ResourceAudioOutput = "Emulation.Audio.Output", ResourceAudioLatency = "Emulation.Audio.Latency", ResourceAudioVolume = "Explorer.Volume", ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor", ResourceCpuModel = "Emulation.Cpu.Model", ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal", ResourceMemoryMain = "Emulation.Memory.Main";
    internal const int BytesPerKibibyte = 1024, DefaultLatency = 50, DefaultVolume = 100;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const string MegahertzUnit = "MHz", KibibyteUnit = "KiB", MillisecondsUnit = "ms", PercentUnit = "%";
    internal static readonly IReadOnlyList<int> Latencies = [20, 30, 40, 50, 75, 100, 150, 200];
    internal static readonly IReadOnlyList<int> Volumes = [0, 25, 50, 75, 100];
}