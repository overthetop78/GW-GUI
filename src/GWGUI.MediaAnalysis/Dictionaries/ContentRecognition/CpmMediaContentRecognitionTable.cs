using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class CpmMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Cpm;
    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Doc, [], MediaContentCategory.Document, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Asm, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Mac, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.For, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.C, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.H, [], MediaContentCategory.SourceCode, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bas, [], MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.BasicListing),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lib, [], MediaContentCategory.Library, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Arc, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lbr, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Com, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Cmd, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sub, [], MediaContentCategory.Command, MediaTextEncoding.Ascii, MediaExecutionKind.CommandScript, MediaPreviewKind.Text)
    ];
}
