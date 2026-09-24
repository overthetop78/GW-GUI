using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class AtariTosMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.AtariTos;

    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Doc, [], MediaContentCategory.Document, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Asc, [], MediaContentCategory.Text, MediaTextEncoding.Ascii, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Inf, [], MediaContentCategory.Configuration, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Cfg, [], MediaContentCategory.Configuration, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Asm, [], MediaContentCategory.SourceCode, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.S, [], MediaContentCategory.SourceCode, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.C, [], MediaContentCategory.SourceCode, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.H, [], MediaContentCategory.SourceCode, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Neo, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pi1, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pi2, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pi3, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pc1, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pc2, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pc3, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Deg, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Iff, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Mod, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Snd, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Ym, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Zip, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Arc, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lzh, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lha, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Zoo, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Prg, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Ttp, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Tos, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.App, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Gtp, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Acc, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Primary, string.Empty, [MediaContentSignatures.AtariTosExecutable], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal)
    ];
}
