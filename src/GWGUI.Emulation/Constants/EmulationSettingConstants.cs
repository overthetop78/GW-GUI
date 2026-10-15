namespace GWGUI.Emulation.Constants;

public static class EmulationSettingConstants
{
    public const string Enabled = "enabled";
    public const string Disabled = "disabled";

    public const int DefaultAudioLatencyMilliseconds = 50;
    public const int DefaultAudioVolumePercent = 100;

    public static IReadOnlyList<int> AudioLatencyMillisecondsChoices { get; } = Array.AsReadOnly<int>([20, 30, 40, 50, 75, 100, 150, 200]);
    public static IReadOnlyList<int> AudioVolumePercentChoices { get; } = Array.AsReadOnly<int>([0, 25, 50, 75, 100]);
}
