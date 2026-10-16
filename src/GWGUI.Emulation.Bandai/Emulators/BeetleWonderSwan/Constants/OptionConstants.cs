using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Bandai.Emulators.BeetleWonderSwan.Constants;

internal static class OptionConstants
{
    internal const string OptionLanguage = "wswan_language";
    internal const string OptionNameSource = "wswan_name_source";
    internal const string OptionBirthYear = "wswan_birth_year";
    internal const string OptionBirthMonth = "wswan_birth_month";
    internal const string OptionBirthDay = "wswan_birth_day";
    internal const string OptionSex = "wswan_sex";
    internal const string OptionBlood = "wswan_blood";
    internal const string OptionRotateDisplay = "wswan_rotate_display";
    internal const string OptionRotateKeymap = "wswan_rotate_keymap";
    internal const string OptionMonoPalette = "wswan_mono_palette";
    internal const string OptionGfxColors = "wswan_gfx_colors";
    internal const string OptionFrameskip = "wswan_frameskip";
    internal const string OptionFrameskipThreshold = "wswan_frameskip_threshold";
    internal const string Option60hzMode = "wswan_60hz_mode";
    internal const string OptionSoundSampleRate = "wswan_sound_sample_rate";
    internal const string OptionSoundLowPass = "wswan_sound_low_pass";

    internal const int DefaultBirthYear = 1989;
    internal const int DefaultBirthDay = 23;
    internal const int DefaultFrameSkipThresholdPercent = 33;
    internal const int DefaultAudioSampleRateHz = 44100;

    internal static EmulationSettingsNumericRange BirthYears { get; } = new(1920, 2029, 1);
    internal static EmulationSettingsNumericRange BirthDays { get; } = new(1, 31, 1);
    internal static EmulationSettingsNumericRange FrameSkipThresholds { get; } = new(15, 60, 3, " %");
    internal static IReadOnlyList<int> AudioSampleRatesHz { get; } = [11025, 22050, 44100, 48000];
}
