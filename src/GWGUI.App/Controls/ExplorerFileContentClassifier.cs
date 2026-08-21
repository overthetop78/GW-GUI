using GWGUI.MediaEngine.FileSystems;
using GWGUI.App.Enums;

namespace GWGUI.App.Controls;

internal static class ExplorerFileContentClassifier
{
    public static ExplorerIconCategory? KnownIcon(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var metadata = MetadataIcon(entry, family);
        if (metadata is not null) return metadata;
        if (IsAmigaExecutable(entry.Content) && family == ExplorerFileSystemFamily.Amiga) return ExplorerIconCategory.Program;
        if (IsDosExecutable(entry.Content) && family == ExplorerFileSystemFamily.IbmPc) return ExplorerIconCategory.Program;
        if (IsAtariExecutable(entry.Content) && family == ExplorerFileSystemFamily.AtariSt) return ExplorerIconCategory.Program;
        if (HasFormType(entry.Content, "ILBM")) return ExplorerIconCategory.Image;
        if (HasFormType(entry.Content, "8SVX")) return ExplorerIconCategory.Audio;
        return null;
    }

    public static bool LooksLikeText(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: > 0 }) return false;
        var sample = data.Take(Math.Min(data.Count, 512)).ToArray();
        var printable = sample.Count(value => value is 9 or 10 or 13 || value >= 32 && value < 127);
        return printable >= sample.Length * 0.9;
    }

    private static ExplorerIconCategory? MetadataIcon(FileSystemEntry entry, ExplorerFileSystemFamily family)
    {
        var type = entry.Comment.Trim();
        if (family == ExplorerFileSystemFamily.Commodore && type.StartsWith("PRG", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Program;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Text") return ExplorerIconCategory.Text;
        if (family == ExplorerFileSystemFamily.AppleDos && type is "Integer BASIC" or "Applesoft BASIC") return ExplorerIconCategory.Program;
        if (family == ExplorerFileSystemFamily.ProDos && type is "Text") return ExplorerIconCategory.Text;
        if (family == ExplorerFileSystemFamily.ProDos && type is "BASIC" or "System") return ExplorerIconCategory.Program;
        if (family == ExplorerFileSystemFamily.Macintosh)
        {
            if (type.Equals("APPL", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Program;
            if (type.Equals("TEXT", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Text;
            if (type.Equals("PICT", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Image;
            if (type.Equals("snd", StringComparison.OrdinalIgnoreCase) || type.Equals("AIFF", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Audio;
        }
        if (family == ExplorerFileSystemFamily.Ucsd)
        {
            if (type.Equals("UCSD code file", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Program;
            if (type.Equals("UCSD text file", StringComparison.OrdinalIgnoreCase)) return ExplorerIconCategory.Text;
            if (type is "UCSD graphics file" or "UCSD photo file") return ExplorerIconCategory.Image;
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
