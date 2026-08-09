using System.Text;

namespace GWGUI.Domain.Commands;

public static class CommandLineTokenizer
{
    public static IReadOnlyList<string> Tokenize(string value)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var quoted = false;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '"')
            {
                quoted = !quoted;
                continue;
            }
            if (char.IsWhiteSpace(character) && !quoted)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }
            current.Append(character);
        }
        if (quoted) throw new ArgumentException("An expert argument contains an unclosed quote.");
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }
}
