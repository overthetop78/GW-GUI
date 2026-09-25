namespace GWGUI.Emulation.Atari.Common.Constants;

internal static class MachineConstants
{
    internal const double MinimumFramesPerSecond = 1;
    internal const double MaximumFramesPerSecond = 1000;
    internal const int PauseWaitMilliseconds = 100;
    internal const int DiagnosticTailCount = 100;
    internal const int EmptyCount = 0;
    internal const long NoRemainingTicks = 0;
    internal const int InvalidSampleRate = 0;
    internal const string ThreadNamePrefix = "gwgui Atari";
    internal const string DiagnosticDataKey = "AtariDiagnostics";
    internal const string InvalidStartStateMessage = "The Atari machine can only be started once.";
    internal const string InvalidStateMessage = "The Atari machine must be running or paused.";
    internal const string StoppedMessage = "The Atari machine stopped.";
}

internal static class MachineConfigurationConstants
{
    internal const string Atari = "atari";
}

internal static class MachineOptionConstants
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

internal static class MachineOptionFunctionsConstants
{
    internal const string False = "false";
    internal const string True = "true";
    internal const string Value1 = "1";
    internal const string Value0 = "0";
    internal const string Monochrome = "Monochrome";
    internal const string Enabled = "enabled";
    internal const string St = "st";
    internal const string Ste = "ste";
    internal const string Tt = "tt";
    internal const string Falcon = "falcon";
    internal const string Value2 = "2";
    internal const string Value4 = "4";
    internal const string Value8 = "8";
    internal const string Value14 = "14";
    internal const string Value16 = "16";
    internal const string NTSC = "NTSC";
    internal const string PAL = "PAL";
    internal const string Auto = "auto";
    internal const string Value3 = "3";
    internal const string Value5 = "5";
    internal const string Value6 = "6";
    internal const string Value100 = "100";
    internal const string On = "on";
    internal const string Off = "off";
}

internal static class MachineValues
{
    internal const string Value0 = "0";
    internal const string Value1 = "1";
    internal const string HatariResetType = "hatari_reset_type";
}

internal static class HardwareSettingsConstants
{
    internal const string CompatibleResource = "Emulation.Cpu.Compatibility.Compatible";
    internal const string CycleExactResource = "Emulation.Cpu.Compatibility.Exact";
    internal const string NoneResource = "Emulation.Memory.None";
    internal const string MultilingualResource = "Emulation.Atari.Firmware.Multilingual";
    internal const string RegionFreeResource = "Emulation.Atari.Video.RegionFree";

    internal const string FrequencyMhzSuffix = " MHz";
    internal const string KibibyteSuffix = " KiB";
    internal const string MebibyteSuffix = " MiB";
    internal const string ByteSuffix = " B";

    internal const int BytesPerKibibyte = 1024;
    internal const int BytesPerMebibyte = 1024 * 1024;
}

internal static class HardwareSettingsFunctionsConstants
{
    internal const string EnUS = "en-US";
    internal const string DeDE = "de-DE";
    internal const string FrFR = "fr-FR";
    internal const string EnGB = "en-GB";
    internal const string EsES = "es-ES";
    internal const string ItIT = "it-IT";
    internal const string SvSE = "sv-SE";
    internal const string DeCH = "de-CH";
    internal const string FiFI = "fi-FI";
    internal const string NbNO = "nb-NO";
    internal const string CsCZ = "cs-CZ";
    internal const string RuRU = "ru-RU";
    internal const string ElGR = "el-GR";
}

internal static class EngineConstants
{
    internal const string IdentifierFormat = "N";
}
