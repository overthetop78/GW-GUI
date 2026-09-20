using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class MsxFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Msx;

    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Msx),
        Rule(Family, FileTypeExtensions.Doc, MediaContentCategory.Document, MediaTextEncoding.Msx),
        Rule(Family, FileTypeExtensions.Asc, MediaContentCategory.Text, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Bas, MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Sc2, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Sc5, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Sc7, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Sc8, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Mgs, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Bgm, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Lzh, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Pma, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Com, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Family, FileTypeExtensions.Rom, MediaContentCategory.System),
        Rule(Family, FileTypeExtensions.Dsk, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.Scp, MediaContentCategory.DiskImage)
    ];
}
