using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class UcsdFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Ucsd;

    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Text, MediaContentCategory.Text, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Foto, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Graf, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Code, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable)
    ];
}
