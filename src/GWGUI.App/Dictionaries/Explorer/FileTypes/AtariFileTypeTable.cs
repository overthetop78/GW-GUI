using GWGUI.App.Contracts.Explorer;
using GWGUI.App.Enums.Explorer;
using static GWGUI.App.Dictionaries.Explorer.FileTypes.ExplorerFileTypeRuleFactory;

namespace GWGUI.App.Dictionaries.Explorer.FileTypes;

internal static class AtariFileTypeTable
{
    private const ExplorerFileSystemFamily St = ExplorerFileSystemFamily.AtariSt;
    private const ExplorerFileSystemFamily EightBit = ExplorerFileSystemFamily.Atari8Bit;
    public static IReadOnlyList<ExplorerFileTypeDefinition> Rows { get; } =
    [
        Rule(St, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Unknown),
        Rule(St, ".doc", ExplorerFileCategory.Document, ExplorerTextEncoding.Unknown),
        Rule(St, ".asc", ExplorerFileCategory.Text, ExplorerTextEncoding.Ascii),
        Rule(St, ".inf", ExplorerFileCategory.Configuration, ExplorerTextEncoding.Unknown),
        Rule(St, ".cfg", ExplorerFileCategory.Configuration, ExplorerTextEncoding.Unknown),
        Rule(St, ".asm", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Unknown),
        Rule(St, ".s", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Unknown),
        Rule(St, ".c", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Unknown),
        Rule(St, ".h", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Unknown),
        Rule(St, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(St, ".neo", ExplorerFileCategory.Image), Rule(St, ".pi1", ExplorerFileCategory.Image),
        Rule(St, ".pi2", ExplorerFileCategory.Image), Rule(St, ".pi3", ExplorerFileCategory.Image),
        Rule(St, ".pc1", ExplorerFileCategory.Image), Rule(St, ".pc2", ExplorerFileCategory.Image),
        Rule(St, ".pc3", ExplorerFileCategory.Image), Rule(St, ".deg", ExplorerFileCategory.Image),
        Rule(St, ".iff", ExplorerFileCategory.Image), Rule(St, ".mod", ExplorerFileCategory.Audio),
        Rule(St, ".snd", ExplorerFileCategory.Audio), Rule(St, ".ym", ExplorerFileCategory.Audio),
        Rule(St, ".zip", ExplorerFileCategory.Archive), Rule(St, ".arc", ExplorerFileCategory.Archive),
        Rule(St, ".lzh", ExplorerFileCategory.Archive), Rule(St, ".lha", ExplorerFileCategory.Archive),
        Rule(St, ".zoo", ExplorerFileCategory.Archive),
        Rule(St, ".prg", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(St, ".ttp", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(St, ".tos", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(St, ".app", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(St, ".gtp", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(St, ".acc", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(St, ".st", ExplorerFileCategory.DiskImage), Rule(St, ".msa", ExplorerFileCategory.DiskImage),
        Rule(St, ".scp", ExplorerFileCategory.DiskImage), Rule(St, ".hfe", ExplorerFileCategory.DiskImage),
        Rule(St, ".stx", ExplorerFileCategory.DiskImage), Rule(St, ".ipf", ExplorerFileCategory.DiskImage),

        Rule(EightBit, ".txt", ExplorerFileCategory.Text, ExplorerTextEncoding.Atascii),
        Rule(EightBit, ".doc", ExplorerFileCategory.Document, ExplorerTextEncoding.Atascii),
        Rule(EightBit, ".lst", ExplorerFileCategory.Text, ExplorerTextEncoding.Atascii),
        Rule(EightBit, ".asm", ExplorerFileCategory.SourceCode, ExplorerTextEncoding.Atascii),
        Rule(EightBit, ".bas", ExplorerFileCategory.BasicProgram, ExplorerTextEncoding.Unknown),
        Rule(EightBit, ".mic", ExplorerFileCategory.Image), Rule(EightBit, ".pic", ExplorerFileCategory.Image),
        Rule(EightBit, ".sap", ExplorerFileCategory.Audio), Rule(EightBit, ".arc", ExplorerFileCategory.Archive),
        Rule(EightBit, ".com", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(EightBit, ".xex", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(EightBit, ".exe", ExplorerFileCategory.Executable, execution: ExplorerExecutionKind.NativeExecutable),
        Rule(EightBit, ".atr", ExplorerFileCategory.DiskImage), Rule(EightBit, ".atx", ExplorerFileCategory.DiskImage), Rule(EightBit, ".xfd", ExplorerFileCategory.DiskImage),
        Rule(EightBit, ".scp", ExplorerFileCategory.DiskImage)
    ];
}
