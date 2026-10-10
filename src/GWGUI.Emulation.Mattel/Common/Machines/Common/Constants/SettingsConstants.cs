namespace GWGUI.Emulation.Mattel.Common.Machines.Common.Constants;

internal static class SettingsConstants
{
    internal const string AudioEnabled = "mattel_audio_enabled";
    internal const string AudioOutput = "gwgui_mattel_audio_output";
    internal const string AudioLatency = "gwgui_mattel_audio_latency";
    internal const string DefaultOutput = "default";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor";
    internal const string ResourceCpuAcceleration = "Emulation.Cpu.Acceleration";
    internal const string ResourceCpuModel = "Emulation.Cpu.Model";
    internal const string ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal";
    internal const string ResourceMemoryMain = "Emulation.Memory.Main";
    internal const string ResourceDisplayMemory = "Emulation.Mattel.Memory.Display";
    internal const string ResourceVideoResolution = "Emulation.Video.Resolution";
    internal const string CpuModel = "mattel_cpu_model";
    internal const string CpuFrequency = "mattel_cpu_frequency";
    internal const string MainMemory = "mattel_main_memory";
    internal const string DisplayMemory = "mattel_display_memory";
    internal const string DisplayResolution = "mattel_display_resolution";
    internal const int BytesPerKibibyte = 1024;
    internal const double HertzPerMegahertz = 1_000_000;
    internal const double HertzPerKilohertz = 1_000;
    internal const string MegahertzUnit = "MHz", KilohertzUnit = "kHz", ByteUnit = "B", KibibyteUnit = "KiB";
    internal const string ResourceFirmware = "Emulation.Firmware.Rom.System";
    internal const string ResourceNativeAudio = "Emulation.Mattel.Audio.Native";
    internal const string ResourceNativeKeyboard = "Emulation.Mattel.Keyboard.Native";
    internal const string ResourceNativeVideo = "Emulation.Mattel.Video.Native";
    internal const string ResourceNativeControls = "Emulation.Mattel.Controls.Native";
    internal const string ResourceNativeSystem = "Emulation.Mattel.System.Native";
    internal const string NativeOptionResourcePrefix = "Emulation.Mattel.Option.";
    internal const string NativeChoiceResourceFormat = "{0}.Value{1}";
    internal const string HelpResourceSuffix = ".Help";
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
