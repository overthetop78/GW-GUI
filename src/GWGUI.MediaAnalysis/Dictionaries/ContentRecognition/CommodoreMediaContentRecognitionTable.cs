using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class CommodoreMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Commodore;
    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.Petscii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Seq, [], MediaContentCategory.Text, MediaTextEncoding.Petscii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Prg, [], MediaContentCategory.Program, MediaTextEncoding.NotApplicable, MediaExecutionKind.InterpretedProgram, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Koa, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Art, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Iff, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sid, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Mus, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Arc, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sda, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None)
    ];
}
