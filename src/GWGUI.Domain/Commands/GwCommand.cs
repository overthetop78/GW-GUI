namespace GWGUI.Domain.Commands;

public sealed record GwCommand(string ExecutablePath, string Verb, IReadOnlyList<string> Arguments)
{
    public IEnumerable<string> AllArguments()
    {
        yield return Verb;
        foreach (var argument in Arguments)
            yield return argument;
    }

    public string ToDisplayString()
    {
        static string Quote(string value) => value.Length == 0 || value.Any(char.IsWhiteSpace) || value.Contains('"')
            ? $"\"{value.Replace("\"", "\\\"")}\""
            : value;

        return string.Join(" ", new[] { Quote(ExecutablePath) }.Concat(AllArguments().Select(Quote)));
    }
}
