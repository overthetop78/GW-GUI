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

    private static readonly IReadOnlyDictionary<(MediaFileSystemFamily? Family, string Extension), MediaContentRecognitionRule> ExtensionIndex =
        Rows.Where(row => row.Extension.Length > 0 && !MediaContentSignatureMatcher.HasSignatures(row))
            .ToDictionary(row => (row.Family, row.Extension));

    public static MediaContentRecognitionRule? Find(MediaFileSystemFamily family, string? extension)
    {
        var normalized = MediaContentRecognitionFunctions.NormalizeExtension(extension);
        if (normalized.Length == 0) return null;
        if (ExtensionIndex.TryGetValue((family, normalized), out var exact)) return exact;
        var parent = family switch
        {
            MediaFileSystemFamily.Atari8Bit or MediaFileSystemFamily.AtariTos => MediaFileSystemFamily.Atari,
            _ => (MediaFileSystemFamily?)null
        };
        if (parent is not null && ExtensionIndex.TryGetValue((parent, normalized), out var inherited)) return inherited;
        return ExtensionIndex.TryGetValue((null, normalized), out var common) ? common : null;
    }

    public static MediaContentRecognitionRule? Find(
        MediaFileSystemFamily family,
        string? extension,
        IReadOnlyList<byte>? content)
    {
        var normalized = MediaContentRecognitionFunctions.NormalizeExtension(extension);
        foreach (var candidateFamily in CandidateFamilies(family))
        {
            var match = Rows.FirstOrDefault(row =>
                row.Family == candidateFamily
                && MediaContentSignatureMatcher.HasSignatures(row)
                && (row.Extension.Length == 0 || row.Extension == normalized)
                && MediaContentSignatureMatcher.Matches(row, content));
            if (match is not null) return match;
        }
        return Find(family, extension);
    }

    private static IEnumerable<MediaFileSystemFamily?> CandidateFamilies(MediaFileSystemFamily family)
    {
        yield return family;
        if (family is MediaFileSystemFamily.Atari8Bit or MediaFileSystemFamily.AtariTos)
            yield return MediaFileSystemFamily.Atari;
        yield return null;
    }
}
