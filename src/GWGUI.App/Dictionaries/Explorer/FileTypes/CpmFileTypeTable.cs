using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class CpmFileTypeTable
{
    private const ExplorerFileSystemFamily Family = ExplorerFileSystemFamily.Cpm;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(Family, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Ascii),
        Rule(Family, ".doc", ExplorerFileCategory.Document, ExplorerTextEncoding.Ascii),
        Rule(Family, ".asm", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Family, ".mac", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Family, ".for", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Family, ".c", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Family, ".h", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Family, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(Family, ".lib", ExplorerFileCategory.Library),
        Rule(Family, ".arc", ExplorerFileCategory.Archive), Rule(Family, ".lbr", ExplorerFileCategory.Archive),
        Rule(Family, ".com", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Family, ".cmd", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Family, ".sub", ExplorerFileCategory.Command, ExplorerTextEncoding.Ascii, ExplorerExecutionKind.CommandScript),
        Rule(Family, ".dsk", ExplorerFileCategory.DiskImage), Rule(Family, ".edsk", ExplorerFileCategory.DiskImage),
        Rule(Family, ".scp", ExplorerFileCategory.DiskImage)
    ];
}
