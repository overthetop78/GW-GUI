using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class BbcMicroFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.BbcMicro;

    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Zip, MediaContentCategory.Archive)
    ];
}
