namespace GWGUI.Emulation.Sinclair.Common.Machines.Common.Constants;

internal static class SettingsConstants
{
    internal const string AudioEnabled = "sinclair_audio_enabled";
    internal const string MouseSpeed = "sinclair_mouse_speed";
    internal const string InitialContent = "sinclair_initial_content";
    internal const string ResourceInitialContent = "Emulation.Sinclair.Content.Initial";
    internal const string ResourceMouse = "Emulation.Tab.Mouse";
    internal const string ResourceMouseSpeed = "Emulation.Mouse.Speed";
    internal const string AudioOutput = "gwgui_sinclair_audio_output";
    internal const string AudioLatency = "gwgui_sinclair_audio_latency";
    internal const string DefaultOutput = "default";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor";
    internal const string ResourceCpuAcceleration = "Emulation.Cpu.Acceleration";
    internal const string ResourceCpuModel = "Emulation.Cpu.Model";
    internal const string ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal";
    internal const string ResourceMemoryMain = "Emulation.Memory.Main";
    internal const string ResourceDisplayMemory = "Emulation.Sinclair.Memory.Display";
    internal const string ResourceVideoResolution = "Emulation.Video.Resolution";
    internal const string CpuModel = "sinclair_cpu_model";
    internal const string CpuFrequency = "sinclair_cpu_frequency";
    internal const string MainMemory = "sinclair_main_memory";
    internal const string DisplayMemory = "sinclair_display_memory";
    internal const string DisplayResolution = "sinclair_display_resolution";
    internal const int BytesPerKibibyte = 1024;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const double HertzPerKilohertz = 1_000;
    internal const string MegahertzUnit = "MHz", KilohertzUnit = "kHz", ByteUnit = "B", KibibyteUnit = "KiB";
    internal const string ResourceFirmware = "Emulation.Firmware.Rom.System";
    internal const string ResourceNativeAudio = "Emulation.Sinclair.Audio.Native";
    internal const string ResourceNativeKeyboard = "Emulation.Sinclair.Keyboard.Native";
    internal const string ResourceNativeVideo = "Emulation.Sinclair.Video.Native";
    internal const string ResourceNativeStorage = "Emulation.Storage.Cassettes";
    internal const string ResourceAudio = "Emulation.Audio";
    internal const string ResourceAudioEnabled = "Emulation.Audio.Enabled";
    internal const string ResourceAudioOutput = "Emulation.Audio.Output";
    internal const string ResourceAudioLatency = "Emulation.Audio.Latency";
    internal const string ResourceAudioVolume = "Explorer.Volume";
    internal const string ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const string MillisecondsUnit = "ms", PercentUnit = "%";
    internal const int DefaultLatency = 50, DefaultVolume = 100;
    internal static readonly IReadOnlyList<int> Latencies = [20, 30, 40, 50, 75, 100, 150, 200];
    internal static readonly IReadOnlyList<int> Volumes = [0, 25, 50, 75, 100];
}
