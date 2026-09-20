using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class CommodoreFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Commodore;
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Petscii),
        Rule(Family, FileTypeExtensions.Seq, MediaContentCategory.Text, MediaTextEncoding.Petscii),
        Rule(Family, FileTypeExtensions.Prg, MediaContentCategory.Program, execution: MediaExecutionKind.InterpretedProgram),
        Rule(Family, FileTypeExtensions.Koa, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Art, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Iff, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Sid, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Mus, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Arc, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Sda, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.D64, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.D71, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.D81, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.G64, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.Scp, MediaContentCategory.DiskImage)
    ];
}
