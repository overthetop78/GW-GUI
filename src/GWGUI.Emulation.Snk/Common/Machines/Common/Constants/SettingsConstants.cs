namespace GWGUI.Emulation.Snk.Common.Machines.Common.Constants;

internal static class SettingsConstants
{
    internal const string AudioEnabled = "snk_audio_enabled";
    internal const string InitialContent = "snk_initial_content";
    internal const string ResourceInitialContent = "Emulation.Snk.Content.Initial";
    internal const string AudioOutput = "gwgui_snk_audio_output";
    internal const string AudioLatency = "gwgui_snk_audio_latency";
    internal const string DefaultOutput = "default";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor";
    internal const string ResourceCpuAcceleration = "Emulation.Cpu.Acceleration";
    internal const string ResourceCpuModel = "Emulation.Cpu.Model";
    internal const string ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal";
    internal const string ResourceMemoryMain = "Emulation.Memory.Main";
    internal const string ResourceDisplayMemory = "Emulation.Snk.Memory.Display";
    internal const string ResourceVideoResolution = "Emulation.Video.Resolution";
    internal const string CpuModel = "snk_cpu_model";
    internal const string CpuFrequency = "snk_cpu_frequency";
    internal const string MainMemory = "snk_main_memory";
    internal const string DisplayMemory = "snk_display_memory";
    internal const string DisplayResolution = "snk_display_resolution";
    internal const int BytesPerKibibyte = 1024;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const double HertzPerKilohertz = 1_000;
    internal const string MegahertzUnit = "MHz", KilohertzUnit = "kHz", ByteUnit = "B", KibibyteUnit = "KiB";
    internal const string ResourceFirmware = "Emulation.Firmware.Rom.System";
    internal const string ResourceNativeAudio = "Emulation.Snk.Audio.Native";
    internal const string ResourceNativeVideo = "Emulation.Snk.Video.Native";
    internal const string ResourceNativeControls = "Emulation.Snk.Controls.Native";
    internal const string ResourceNativeSystem = "Emulation.Snk.System.Native";
    internal const string ResourceNativeStorage = "Emulation.Tab.Storage";
    internal const string ResourceEnabled = "Emulation.Value.Enabled";
    internal const string ResourceDisabled = "Emulation.Value.Disabled";
    internal const string ResourceDefault = "Emulation.Value.Default";
    internal const string ResourceAutomatic = "Emulation.Controller.Automatic";
    internal const string ResourceNone = "Emulation.Value.None";
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
