using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class AppleFileTypeTable
{
    private const ExplorerFileSystemFamily Dos = ExplorerFileSystemFamily.AppleDos;
    private const ExplorerFileSystemFamily ProDos = ExplorerFileSystemFamily.ProDos;
    private const ExplorerFileSystemFamily Macintosh = ExplorerFileSystemFamily.Macintosh;
    private const ExplorerFileSystemFamily Lisa = ExplorerFileSystemFamily.Lisa;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(Dos, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.AppleAscii),
        Rule(Dos, ".pic", ExplorerFileCategory.Image),
        Rule(Dos, ".d13", ExplorerFileCategory.DiskImage), Rule(Dos, ".dsk", ExplorerFileCategory.DiskImage),
        Rule(Dos, ".do", ExplorerFileCategory.DiskImage), Rule(Dos, ".po", ExplorerFileCategory.DiskImage),
        Rule(Dos, ".2mg", ExplorerFileCategory.DiskImage), Rule(Dos, ".nib", ExplorerFileCategory.DiskImage),
        Rule(Dos, ".woz", ExplorerFileCategory.DiskImage), Rule(Dos, ".scp", ExplorerFileCategory.DiskImage),

        Rule(ProDos, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.AppleAscii),
        Rule(ProDos, ".pic", ExplorerFileCategory.Image), Rule(ProDos, ".shk", ExplorerFileCategory.Archive),
        Rule(ProDos, ".sys", ExplorerFileCategory.System),
        Rule(ProDos, ".dsk", ExplorerFileCategory.DiskImage), Rule(ProDos, ".po", ExplorerFileCategory.DiskImage),
        Rule(ProDos, ".2mg", ExplorerFileCategory.DiskImage), Rule(ProDos, ".nib", ExplorerFileCategory.DiskImage),
        Rule(ProDos, ".woz", ExplorerFileCategory.DiskImage), Rule(ProDos, ".scp", ExplorerFileCategory.DiskImage),

        Rule(Macintosh, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.MacRoman),
        Rule(Macintosh, ".pict", ExplorerFileCategory.Image), Rule(Macintosh, ".pct", ExplorerFileCategory.Image),
        Rule(Macintosh, ".snd", ExplorerFileCategory.Audio), Rule(Macintosh, ".aiff", ExplorerFileCategory.Audio),
        Rule(Macintosh, ".aif", ExplorerFileCategory.Audio), Rule(Macintosh, ".sit", ExplorerFileCategory.Archive),
        Rule(Macintosh, ".cpt", ExplorerFileCategory.Archive), Rule(Macintosh, ".hqx", ExplorerFileCategory.Archive),
        Rule(Macintosh, ".bin", ExplorerFileCategory.Archive),
        Rule(Macintosh, ".app", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Macintosh, ".image", ExplorerFileCategory.DiskImage), Rule(Macintosh, ".dc42", ExplorerFileCategory.DiskImage),
        Rule(Macintosh, ".dsk", ExplorerFileCategory.DiskImage), Rule(Macintosh, ".img", ExplorerFileCategory.DiskImage),
        Rule(Macintosh, ".scp", ExplorerFileCategory.DiskImage),

        Rule(Lisa, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Unknown),
        Rule(Lisa, ".sit", ExplorerFileCategory.Archive), Rule(Lisa, ".image", ExplorerFileCategory.DiskImage),
        Rule(Lisa, ".dc42", ExplorerFileCategory.DiskImage), Rule(Lisa, ".img", ExplorerFileCategory.DiskImage),
        Rule(Lisa, ".scp", ExplorerFileCategory.DiskImage)
    ];
}
