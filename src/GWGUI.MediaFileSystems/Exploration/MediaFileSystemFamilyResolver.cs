using GWGUI.MediaAnalysis.Enums;

namespace GWGUI.MediaFileSystems.Exploration;

/// <summary>Selects the file family from identifiers already known after extraction.</summary>
internal static class MediaFileSystemFamilyResolver
{
    public static MediaFileSystemFamily Resolve(string formatId, string? fileSystemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        var fileSystem = fileSystemId ?? string.Empty;
        if (formatId.Equals("tape.atari-cas", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Atari8Bit;
        if (fileSystem.Contains("CP/M", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Cpm;
        if (formatId.StartsWith("acorn.dfs", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.BbcMicro;
        if (formatId.StartsWith("dec.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Dec;
        if (formatId.StartsWith("msx.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Msx;
        if (formatId.StartsWith("ucsd.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Ucsd;
        if (formatId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Amiga;
        if (formatId.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.IbmPc;
        if (formatId.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.AtariTos;
        if (formatId.StartsWith("atari.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Atari8Bit;
        if (formatId.StartsWith("apple2.dos", StringComparison.OrdinalIgnoreCase) ||
            formatId.StartsWith("apple2.appledos", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.AppleDos;
        if (formatId.StartsWith("apple2.prodos", StringComparison.OrdinalIgnoreCase) ||
            formatId.StartsWith("apple3.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.ProDos;
        if (formatId.StartsWith("applemac", StringComparison.OrdinalIgnoreCase) ||
            formatId.StartsWith("mac.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Macintosh;
        if (formatId.StartsWith("applelisa", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Lisa;
        if (formatId.StartsWith("commodore.", StringComparison.OrdinalIgnoreCase)) return MediaFileSystemFamily.Commodore;

        return MediaFileSystemFamily.Unknown;
    }
}
