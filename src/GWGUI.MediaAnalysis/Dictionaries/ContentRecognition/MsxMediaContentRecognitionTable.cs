using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class MsxMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Msx;

    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.Msx, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Doc, [], MediaContentCategory.Document, MediaTextEncoding.Msx, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Asc, [], MediaContentCategory.Text, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bas, [], MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.BasicListing),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sc2, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sc5, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sc7, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sc8, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Mgs, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bgm, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lzh, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pma, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Com, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Rom, [], MediaContentCategory.Rom, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal)
    ];
}
