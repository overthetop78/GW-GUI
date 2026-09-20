using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class AppleFileTypeTable
{
    private const MediaFileSystemFamily Dos = MediaFileSystemFamily.AppleDos;
    private const MediaFileSystemFamily ProDos = MediaFileSystemFamily.ProDos;
    private const MediaFileSystemFamily Macintosh = MediaFileSystemFamily.Macintosh;
    private const MediaFileSystemFamily Lisa = MediaFileSystemFamily.Lisa;
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Dos, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.AppleAscii),
        Rule(Dos, FileTypeExtensions.Pic, MediaContentCategory.Image),
        Rule(Dos, FileTypeExtensions.D13, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Dsk, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Do, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Po, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Ext2mg, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Nib, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Woz, MediaContentCategory.DiskImage),
        Rule(Dos, FileTypeExtensions.Scp, MediaContentCategory.DiskImage),

        Rule(ProDos, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.AppleAscii),
        Rule(ProDos, FileTypeExtensions.Pic, MediaContentCategory.Image),
        Rule(ProDos, FileTypeExtensions.Shk, MediaContentCategory.Archive),
        Rule(ProDos, FileTypeExtensions.Sys, MediaContentCategory.System),
        Rule(ProDos, FileTypeExtensions.Dsk, MediaContentCategory.DiskImage),
        Rule(ProDos, FileTypeExtensions.Po, MediaContentCategory.DiskImage),
        Rule(ProDos, FileTypeExtensions.Ext2mg, MediaContentCategory.DiskImage),
        Rule(ProDos, FileTypeExtensions.Nib, MediaContentCategory.DiskImage),
        Rule(ProDos, FileTypeExtensions.Woz, MediaContentCategory.DiskImage),
        Rule(ProDos, FileTypeExtensions.Scp, MediaContentCategory.DiskImage),

        Rule(Macintosh, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.MacRoman),
        Rule(Macintosh, FileTypeExtensions.Pict, MediaContentCategory.Image),
        Rule(Macintosh, FileTypeExtensions.Pct, MediaContentCategory.Image),
        Rule(Macintosh, FileTypeExtensions.Snd, MediaContentCategory.Audio),
        Rule(Macintosh, FileTypeExtensions.Aiff, MediaContentCategory.Audio),
        Rule(Macintosh, FileTypeExtensions.Aif, MediaContentCategory.Audio),
        Rule(Macintosh, FileTypeExtensions.Sit, MediaContentCategory.Archive),
        Rule(Macintosh, FileTypeExtensions.Cpt, MediaContentCategory.Archive),
        Rule(Macintosh, FileTypeExtensions.Hqx, MediaContentCategory.Archive),
        Rule(Macintosh, FileTypeExtensions.Bin, MediaContentCategory.Archive),
        Rule(Macintosh, FileTypeExtensions.App, MediaContentCategory.Executable, execution: MediaExecutionKind.NativeExecutable),
        Rule(Macintosh, FileTypeExtensions.Image, MediaContentCategory.DiskImage),
        Rule(Macintosh, FileTypeExtensions.Dc42, MediaContentCategory.DiskImage),
        Rule(Macintosh, FileTypeExtensions.Dsk, MediaContentCategory.DiskImage),
        Rule(Macintosh, FileTypeExtensions.Img, MediaContentCategory.DiskImage),
        Rule(Macintosh, FileTypeExtensions.Scp, MediaContentCategory.DiskImage),

        Rule(Lisa, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Unknown),
        Rule(Lisa, FileTypeExtensions.Sit, MediaContentCategory.Archive),
        Rule(Lisa, FileTypeExtensions.Image, MediaContentCategory.DiskImage),
        Rule(Lisa, FileTypeExtensions.Dc42, MediaContentCategory.DiskImage),
        Rule(Lisa, FileTypeExtensions.Img, MediaContentCategory.DiskImage),
        Rule(Lisa, FileTypeExtensions.Scp, MediaContentCategory.DiskImage)
    ];
}
