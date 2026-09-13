using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class IbmPcFileTypeTable
{
    private const ExplorerFileSystemFamily Family = ExplorerFileSystemFamily.IbmPc;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(Family, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.DosOem),
        Rule(Family, ".nfo", ExplorerFileCategory.Text, ExplorerTextEncoding.DosOem),
        Rule(Family, ".readme", ExplorerFileCategory.Text, ExplorerTextEncoding.DosOem),
        Rule(Family, ".doc", ExplorerFileCategory.Document, ExplorerTextEncoding.Unknown),
        Rule(Family, ".asm", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.DosOem),
        Rule(Family, ".c", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.DosOem),
        Rule(Family, ".h", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.DosOem),
        Rule(Family, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(Family, ".ini", ExplorerFileCategory.Configuration, ExplorerTextEncoding.DosOem),
        Rule(Family, ".cfg", ExplorerFileCategory.Configuration, ExplorerTextEncoding.DosOem),
        Rule(Family, ".xml", ExplorerFileCategory.Configuration, ExplorerTextEncoding.Unknown),
        Rule(Family, ".html", ExplorerFileCategory.Document, ExplorerTextEncoding.Unknown),
        Rule(Family, ".bmp", ExplorerFileCategory.Image), Rule(Family, ".gif", ExplorerFileCategory.Image),
        Rule(Family, ".jpg", ExplorerFileCategory.Image), Rule(Family, ".jpeg", ExplorerFileCategory.Image),
        Rule(Family, ".png", ExplorerFileCategory.Image), Rule(Family, ".pcx", ExplorerFileCategory.Image),
        Rule(Family, ".wav", ExplorerFileCategory.Audio), Rule(Family, ".voc", ExplorerFileCategory.Audio),
        Rule(Family, ".mid", ExplorerFileCategory.Audio), Rule(Family, ".midi", ExplorerFileCategory.Audio),
        Rule(Family, ".s3m", ExplorerFileCategory.Audio), Rule(Family, ".xm", ExplorerFileCategory.Audio),
        Rule(Family, ".zip", ExplorerFileCategory.Archive), Rule(Family, ".arc", ExplorerFileCategory.Archive),
        Rule(Family, ".arj", ExplorerFileCategory.Archive), Rule(Family, ".lha", ExplorerFileCategory.Archive),
        Rule(Family, ".lzh", ExplorerFileCategory.Archive), Rule(Family, ".zoo", ExplorerFileCategory.Archive),
        Rule(Family, ".tar", ExplorerFileCategory.Archive), Rule(Family, ".gz", ExplorerFileCategory.Archive),
        Rule(Family, ".exe", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Family, ".com", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(Family, ".bat", ExplorerFileCategory.Command, ExplorerTextEncoding.DosOem, ExplorerExecutionKind.CommandScript),
        Rule(Family, ".cmd", ExplorerFileCategory.Command, ExplorerTextEncoding.DosOem, ExplorerExecutionKind.CommandScript),
        Rule(Family, ".sys", ExplorerFileCategory.System), Rule(Family, ".dll", ExplorerFileCategory.Library),
        Rule(Family, ".obj", ExplorerFileCategory.ObjectCode), Rule(Family, ".lib", ExplorerFileCategory.Library),
        Rule(Family, ".ima", ExplorerFileCategory.DiskImage), Rule(Family, ".img", ExplorerFileCategory.DiskImage),
        Rule(Family, ".scp", ExplorerFileCategory.DiskImage), Rule(Family, ".hfe", ExplorerFileCategory.DiskImage),
        Rule(Family, ".td0", ExplorerFileCategory.DiskImage), Rule(Family, ".imd", ExplorerFileCategory.DiskImage),
        Rule(Family, ".86f", ExplorerFileCategory.DiskImage)
    ];
}
