using GWGUI.App.Enums.Explorer;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.App.Functions.Explorer;

internal static partial class ExplorerFileContentClassifier
{
    private static bool IsAtari8BitBasicProgram(IReadOnlyList<byte>? data)
    {
        if (data is not { Count: >= 14 } || ReadUInt16(data, 0) != 0) return false;
        var vntp = ReadUInt16(data, 2);
        var vntd = ReadUInt16(data, 4);
        var vvtp = ReadUInt16(data, 6);
        var stmtab = ReadUInt16(data, 8);
        var stmcur = ReadUInt16(data, 10);
        var starp = ReadUInt16(data, 12);
        var origin = vntp - 14;
        var normalizedVntp = vntp - origin;
        var normalizedVntd = vntd - origin;
        var normalizedVvtp = vvtp - origin;
        var normalizedStmtab = stmtab - origin;
        var normalizedStmcur = stmcur - origin;
        var normalizedStarp = starp - origin;
        if (vntp < 0x0100
            || normalizedVntp != 14
            || normalizedVntp > normalizedVntd
            || normalizedVntd > normalizedVvtp
            || normalizedVvtp > normalizedStmtab
            || normalizedStmtab >= data.Count) return false;
        if (normalizedStmtab < normalizedStmcur
            && normalizedStmcur <= normalizedStarp
            && normalizedStarp <= data.Count) return true;
        return HasValidAtari8BitBasicLineTable(data, normalizedStmtab);
    }
}
