using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class DecFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Dec;

    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Mac, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.For, MediaContentCategory.SourceCode, MediaTextEncoding.Ascii),
        Rule(Family, FileTypeExtensions.Bas, MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Cmd, MediaContentCategory.Command, MediaTextEncoding.Ascii, MediaExecutionKind.CommandScript),
        Rule(Family, FileTypeExtensions.Com, MediaContentCategory.Command, MediaTextEncoding.Ascii, MediaExecutionKind.CommandScript),
        Rule(Family, FileTypeExtensions.Bup, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Sav, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Family, FileTypeExtensions.Lda, MediaContentCategory.ObjectCode),
        Rule(Family, FileTypeExtensions.Rel, MediaContentCategory.ObjectCode),
        Rule(Family, FileTypeExtensions.Obj, MediaContentCategory.ObjectCode),
        Rule(Family, FileTypeExtensions.Sys, MediaContentCategory.System)
    ];
}
