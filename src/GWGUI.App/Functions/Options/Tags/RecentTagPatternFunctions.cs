namespace GWGUI.App.Functions.Options.Tags;

internal static class RecentTagPatternFunctions
{
    private const int RecentPatternLimit = 5;

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
}
