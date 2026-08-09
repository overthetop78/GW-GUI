namespace GWGUI.App.Options;

internal static class TagPatternFormatter
{
    private const int RecentPatternLimit = 5;
    private static readonly TagExample[] Examples =
    [
        new("Disquette", "PC", "720", "IMA"),
        new("Workbench", "AMIGA", "DD", "ADF"),
        new("Jeu", "ST", "720", "ST"),
        new("Archive", "PC", "1440", "IMG")
    ];

    internal static string Render(string pattern, string name, string family, string format, string extension, DateTime timestamp) => pattern
        .Replace("{TAG}", family + "-" + format, StringComparison.OrdinalIgnoreCase)
        .Replace("{NAME}", name, StringComparison.OrdinalIgnoreCase)
        .Replace("{FAMILY}", family, StringComparison.OrdinalIgnoreCase)
        .Replace("{FORMAT}", format, StringComparison.OrdinalIgnoreCase)
        .Replace("{EXTENSION}", extension, StringComparison.OrdinalIgnoreCase)
        .Replace("{DATE:YYYY-MM-DD}", timestamp.ToString("yyyy-MM-dd"), StringComparison.OrdinalIgnoreCase)
        .Replace("{DATE:YYYYMMDD}", timestamp.ToString("yyyyMMdd"), StringComparison.OrdinalIgnoreCase)
        .Replace("{DATE:DD-MM-YYYY}", timestamp.ToString("dd-MM-yyyy"), StringComparison.OrdinalIgnoreCase)
        .Replace("{TIME:HH-MM-SS}", timestamp.ToString("HH-mm-ss"), StringComparison.OrdinalIgnoreCase)
        .Replace("{TIME:HHMMSS}", timestamp.ToString("HHmmss"), StringComparison.OrdinalIgnoreCase)
        .Replace("{TIME:HH-MM}", timestamp.ToString("HH-mm"), StringComparison.OrdinalIgnoreCase);

    internal static string CreateExample(string pattern, int index)
    {
        var sample = Examples[Math.Abs(index % Examples.Length)];
        var rendered = Render(pattern, sample.Name, sample.Family, sample.Format, sample.Extension, new DateTime(2026, 8, 6, 14, 35, 42));
        var fileName = pattern.Contains("{NAME}", StringComparison.OrdinalIgnoreCase) ? rendered : rendered + sample.Name;
        return fileName + "." + sample.Extension.ToLowerInvariant();
    }

    internal static bool Remember(List<string> recentPatterns, string pattern)
    {
        pattern = pattern.Trim();
        if (string.IsNullOrEmpty(pattern)) return false;
        recentPatterns.RemoveAll(item => string.Equals(item, pattern, StringComparison.OrdinalIgnoreCase));
        recentPatterns.Insert(0, pattern);
        if (recentPatterns.Count > RecentPatternLimit)
            recentPatterns.RemoveRange(RecentPatternLimit, recentPatterns.Count - RecentPatternLimit);
        return true;
    }

    private sealed record TagExample(string Name, string Family, string Format, string Extension);
}
