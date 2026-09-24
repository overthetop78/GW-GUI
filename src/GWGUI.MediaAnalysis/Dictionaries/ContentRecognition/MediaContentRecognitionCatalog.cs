using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

public static class MediaContentRecognitionCatalog
{
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        .. CommonMediaContentRecognitionTable.Rows,
        .. AmigaMediaContentRecognitionTable.Rows,
        .. IbmPcMediaContentRecognitionTable.Rows,
        .. AtariCommonMediaContentRecognitionTable.Rows,
        .. AtariTosMediaContentRecognitionTable.Rows,
        .. Atari8BitMediaContentRecognitionTable.Rows,
        .. AppleDosMediaContentRecognitionTable.Rows,
        .. ProDosMediaContentRecognitionTable.Rows,
        .. MacintoshMediaContentRecognitionTable.Rows,
        .. LisaMediaContentRecognitionTable.Rows,
        .. CommodoreMediaContentRecognitionTable.Rows,
        .. CpmMediaContentRecognitionTable.Rows,
        .. BbcMicroMediaContentRecognitionTable.Rows,
        .. DecMediaContentRecognitionTable.Rows,
        .. MsxMediaContentRecognitionTable.Rows,
        .. UcsdMediaContentRecognitionTable.Rows
    ];

    public static MediaContentRecognitionRule? Find(MediaFileSystemFamily family, string? extension)
        => Find(family, extension, null, MediaContentRecognitionPriority.Standard);

    public static MediaContentRecognitionRule? Find(
        MediaFileSystemFamily family,
        string? extension,
        IReadOnlyList<byte>? content,
        MediaContentRecognitionPriority priority)
    {
        var normalized = MediaContentRecognitionFunctions.NormalizeExtension(extension);
        foreach (var candidateFamily in CandidateFamilies(family))
        {
            var match = Rows.FirstOrDefault(row =>
                row.Family == candidateFamily
                && row.Priority == priority
                && (row.Extension.Length == 0 || row.Extension == normalized)
                && (MediaContentRecognitionRuleMatcher.HasContentConditions(row)
                    ? MediaContentRecognitionRuleMatcher.Matches(row, content)
                    : row.Extension.Length > 0));
            if (match is not null) return match;
        }
        return null;
    }

    private static IEnumerable<MediaFileSystemFamily?> CandidateFamilies(MediaFileSystemFamily family)
    {
        yield return family;
        if (family is MediaFileSystemFamily.Atari8Bit or MediaFileSystemFamily.AtariTos)
            yield return MediaFileSystemFamily.Atari;
        yield return null;
    }
}
