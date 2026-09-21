using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class ProDosMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.ProDos;

    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.AppleAscii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Pic, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Shk, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Sys, [], MediaContentCategory.System, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal)
    ];
}
