using GWGUI.MediaAnalysis.Constants;
using GWGUI.MediaAnalysis.Contracts;
using GWGUI.MediaAnalysis.Enums;
using GWGUI.MediaAnalysis.Functions;
using static GWGUI.MediaAnalysis.Functions.MediaContentTypeRuleFactory;

namespace GWGUI.MediaAnalysis.Dictionaries.FileTypes;

internal static class CommonFileTypeTable
{
    public static IReadOnlyList<MediaContentTypeDefinition> Rows { get; } =
    [
        Rule(null, FileTypeExtensions.Txt, MediaContentCategory.Text),
        Rule(null, FileTypeExtensions.Nfo, MediaContentCategory.Text),
        Rule(null, FileTypeExtensions.Readme, MediaContentCategory.Text),
        Rule(null, FileTypeExtensions.Doc, MediaContentCategory.Document),
        Rule(null, FileTypeExtensions.Asm, MediaContentCategory.SourceCode),
        Rule(null, FileTypeExtensions.S, MediaContentCategory.SourceCode),
        Rule(null, FileTypeExtensions.C, MediaContentCategory.SourceCode),
        Rule(null, FileTypeExtensions.H, MediaContentCategory.SourceCode),
        Rule(null, FileTypeExtensions.For, MediaContentCategory.SourceCode),
        Rule(null, FileTypeExtensions.Bas, MediaContentCategory.BasicProgram),
        Rule(null, FileTypeExtensions.Ini, MediaContentCategory.Configuration),
        Rule(null, FileTypeExtensions.Cfg, MediaContentCategory.Configuration),
        Rule(null, FileTypeExtensions.Xml, MediaContentCategory.Configuration),
        Rule(null, FileTypeExtensions.Json, MediaContentCategory.Configuration),
        Rule(null, FileTypeExtensions.Html, MediaContentCategory.Document),
        Rule(null, FileTypeExtensions.Htm, MediaContentCategory.Document),
        Rule(null, FileTypeExtensions.Dat, MediaContentCategory.Data),
        Rule(null, FileTypeExtensions.Obj, MediaContentCategory.ObjectCode),
        Rule(null, FileTypeExtensions.O, MediaContentCategory.ObjectCode),
        Rule(null, FileTypeExtensions.Sys, MediaContentCategory.System),
        Rule(null, FileTypeExtensions.Rom, MediaContentCategory.System),
        Rule(null, FileTypeExtensions.Lib, MediaContentCategory.Library),
        Rule(null, FileTypeExtensions.Library, MediaContentCategory.Library),
        Rule(null, FileTypeExtensions.Dll, MediaContentCategory.Library),
        Rule(null, FileTypeExtensions.Fon, MediaContentCategory.Font),
        Rule(null, FileTypeExtensions.Fnt, MediaContentCategory.Font),
        Rule(null, FileTypeExtensions.Ttf, MediaContentCategory.Font),
        Rule(null, FileTypeExtensions.Bmp, MediaContentCategory.Image),
        Rule(null, FileTypeExtensions.Gif, MediaContentCategory.Image),
        Rule(null, FileTypeExtensions.Jpg, MediaContentCategory.Image),
        Rule(null, FileTypeExtensions.Jpeg, MediaContentCategory.Image),
        Rule(null, FileTypeExtensions.Png, MediaContentCategory.Image),
        Rule(null, FileTypeExtensions.Pcx, MediaContentCategory.Image),
        Rule(null, FileTypeExtensions.Wav, MediaContentCategory.Audio),
        Rule(null, FileTypeExtensions.Mid, MediaContentCategory.Audio),
        Rule(null, FileTypeExtensions.Midi, MediaContentCategory.Audio),
        Rule(null, FileTypeExtensions.Mod, MediaContentCategory.Audio),
        Rule(null, FileTypeExtensions.Xm, MediaContentCategory.Audio),
        Rule(null, FileTypeExtensions.Zip, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Arc, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Lha, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Lzh, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Zoo, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Tar, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Gz, MediaContentCategory.Archive),
        Rule(null, FileTypeExtensions.Scp, MediaContentCategory.DiskImage),
        Rule(null, FileTypeExtensions.Hfe, MediaContentCategory.DiskImage),
        Rule(null, FileTypeExtensions.Img, MediaContentCategory.DiskImage),
        Rule(null, FileTypeExtensions.Ima, MediaContentCategory.DiskImage),
        Rule(null, FileTypeExtensions.Dsk, MediaContentCategory.DiskImage),
        Rule(null, FileTypeExtensions.Iso, MediaContentCategory.DiskImage)
    ];
}
