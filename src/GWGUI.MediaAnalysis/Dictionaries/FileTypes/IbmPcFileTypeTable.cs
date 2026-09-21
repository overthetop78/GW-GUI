using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class IbmPcFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.IbmPc;
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.Nfo, MediaContentCategory.Text, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.Readme, MediaContentCategory.Text, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.Doc, MediaContentCategory.Document, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Asm, MediaContentCategory.SourceCode, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.C, MediaContentCategory.SourceCode, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.H, MediaContentCategory.SourceCode, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.Bas, MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Ini, MediaContentCategory.Configuration, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.Cfg, MediaContentCategory.Configuration, MediaTextEncoding.DosOem),
        Rule(Family, FileTypeExtensions.Xml, MediaContentCategory.Configuration, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Html, MediaContentCategory.Document, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Bmp, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Gif, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Jpg, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Jpeg, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Png, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Pcx, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Wav, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Voc, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Mid, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Midi, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.S3m, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Xm, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Zip, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Arc, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Arj, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Lha, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Lzh, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Zoo, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Tar, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Gz, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Exe, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Family, FileTypeExtensions.Com, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Family, FileTypeExtensions.Bat, MediaContentCategory.Command, MediaTextEncoding.DosOem, MediaExecutionKind.CommandScript),
        Rule(Family, FileTypeExtensions.Cmd, MediaContentCategory.Command, MediaTextEncoding.DosOem, MediaExecutionKind.CommandScript),
        Rule(Family, FileTypeExtensions.Sys, MediaContentCategory.System),
        Rule(Family, FileTypeExtensions.Dll, MediaContentCategory.Library),
        Rule(Family, FileTypeExtensions.Obj, MediaContentCategory.ObjectCode),
        Rule(Family, FileTypeExtensions.Lib, MediaContentCategory.Library)
    ];
}
