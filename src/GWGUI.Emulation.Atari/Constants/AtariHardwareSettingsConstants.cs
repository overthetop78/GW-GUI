namespace GWGUI.Emulation.Atari;

internal static class AtariHardwareSettingsConstants
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
