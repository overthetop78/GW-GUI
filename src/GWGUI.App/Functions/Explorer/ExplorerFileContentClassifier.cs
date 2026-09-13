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
        if (family == ExplorerFileSystemFamily.Atari8Bit)
        {
            if (IsAtari8BitBasicProgram(entry.Content)) return ExplorerFileCategory.BasicProgram;
            if (IsAtari8BitXex(entry.Content)) return ExplorerFileCategory.Executable;
            if (IsAtari8BitCassetteBootProgram(entry.Content)) return ExplorerFileCategory.BootProgram;
        }
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

    private static bool IsAtari8BitBasicProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 14 } || ReadUInt16(data, 0) != 0) return false;
        var vntp = ReadUInt16(data, 2);
        var vntd = ReadUInt16(data, 4);
        var vvtp = ReadUInt16(data, 6);
        var stmtab = ReadUInt16(data, 8);
        var stmcur = ReadUInt16(data, 10);
        var starp = ReadUInt16(data, 12);
        return vntp >= 0x0100
            && vntp <= vntd
            && vntd <= vvtp
            && vvtp <= stmtab
            && stmtab < stmcur
            && stmcur <= starp
            && starp - vntp <= data.Count - 14;
    }

    private static bool IsAtari8BitXex(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 8 } || ReadUInt16(data, 0) != 0xffff) return false;
        var offset = 2;
        var segments = 0;
        while (offset < data.Count)
        {
            while (offset + 1 < data.Count && ReadUInt16(data, offset) == 0xffff) offset += 2;
            if (offset + 4 > data.Count) return false;
            var start = ReadUInt16(data, offset);
            var end = ReadUInt16(data, offset + 2);
            if (end < start) return false;
            offset += 4;
            var length = end - start + 1;
            if (offset + length > data.Count) return false;
            offset += length;
            segments++;
        }
        return segments > 0;
    }

    private static bool IsAtari8BitCassetteBootProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 128 } || data[0] != 0 || data[1] == 0) return false;
        var recordCount = data[1];
        var loadAddress = ReadUInt16(data, 2);
        var initAddress = ReadUInt16(data, 4);
        return loadAddress >= 0x0200
            && initAddress >= 0x0200
            && loadAddress + recordCount * 128 <= 0x10000
            && data.Count >= recordCount * 128;
    }

    private static int ReadUInt16(IReadOnlyList<byte> data, int offset) =>
        data[offset] | data[offset + 1] << 8;

    private static bool HasFormType(IReadOnlyList<byte>? data, string type) =>
        data is { Count: >= 12 } &&
        data[0] == (byte)'F' && data[1] == (byte)'O' && data[2] == (byte)'R' && data[3] == (byte)'M' &&
        data.Skip(8).Take(4).SequenceEqual(System.Text.Encoding.ASCII.GetBytes(type));
}
