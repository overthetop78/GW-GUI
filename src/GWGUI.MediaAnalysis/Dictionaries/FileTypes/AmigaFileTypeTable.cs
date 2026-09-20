using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class AmigaFileTypeTable
{
    private const MediaFileSystemFamily Family = MediaFileSystemFamily.Amiga;
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(Family, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Nfo, MediaContentCategory.Text, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Readme, MediaContentCategory.Text, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Doc, MediaContentCategory.Document, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Guide, MediaContentCategory.Document, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Asm, MediaContentCategory.SourceCode, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.S, MediaContentCategory.SourceCode, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.C, MediaContentCategory.SourceCode, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.H, MediaContentCategory.SourceCode, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Bas, MediaContentCategory.BasicProgram, MediaTextEncoding.Unknown),
        Rule(Family, FileTypeExtensions.Ini, MediaContentCategory.Configuration, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Cfg, MediaContentCategory.Configuration, MediaTextEncoding.Latin1),
        Rule(Family, FileTypeExtensions.Info, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Iff, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Ilbm, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Lbm, MediaContentCategory.Image),
        Rule(Family, FileTypeExtensions.Mod, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Med, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Xm, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Ext8svx, MediaContentCategory.Audio),
        Rule(Family, FileTypeExtensions.Lha, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Lzh, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Zip, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Arc, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Zoo, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Dms, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Tar, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Gz, MediaContentCategory.Archive),
        Rule(Family, FileTypeExtensions.Library, MediaContentCategory.Library),
        Rule(Family, FileTypeExtensions.Device, MediaContentCategory.System),
        Rule(Family, FileTypeExtensions.Handler, MediaContentCategory.System),
        Rule(Family, FileTypeExtensions.Adf, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.Scp, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.Hfe, MediaContentCategory.DiskImage),
        Rule(Family, FileTypeExtensions.Ipf, MediaContentCategory.DiskImage)
    ];
}
