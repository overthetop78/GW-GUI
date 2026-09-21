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
        new(Family, FileTypeExtensions.Txt, [], MediaContentCategory.Text, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Nfo, [], MediaContentCategory.Text, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Readme, [], MediaContentCategory.Text, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Doc, [], MediaContentCategory.Document, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Asm, [], MediaContentCategory.SourceCode, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.C, [], MediaContentCategory.SourceCode, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.H, [], MediaContentCategory.SourceCode, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Bas, [], MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.BasicListing),
        new(Family, FileTypeExtensions.Ini, [], MediaContentCategory.Configuration, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Cfg, [], MediaContentCategory.Configuration, MediaTextEncoding.DosOem, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Xml, [], MediaContentCategory.Configuration, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Html, [], MediaContentCategory.Document, MediaTextEncoding.Unknown, MediaExecutionKind.None, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Bmp, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Gif, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Jpg, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Jpeg, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Png, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Pcx, [], MediaContentCategory.Image, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Image),
        new(Family, FileTypeExtensions.Wav, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Voc, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Mid, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Midi, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.S3m, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Xm, [], MediaContentCategory.Audio, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Audio),
        new(Family, FileTypeExtensions.Zip, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Arc, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Arj, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Lha, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Lzh, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Zoo, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Tar, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Gz, [], MediaContentCategory.Archive, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.None),
        new(Family, FileTypeExtensions.Exe, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Com, [], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Bat, [], MediaContentCategory.Command, MediaTextEncoding.DosOem, MediaExecutionKind.CommandScript, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Cmd, [], MediaContentCategory.Command, MediaTextEncoding.DosOem, MediaExecutionKind.CommandScript, MediaPreviewKind.Text),
        new(Family, FileTypeExtensions.Sys, [], MediaContentCategory.System, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Dll, [], MediaContentCategory.Library, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Obj, [], MediaContentCategory.ObjectCode, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),
        new(Family, FileTypeExtensions.Lib, [], MediaContentCategory.Library, MediaTextEncoding.NotApplicable, MediaExecutionKind.None, MediaPreviewKind.Hexadecimal),

        new(Family, string.Empty, [MediaContentSignatures.DosMzExecutable], MediaContentCategory.Executable, MediaTextEncoding.NotApplicable, MediaExecutionKind.NativeExecutable, MediaPreviewKind.Hexadecimal)
    ];
}
