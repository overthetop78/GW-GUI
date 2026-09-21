using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class AmigaMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Amiga;
    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Nfo, [], MediaContentCategory.Text, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Readme, [], MediaContentCategory.Text, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Doc, [], MediaContentCategory.Document, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Guide, [], MediaContentCategory.Document, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Asm, [], MediaContentCategory.SourceCode, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.S, [], MediaContentCategory.SourceCode, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.C, [], MediaContentCategory.SourceCode, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.H, [], MediaContentCategory.SourceCode, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Bas, [], MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.BasicListing),
        new(Family, FileTypeExtensions.Ini, [], MediaContentCategory.Configuration, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Cfg, [], MediaContentCategory.Configuration, MediaTextEncoding.Latin1, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Info, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Iff, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Ilbm, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Lbm, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Mod, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Med, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Xm, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Ext8svx, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Lha, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Lzh, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Zip, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Arc, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Zoo, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Dms, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Tar, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Library, [], MediaContentCategory.Library, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Device, [], MediaContentCategory.System, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Handler, [], MediaContentCategory.System, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),

        new(Family, string.Empty, [MediaContentSignatures.AmigaHunkExecutable], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal)
    ];
}
