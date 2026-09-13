using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class CommonFileTypeTable
{
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(null, ".txt", ExplorerFileCategory.Text),
        Rule(null, ".nfo", ExplorerFileCategory.Text),
        Rule(null, ".readme", ExplorerFileCategory.Text),
        Rule(null, ".doc", ExplorerFileCategory.Document),
        Rule(null, ".asm", ExplorerFileCategory.SourceCode),
        Rule(null, ".s", ExplorerFileCategory.SourceCode),
        Rule(null, ".c", ExplorerFileCategory.SourceCode),
        Rule(null, ".h", ExplorerFileCategory.SourceCode),
        Rule(null, ".for", ExplorerFileCategory.SourceCode),
        Rule(null, ".bas", ExplorerFileCategory.BasicProgram),
        Rule(null, ".ini", ExplorerFileCategory.Configuration),
        Rule(null, ".cfg", ExplorerFileCategory.Configuration),
        Rule(null, ".xml", ExplorerFileCategory.Configuration),
        Rule(null, ".json", ExplorerFileCategory.Configuration),
        Rule(null, ".html", ExplorerFileCategory.Document),
        Rule(null, ".htm", ExplorerFileCategory.Document),
        Rule(null, ".dat", ExplorerFileCategory.Data),
        Rule(null, ".obj", ExplorerFileCategory.ObjectCode),
        Rule(null, ".o", ExplorerFileCategory.ObjectCode),
        Rule(null, ".sys", ExplorerFileCategory.System),
        Rule(null, ".rom", ExplorerFileCategory.System),
        Rule(null, ".lib", ExplorerFileCategory.Library),
        Rule(null, ".library", ExplorerFileCategory.Library),
        Rule(null, ".dll", ExplorerFileCategory.Library),
        Rule(null, ".fon", ExplorerFileCategory.Font),
        Rule(null, ".fnt", ExplorerFileCategory.Font),
        Rule(null, ".ttf", ExplorerFileCategory.Font),
        Rule(null, ".bmp", ExplorerFileCategory.Image),
        Rule(null, ".gif", ExplorerFileCategory.Image),
        Rule(null, ".jpg", ExplorerFileCategory.Image),
        Rule(null, ".jpeg", ExplorerFileCategory.Image),
        Rule(null, ".png", ExplorerFileCategory.Image),
        Rule(null, ".pcx", ExplorerFileCategory.Image),
        Rule(null, ".wav", ExplorerFileCategory.Audio),
        Rule(null, ".mid", ExplorerFileCategory.Audio),
        Rule(null, ".midi", ExplorerFileCategory.Audio),
        Rule(null, ".mod", ExplorerFileCategory.Audio),
        Rule(null, ".xm", ExplorerFileCategory.Audio),
        Rule(null, ".zip", ExplorerFileCategory.Archive),
        Rule(null, ".arc", ExplorerFileCategory.Archive),
        Rule(null, ".lha", ExplorerFileCategory.Archive),
        Rule(null, ".lzh", ExplorerFileCategory.Archive),
        Rule(null, ".zoo", ExplorerFileCategory.Archive),
        Rule(null, ".tar", ExplorerFileCategory.Archive),
        Rule(null, ".gz", ExplorerFileCategory.Archive),
        Rule(null, ".scp", ExplorerFileCategory.DiskImage),
        Rule(null, ".hfe", ExplorerFileCategory.DiskImage),
        Rule(null, ".img", ExplorerFileCategory.DiskImage),
        Rule(null, ".ima", ExplorerFileCategory.DiskImage),
        Rule(null, ".dsk", ExplorerFileCategory.DiskImage),
        Rule(null, ".iso", ExplorerFileCategory.DiskImage)
    ];
}
