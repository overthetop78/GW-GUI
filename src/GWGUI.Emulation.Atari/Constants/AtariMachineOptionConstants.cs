namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariMachineOptionConstants
{
    internal const string MachineType = "hatari_machinetype";
    internal const string RamSize = "hatari_ramsize";
    internal const string CpuFrequency = "hatari_cpu_freq";
    internal const string HighResolution = "hatari_video_hires";
    internal const string RefreshRate = "hatari_forcerefresh";
    internal const string CropOverscan = "hatari_video_crop_overscan";
    internal const string FrameSkip = "hatari_frameskips";
    internal const string MouseSpeed = "hatari_emulated_mouse_speed";
    internal const string FastFloppy = "hatari_fastfdc";
    internal const string FloppyWriteProtection = "hatari_writeprotect_floppy";
    internal const string DisableMouse = "hatari_nomouse";
    internal const string StartInMouseMode = "hatari_start_in_mouse_mode";
    internal const string DisableKeyboard = "hatari_nokeys";
    internal const string TwoJoysticks = "hatari_twojoy";
    internal const string DriveActivity = "hatari_led_status_display";
    internal const string InputStatusDisplay = "hatari_joymousestatus_display";
    internal const string AutoloadConfiguration = "hatari_autoload_config";

    internal const string MainMemory = "gwgui_atari_main_memory";
    internal const string Frequency = "gwgui_atari_cpu_frequency";
    internal const string Crop = "gwgui_atari_video_crop";
    internal const string Frames = "gwgui_atari_video_frameskip";
    internal const string PointerSpeed = "gwgui_atari_mouse_speed";
    internal const string FloppySpeedPrefix = "storage.speed.";
    internal const string FloppyWriteProtectionPrefix = "storage.writeProtected.";
}
