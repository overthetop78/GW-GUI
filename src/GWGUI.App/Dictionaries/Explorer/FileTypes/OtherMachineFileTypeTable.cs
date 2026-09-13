using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class OtherMachineFileTypeTable
{
    private const ExplorerFileSystemFamily Bbc = ExplorerFileSystemFamily.BbcMicro;
    private const ExplorerFileSystemFamily Dec = ExplorerFileSystemFamily.Dec;
    private const ExplorerFileSystemFamily Msx = ExplorerFileSystemFamily.Msx;
    private const ExplorerFileSystemFamily Ucsd = ExplorerFileSystemFamily.Ucsd;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(Bbc, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Ascii),
        Rule(Bbc, ".zip", ExplorerFileCategory.Archive), Rule(Bbc, ".ssd", ExplorerFileCategory.DiskImage),
        Rule(Bbc, ".dsd", ExplorerFileCategory.DiskImage), Rule(Bbc, ".adl", ExplorerFileCategory.DiskImage),
        Rule(Bbc, ".adm", ExplorerFileCategory.DiskImage), Rule(Bbc, ".adf", ExplorerFileCategory.DiskImage),
        Rule(Bbc, ".scp", ExplorerFileCategory.DiskImage),

        Rule(Dec, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Ascii),
        Rule(Dec, ".mac", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Dec, ".for", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Ascii),
        Rule(Dec, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(Dec, ".cmd", ExplorerFileCategory.Command, ExplorerTextEncoding.Ascii, ExplorerExecutionKind.CommandScript),
        Rule(Dec, ".com", ExplorerFileCategory.Command, ExplorerTextEncoding.Ascii, ExplorerExecutionKind.CommandScript),
        Rule(Dec, ".bup", ExplorerFileCategory.Archive), Rule(Dec, ".sav", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Dec, ".lda", ExplorerFileCategory.ObjectCode), Rule(Dec, ".rel", ExplorerFileCategory.ObjectCode),
        Rule(Dec, ".obj", ExplorerFileCategory.ObjectCode), Rule(Dec, ".sys", ExplorerFileCategory.System),
        Rule(Dec, ".img", ExplorerFileCategory.DiskImage), Rule(Dec, ".dsk", ExplorerFileCategory.DiskImage),
        Rule(Dec, ".scp", ExplorerFileCategory.DiskImage),

        Rule(Msx, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Msx),
        Rule(Msx, ".doc", ExplorerFileCategory.Document, ExplorerTextEncoding.Msx),
        Rule(Msx, ".asc", ExplorerFileCategory.Text, ExplorerTextEncoding.Ascii),
        Rule(Msx, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(Msx, ".sc2", ExplorerFileCategory.Image), Rule(Msx, ".sc5", ExplorerFileCategory.Image),
        Rule(Msx, ".sc7", ExplorerFileCategory.Image), Rule(Msx, ".sc8", ExplorerFileCategory.Image),
        Rule(Msx, ".mgs", ExplorerFileCategory.Audio), Rule(Msx, ".bgm", ExplorerFileCategory.Audio),
        Rule(Msx, ".lzh", ExplorerFileCategory.Archive), Rule(Msx, ".pma", ExplorerFileCategory.Archive),
        Rule(Msx, ".com", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Msx, ".rom", ExplorerFileCategory.System), Rule(Msx, ".dsk", ExplorerFileCategory.DiskImage),
        Rule(Msx, ".scp", ExplorerFileCategory.DiskImage),

        Rule(Ucsd, ".text", ExplorerFileCategory.Text, ExplorerTextEncoding.Ascii),
        Rule(Ucsd, ".foto", ExplorerFileCategory.Image), Rule(Ucsd, ".graf", ExplorerFileCategory.Image),
        Rule(Ucsd, ".code", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Ucsd, ".td0", ExplorerFileCategory.DiskImage), Rule(Ucsd, ".img", ExplorerFileCategory.DiskImage),
        Rule(Ucsd, ".dsk", ExplorerFileCategory.DiskImage), Rule(Ucsd, ".scp", ExplorerFileCategory.DiskImage)
    ];
}
