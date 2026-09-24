using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class AppleDosMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.AppleDos;

    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.AppleAscii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pic, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image)
    ];
}
