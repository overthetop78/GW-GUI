using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class CommodoreFileTypeTable
{
    private const ExplorerFileSystemFamily Family = ExplorerFileSystemFamily.Commodore;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(Family, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Petscii),
        Rule(Family, ".seq", ExplorerFileCategory.Text, ExplorerTextEncoding.Petscii),
        Rule(Family, ".prg", ExplorerFileCategory.Program, execution: ExplorerExecutionKind.InterpretedProgram),
        Rule(Family, ".koa", ExplorerFileCategory.Image), Rule(Family, ".art", ExplorerFileCategory.Image),
        Rule(Family, ".iff", ExplorerFileCategory.Image), Rule(Family, ".sid", ExplorerFileCategory.Audio),
        Rule(Family, ".mus", ExplorerFileCategory.Audio), Rule(Family, ".arc", ExplorerFileCategory.Archive),
        Rule(Family, ".sda", ExplorerFileCategory.Archive), Rule(Family, ".d64", ExplorerFileCategory.DiskImage),
        Rule(Family, ".d71", ExplorerFileCategory.DiskImage), Rule(Family, ".d81", ExplorerFileCategory.DiskImage),
        Rule(Family, ".g64", ExplorerFileCategory.DiskImage), Rule(Family, ".scp", ExplorerFileCategory.DiskImage)
    ];
}
