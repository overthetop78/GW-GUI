using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitPackedCassetteBasicProgram(FileSystemEntry entry)
    {
        if (entry.Content is not { Count: >= 1024 } data
            || !entry.Metadata.TryGetValue("sourceKind", out var sourceKind)
            || !sourceKind.Equals("atari-cas-records", StringComparison.Ordinal)
            || !entry.Metadata.TryGetValue("checksumPresent", out var checksumPresent)
            || !bool.TryParse(checksumPresent, out var hasChecksums) || !hasChecksums
            || !entry.Metadata.TryGetValue("endRecordPresent", out var endRecordPresent)
            || !bool.TryParse(endRecordPresent, out var hasEndRecord) || !hasEndRecord
            || ReadUInt16(data, 0) != 0)
            return false;

        var vntp = ReadUInt16(data, 2);
        var vntd = ReadUInt16(data, 4);
        var vvtp = ReadUInt16(data, 6);
        var stmtab = ReadUInt16(data, 8);
        var stmcur = ReadUInt16(data, 10);
        var starp = ReadUInt16(data, 12);
        var expandedLength = starp - vntp + 14;
        return vntp == 0x0200
            && vntd > vntp && vntd - vntp <= 0x0100
            && vvtp >= vntd && vvtp - vntd <= 16
            && stmtab > vvtp
            && stmcur > stmtab
            && starp >= stmcur
            && expandedLength * 100 >= data.Count * 190
            && expandedLength * 100 <= data.Count * 205;
    }
}
