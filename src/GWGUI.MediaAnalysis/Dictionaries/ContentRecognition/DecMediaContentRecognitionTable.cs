using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class DecMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Dec;

    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Mac, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.For, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bas, [], MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.BasicListing),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Cmd, [], MediaContentCategory.Command, MediaTextEncoding.Ascii, MediaExecutionKind.CommandScript, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Com, [], MediaContentCategory.Command, MediaTextEncoding.Ascii, MediaExecutionKind.CommandScript, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bup, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sav, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lda, [], MediaContentCategory.ObjectCode, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Rel, [], MediaContentCategory.ObjectCode, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Obj, [], MediaContentCategory.ObjectCode, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sys, [], MediaContentCategory.System, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal)
    ];
}
