using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Constants;

internal static class OptionConstants
{
    internal const string Bios = "o2em_bios";
    internal const string Region = "o2em_region";
    internal const string SwapGamepads = "o2em_swap_gamepads";
    internal const string VkbdTransparency = "o2em_vkbd_transparency";
    internal const string CropOverscan = "o2em_crop_overscan";
    internal const string MixFrames = "o2em_mix_frames";
    internal const string AudioVolume = "o2em_audio_volume";
    internal const string VoiceVolume = "o2em_voice_volume";
    internal const string LowPassFilter = "o2em_low_pass_filter";
    internal const string LowPassRange = "o2em_low_pass_range";
    internal const string O2romBin = "o2rom.bin";
    internal const string C52Bin = "c52.bin";
    internal const string G7400Bin = "g7400.bin";
    internal const string JopacBin = "jopac.bin";
    internal const string Auto = "auto";
    internal const string NTSC = "NTSC";
    internal const string PAL = "PAL";
    internal const string Disabled = "disabled";
    internal const string Enabled = "enabled";
    internal const string Mix = "mix";
    internal const string Ghost65 = "ghost_65";
    internal const string Ghost75 = "ghost_75";
    internal const string Ghost85 = "ghost_85";
    internal const string Ghost95 = "ghost_95";
    internal const int DefaultKeyboardTransparencyPercent = 0;
    internal const int DefaultAudioVolumePercent = 50;
    internal const int DefaultVoiceVolumePercent = 70;
    internal const int DefaultLowPassRangePercent = 60;

    internal static IReadOnlyList<EmulationSettingsNumericRange> VolumePercentRanges { get; } = [new(0, 100, 5, "%")];
    internal static IReadOnlyList<EmulationSettingsNumericRange> KeyboardTransparencyPercentRanges { get; } = [new(0, 75, 25, "%")];
    internal static IReadOnlyList<EmulationSettingsNumericRange> LowPassPercentRanges { get; } = [new(5, 95, 5, "%")];
}
