namespace GWGUI.App.ViewModels.Options;

public sealed record RecentTagPatternOption(int Number, string? Pattern)
{
    public string Display => string.IsNullOrWhiteSpace(Pattern) ? "—" : Pattern;
}
