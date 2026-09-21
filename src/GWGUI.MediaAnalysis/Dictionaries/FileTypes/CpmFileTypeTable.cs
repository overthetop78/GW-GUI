using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class CpmFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Cpm;
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Doc, MediaContentCategory.Document, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Asm, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Mac, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.For, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.C, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.H, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Bas, MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Lib, MediaContentCategory.Library),
        Rule(Family, FileTypeExtensions.Arc, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Lbr, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Com, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Family, FileTypeExtensions.Cmd, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Family, FileTypeExtensions.Sub, MediaContentCategory.Command, MediaTextEncoding.Ascii, MediaExecutionKind.CommandScript)
    ];
}
