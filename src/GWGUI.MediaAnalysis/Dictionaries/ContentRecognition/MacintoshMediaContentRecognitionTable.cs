using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class MacintoshMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Macintosh;

    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.MacRoman, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pict, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pct, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Snd, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Aiff, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Aif, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sit, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Cpt, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Hqx, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bin, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.App, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal)
    ];
}
