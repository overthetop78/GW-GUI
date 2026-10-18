namespace GWGUI.Emulation.Thomson.Common.Machines.Common.Constants;
internal static class SettingsConstants
{
    internal const string AudioEnabled = "thomson_audio_enabled", AudioOutput = "gwgui_thomson_audio_output", AudioLatency = "gwgui_thomson_audio_latency", MouseSpeed = "thomson_mouse_speed";
    internal const string CpuModel = "thomson_cpu_model", CpuFrequency = "thomson_cpu_frequency", MainMemory = "thomson_main_memory";
    internal const string DefaultOutput = "default";
    internal const string ResourceAudio = "Emulation.Audio", ResourceAudioEnabled = "Emulation.Audio.Enabled", ResourceAudioOutput = "Emulation.Audio.Output",
        ResourceAudioLatency = "Emulation.Audio.Latency", ResourceAudioVolume = "Explorer.Volume", ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    internal const string ResourceMouse = "Emulation.Tab.Mouse", ResourceMouseSpeed = "Emulation.Mouse.Speed";
    internal const string LightPenEnabled = "gwgui_thomson_light_pen_enabled";
    internal const string ResourceLightPen = "Emulation.Thomson.LightPen", ResourceLightPenContact = "Emulation.Thomson.LightPen.Contact";
    internal const string ResourceCpuProcessor = "Emulation.Cpu.Processor", ResourceCpuModel = "Emulation.Cpu.Model", ResourceCpuSpeedOriginal = "Emulation.Cpu.SpeedOriginal", ResourceMemoryMain = "Emulation.Memory.Main";
    internal const int BytesPerKibibyte = 1024, MouseSpeedStepPercent = 10;
    internal const string KibibyteUnit = "KiB", MillisecondsUnit = "ms", PercentUnit = "%";
}
