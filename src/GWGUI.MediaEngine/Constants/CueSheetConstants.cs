namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant CUE sheet tokens and CD time units understood by MediaEngine.</summary>
public static class CueSheetConstants
{
    public const int FramesPerSecond = 75;
    public const int SecondsPerMinute = 60;
    public const string Indent = "  ";
    public static readonly System.Text.Encoding Utf8WithoutBom = new System.Text.UTF8Encoding(false, true);
    public const string File = "FILE";
    public const string Track = "TRACK";
    public const string Index = "INDEX";
    public const string Pregap = "PREGAP";
    public const string Postgap = "POSTGAP";
    public const string Flags = "FLAGS";
    public const string Catalog = "CATALOG";
    public const string Isrc = "ISRC";
    public const string Title = "TITLE";
    public const string Performer = "PERFORMER";
    public const string Songwriter = "SONGWRITER";
    public const string Rem = "REM";
    public const string BinaryFile = "BINARY";
    public const string WaveFile = "WAVE";
    public const string AudioTrack = "AUDIO";
    public const string Mode1Data2048 = "MODE1/2048";
    public const string Mode1Raw2352 = "MODE1/2352";
    public const string Mode2Data2336 = "MODE2/2336";
    public const string Mode2Raw2352 = "MODE2/2352";
}
