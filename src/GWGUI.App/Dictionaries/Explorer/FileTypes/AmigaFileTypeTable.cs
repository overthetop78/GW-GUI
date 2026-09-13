using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class AmigaFileTypeTable
{
    private const ExplorerFileSystemFamily Family = ExplorerFileSystemFamily.Amiga;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(Family, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Latin1),
        Rule(Family, ".nfo", ExplorerFileCategory.Text, ExplorerTextEncoding.Latin1),
        Rule(Family, ".readme", ExplorerFileCategory.Text, ExplorerTextEncoding.Latin1),
        Rule(Family, ".doc", ExplorerFileCategory.Document, ExplorerTextEncoding.Latin1),
        Rule(Family, ".guide", ExplorerFileCategory.Document, ExplorerTextEncoding.Latin1),
        Rule(Family, ".asm", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Latin1),
        Rule(Family, ".s", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Latin1),
        Rule(Family, ".c", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Latin1),
        Rule(Family, ".h", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Latin1),
        Rule(Family, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(Family, ".ini", ExplorerFileCategory.Configuration, ExplorerTextEncoding.Latin1),
        Rule(Family, ".cfg", ExplorerFileCategory.Configuration, ExplorerTextEncoding.Latin1),
        Rule(Family, ".info", ExplorerFileCategory.Image),
        Rule(Family, ".iff", ExplorerFileCategory.Image),
        Rule(Family, ".ilbm", ExplorerFileCategory.Image),
        Rule(Family, ".lbm", ExplorerFileCategory.Image),
        Rule(Family, ".mod", ExplorerFileCategory.Audio),
        Rule(Family, ".med", ExplorerFileCategory.Audio),
        Rule(Family, ".xm", ExplorerFileCategory.Audio),
        Rule(Family, ".8svx", ExplorerFileCategory.Audio),
        Rule(Family, ".lha", ExplorerFileCategory.Archive),
        Rule(Family, ".lzh", ExplorerFileCategory.Archive),
        Rule(Family, ".zip", ExplorerFileCategory.Archive),
        Rule(Family, ".arc", ExplorerFileCategory.Archive),
        Rule(Family, ".zoo", ExplorerFileCategory.Archive),
        Rule(Family, ".dms", ExplorerFileCategory.Archive),
        Rule(Family, ".tar", ExplorerFileCategory.Archive),
        Rule(Family, ".gz", ExplorerFileCategory.Archive),
        Rule(Family, ".library", ExplorerFileCategory.Library),
        Rule(Family, ".device", ExplorerFileCategory.System),
        Rule(Family, ".handler", ExplorerFileCategory.System),
        Rule(Family, ".adf", ExplorerFileCategory.DiskImage),
        Rule(Family, ".scp", ExplorerFileCategory.DiskImage),
        Rule(Family, ".hfe", ExplorerFileCategory.DiskImage),
        Rule(Family, ".ipf", ExplorerFileCategory.DiskImage)
    ];
}
