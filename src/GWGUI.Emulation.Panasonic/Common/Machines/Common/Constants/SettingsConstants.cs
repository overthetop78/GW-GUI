namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Constants;
internal static class SettingsConstants
{
    internal const string AudioEnabled = "panasonic_audio_enabled", AudioOutput = "gwgui_panasonic_audio_output", AudioLatency = "gwgui_panasonic_audio_latency";
    internal const string CpuModel = "panasonic_cpu_model", CpuFrequency = "panasonic_cpu_frequency", MainMemory = "panasonic_main_memory";
    internal const string VideoMemory = "panasonic_video_memory", ResourceMemoryVideo = "Emulation.Memory.Video";
    internal const string NonVolatileMemory = "panasonic_nonvolatile_memory", ResourceNonVolatileMemory = "Emulation.Memory.NonVolatile";
    internal const string DefaultOutput = "default";
    internal const string ResourceAudio = "Emulation.Audio", ResourceAudioEnabled = "Emulation.Audio.Enabled", ResourceAudioOutput = "Emulation.Audio.Output",
        ResourceAudioLatency = "Emulation.Audio.Latency", ResourceAudioVolume = "Explorer.Volume", ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor", ResourceCpuModel = "Emulation.Cpu.Model", ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal", ResourceMemoryMain = "Emulation.Memory.Main";
    internal const int BytesPerKibibyte = 1024;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const string MegahertzUnit = "MHz", KibibyteUnit = "KiB", MillisecondsUnit = "ms", PercentUnit = "%";
}