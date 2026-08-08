using System.Text;
using System.Text.RegularExpressions;

namespace GWGUI.App.Localization;

public static class ExplorerWarningLocalizer
{
    private const string Prefix = "Explorer.Warning.";
    private static readonly IReadOnlyList<WarningPattern> Patterns = BuildPatterns();

    public static string Localize(string warning)
    {
        foreach (var pattern in Patterns)
        {
            var match = pattern.Regex.Match(warning);
            if (!match.Success) continue;
            var arguments = Enumerable.Range(1, match.Groups.Count - 1)
                .Select(index => (object)match.Groups[index].Value)
                .ToArray();
            return LocExtension.Get(pattern.Key, arguments);
        }

        return warning;
    }

    private static IReadOnlyList<WarningPattern> BuildPatterns() =>
        LocExtension.GetDefinedKeys("ExplorerWarnings", System.Globalization.CultureInfo.InvariantCulture)
            .Where(key => key.StartsWith(Prefix, StringComparison.Ordinal))
            .Select(key => new WarningPattern(key, CreateRegex(LocExtension.GetInvariant(key))))
            .OrderByDescending(pattern => LiteralLength(LocExtension.GetInvariant(pattern.Key)))
            .ToArray();

    private static Regex CreateRegex(string template)
    {
        var expression = new StringBuilder("^");
        var position = 0;
        foreach (Match placeholder in Regex.Matches(template, @"\{\d+\}"))
        {
            expression.Append(Regex.Escape(template[position..placeholder.Index]));
            expression.Append("(.+?)");
            position = placeholder.Index + placeholder.Length;
        }
        expression.Append(Regex.Escape(template[position..]));
        expression.Append('$');
        return new Regex(expression.ToString(), RegexOptions.CultureInvariant);
    }

    private static int LiteralLength(string template) => Regex.Replace(template, @"\{\d+\}", "").Length;

    private sealed record WarningPattern(string Key, Regex Regex);
}
