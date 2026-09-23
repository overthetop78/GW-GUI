using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaAnalysis.Dictionaries.ContentRecognition;

internal static class IbmPcMediaContentRecognitionTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.IbmPc;
    // Famille | Extension | Signatures | Catégorie | Encodage | Exécution | Aperçu
    public static IReadOnlyList<MediaContentRecognitionRule> Rows { get; } =
    [
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Nfo, [], MediaContentCategory.Text, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Readme, [], MediaContentCategory.Text, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Doc, [], MediaContentCategory.Document, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Asm, [], MediaContentCategory.SourceCode, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.C, [], MediaContentCategory.SourceCode, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.H, [], MediaContentCategory.SourceCode, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bas, [], MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.BasicListing),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Ini, [], MediaContentCategory.Configuration, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Cfg, [], MediaContentCategory.Configuration, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Xml, [], MediaContentCategory.Configuration, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Html, [], MediaContentCategory.Document, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bmp, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Gif, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Jpg, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Jpeg, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Png, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Pcx, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Wav, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Voc, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Mid, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Midi, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.S3m, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Xm, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Zip, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Arc, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Arj, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lha, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lzh, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Zoo, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Tar, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Gz, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Exe, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Com, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Bat, [], MediaContentCategory.Command, MediaTextEncoding.DosOem, MediaExecutionKind.CommandScript, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Cmd, [], MediaContentCategory.Command, MediaTextEncoding.DosOem, MediaExecutionKind.CommandScript, MediaPreviewKind.Text),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Sys, [], MediaContentCategory.System, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Dll, [], MediaContentCategory.Library, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Obj, [], MediaContentCategory.ObjectCode, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, MediaContentRecognitionPriority.Standard, FileTypeExtensions.Lib, [], MediaContentCategory.Library, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),

        new(Family, MediaContentRecognitionPriority.Primary, string.Empty, [MediaContentSignatures.DosMzExecutable], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal)
    ];
}
