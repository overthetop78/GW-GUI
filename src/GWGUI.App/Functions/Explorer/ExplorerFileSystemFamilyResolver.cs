using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.Exploration.Results;


namespace GWGUI.App.Functions.Explorer;

internal static class ExplorerFileSystemFamilyResolver
{
    public static ExplorerFileSystemFamily Resolve(ExploredDiskImage document)
    {
        var format = document.Image.FormatId;
        var fileSystem = document.Volume.FileSystemId;
        if (fileSystem.Contains("CP/M", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Cpm;
        if (format.StartsWith("acorn.dfs", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.BbcMicro;
        if (format.StartsWith("dec.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Dec;
        if (format.StartsWith("msx.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Msx;
        if (format.StartsWith("ucsd.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Ucsd;
        if (format.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Amiga;
        if (format.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.IbmPc;
        if (format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.AtariSt;
        if (format.StartsWith("atari.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Atari8Bit;
        if (format.StartsWith("apple2.dos", StringComparison.OrdinalIgnoreCase) ||
            format.StartsWith("apple2.appledos", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.AppleDos;
        if (format.StartsWith("apple2.prodos", StringComparison.OrdinalIgnoreCase) ||
            format.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.ProDos;
        if (format.StartsWith("applemac", StringComparison.OrdinalIgnoreCase) ||
            format.StartsWith("mac.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Macintosh;
        if (format.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Lisa;
        if (format.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase)) return ExplorerFileSystemFamily.Commodore;
        return ExplorerFileSystemFamily.Unknown;
    }
}
