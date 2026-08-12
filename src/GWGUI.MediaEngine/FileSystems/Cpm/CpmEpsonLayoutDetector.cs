using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Recherche le répertoire des variantes Epson sur les frontières logiques CP/M.</summary>
internal static class CpmEpsonLayoutDetector
{
    /// <summary>Conserve le layout configuré en cas d'égalité et retourne le premier offset strictement mieux noté.</summary>
    public static CpmLayout Resolve(string formatId, CpmDirectoryReader.LogicalImage image, CpmLayout configured)
    {
        if (!formatId.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase)) return configured;
        var best = configured;
        var bestScore = CpmDirectoryReader.ScoreDirectory(image, configured, rejectLowercase: true);
        var limit = Math.Min(image.Bytes.Length - configured.DirectoryEntries * CpmFormat.DirectoryEntrySize, CpmFormat.MaximumEpsonDirectorySearchLength);
        for (var offset = 0; offset <= limit; offset += CpmFormat.DirectoryEntrySize)
        {
            var candidate = configured with { DirectoryOffset = offset };
            var score = CpmDirectoryReader.ScoreDirectory(image, candidate, rejectLowercase: true);
            if (score <= bestScore) continue;
            best = candidate;
            bestScore = score;
        }
        return best;
    }
}
