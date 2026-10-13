namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Constants;

internal static class SettingsConstants
{
    internal const string Cpu = "acorn_cpu";
    internal const string CpuFrequency = "acorn_cpu_frequency";
    internal const string MainMemory = "acorn_main_memory";
    internal const string Firmware = "acorn_firmware";
    internal const string Video = "acorn_video";
    internal const string AudioEnabled = "acorn_audio_enabled";
    internal const string AudioOutput = "gwgui_acorn_audio_output";
    internal const string AudioLatency = "gwgui_acorn_audio_latency";
    internal const string SharedFolder = "acorn_shared_folder";
    internal const string FloppyFolder = "acorn_floppy_folder";
    internal const string StateFolder = "acorn_state_folder";
    internal const string CaptureFolder = "acorn_capture_folder";
    internal const string DefaultOutput = "default";
    internal const string ResourceDiscController = "Emulation.Acorn.Storage.DiscController";
    internal const string ResourceVideoController = "Emulation.Acorn.Video.Controller";
    internal const string ResourceVideoPixels = "Emulation.Acorn.Video.Pixels";
    internal const string ResourceTeletext = "Emulation.Acorn.Video.Teletext";
    internal const string ResourceAutoBoot = "Emulation.Acorn.Option.AutoBoot";
    internal const string ResourceEmbeddedFirmware = "Emulation.Acorn.Firmware.Embedded";
    internal const string ResourceCpu = "Emulation.Cpu.Model";
    internal const string CpuResourcePrefix = "Emulation.Acorn.Cpu.";
    internal const string ResourceFrequency = "Emulation.Cpu.SpeedOriginal";
    internal const string ResourceMemory = "Emulation.Memory.Main";
    internal const string ResourceVideo = "Emulation.Video.Settings.Display";
    internal const string ResourceFirmware = "Emulation.Firmware.Rom.System";
    internal const string ResourceGeneral = "Emulation.Tab.General";
    internal const string ResourceAudio = "Emulation.Audio";
    internal const string ResourceAudioEnabled = "Emulation.Audio.Enabled";
    internal const string ResourceAudioOutput = "Emulation.Audio.Output";
    internal const string ResourceAudioLatency = "Emulation.Audio.Latency";
    internal const string ResourceAudioVolume = "Explorer.Volume";
    internal const string ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const int DefaultLatency = 50;
    internal const int DefaultVolume = 100;
    internal const int Kibibyte = 1024;
    internal const int Megahertz = 1_000_000;
    internal static readonly IReadOnlyList<int> Latencies = [20, 30, 40, 50, 75, 100, 150, 200];
    internal static readonly IReadOnlyList<int> Volumes = [0, 25, 50, 75, 100];
}
