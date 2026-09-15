using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static ExplorerFileCategory? MetadataCategory(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var type = entry.Comment.Trim();
        if (family == ExplorerFileSystemFamily.Commodore && type.StartsWith("PRG", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Program;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Text") return ExplorerFileCategory.Text;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Integer BASIC" or "Applesoft BASIC") return ExplorerFileCategory.BasicProgram;
        if (family == ExplorerFileSystemFamily.ProDos && type is "Text") return ExplorerFileCategory.Text;
        if (family == ExplorerFileSystemFamily.ProDos && type is "BASIC") return ExplorerFileCategory.BasicProgram;
        if (family == ExplorerFileSystemFamily.ProDos && type is "System") return ExplorerFileCategory.System;
        if (family == ExplorerFileSystemFamily.Macintosh)
        {
            if (type.Equals("APPL", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Executable;
            if (type.Equals("TEXT", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Text;
            if (type.Equals("PICT", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Image;
            if (type.Equals("snd", StringComparison.OrdinalIgnoreCase) || type.Equals("AIFF", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Audio;
        }
        if (family == ExplorerFileSystemFamily.Ucsd)
        {
            if (type.Equals("UCSD code file", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Executable;
            if (type.Equals("UCSD text file", StringComparison.OrdinalIgnoreCase)) return ExplorerFileCategory.Text;
            if (type is "UCSD graphics file" or "UCSD photo file") return ExplorerFileCategory.Image;
        }
        return null;
    }
}
