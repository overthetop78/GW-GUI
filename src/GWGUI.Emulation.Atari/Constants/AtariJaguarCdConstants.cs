namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariJaguarCdConstants
{
    internal static readonly IReadOnlySet<string> CompleteDiscExtensions =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "cue", "cdi" };

    internal const string CueExtension = ".cue";
    internal const string CueFileDirective = "FILE ";
    internal const char CueQuotedPathDelimiter = '"';
    internal const int MissingCueDelimiterIndex = -1;
    internal const int CueContentStartOffset = 1;
    internal const bool RequiresFullPath = true;
}
