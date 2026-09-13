using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static class ExplorerFileContentClassifier
{
    public static ExplorerFileCategory? KnownCategory(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var metadata = MetadataCategory(entry, family);
        if (metadata is not null) return metadata;
        if (IsAmigaExecutable(entry.Content) && family == ExplorerFileSystemFamily.Amiga) return ExplorerFileCategory.Executable;
        if (IsDosExecutable(entry.Content) && family == ExplorerFileSystemFamily.IbmPc) return ExplorerFileCategory.Executable;
        if (IsAtariExecutable(entry.Content) && family == ExplorerFileSystemFamily.AtariSt) return ExplorerFileCategory.Executable;
        if (HasFormType(entry.Content, "ILBM")) return ExplorerFileCategory.Image;
        if (HasFormType(entry.Content, "8SVX")) return ExplorerFileCategory.Audio;
        return null;
    }

    public static bool LooksLikeText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value => value is 9 or 10 or 13 || value >= 32 && value < 127);
        return printable >= sample.Length * 0.9;
    }

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

    private static bool IsAmigaExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 4 } && data[0] == 0 && data[1] == 0 && data[2] == 3 && data[3] == 0xF3;

    private static bool IsDosExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == (byte)'M' && data[1] == (byte)'Z';

    private static bool IsAtariExecutable(IReadOnlyList<byte>? data) =>
        data is { Count: >= 2 } && data[0] == 0x60 && data[1] == 0x1A;

    private static bool HasFormType(IReadOnlyList<byte>? data, string type) =>
        data is { Count: >= 12 } &&
        data[0] == (byte)'F' && data[1] == (byte)'O' && data[2] == (byte)'R' && data[3] == (byte)'M' &&
        data.Skip(8).Take(4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(type));
}
