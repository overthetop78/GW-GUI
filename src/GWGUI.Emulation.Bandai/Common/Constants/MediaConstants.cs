namespace GWGUI.Emulation.Bandai.Common.Constants;

internal static class MediaConstants
{
    internal const int MaximumTamagotchiRomBytes = 12288;
    internal const int TamagotchiInstructionBytes = 2;
    internal const string TamagotchiRomMd5 = "3fce172403c27274e59723bbb9f6d8e9";
    internal const string BatterySaveExtension = ".sav";
    internal const string CueExtension = ".cue";
    internal const int MaximumBatterySaveBytes = 16 * 1024 * 1024;
    internal const uint BatteryMemoryId = 0;
    internal const char ExtensionPrefix = '.';
    internal const char SupportedExtensionSeparator = '|';
    internal const string ExtensionListSeparator = ", ";
}
