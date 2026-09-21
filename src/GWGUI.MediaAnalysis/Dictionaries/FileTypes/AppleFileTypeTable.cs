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

        Rule(ProDos, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.AppleAscii),
        Rule(ProDos, FileTypeExtensions.Pic, MediaContentCategory.Image),
        Rule(ProDos, FileTypeExtensions.Shk, MediaContentCategory.Archive),
        Rule(ProDos, FileTypeExtensions.Sys, MediaContentCategory.System),

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

        Rule(Lisa, FileTypeExtensions.Txt, MediaContentCategory.Text, MediaTextEncoding.Unknown),
        Rule(Lisa, FileTypeExtensions.Sit, MediaContentCategory.Archive)
    ];
}
