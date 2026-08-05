namespace GWGUI.Domain.Naming;

public enum SequenceKind { Numeric, Alphabetic }

public static class SequenceFormatter
{
    public static bool TryParse(string? text, SequenceKind kind, out long value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (kind == SequenceKind.Numeric) return long.TryParse(text, out value) && value >= 0;
        try
        {
            checked
            {
                foreach (var character in text.Trim().ToUpperInvariant())
                {
                    if (character is < 'A' or > 'Z') return false;
                    value = value * 26 + character - 'A' + 1;
                }
                value--;
                return value >= 0;
            }
        }
        catch (OverflowException) { value = 0; return false; }
    }

    public static string Format(long value, SequenceKind kind, int minimumWidth)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (minimumWidth is < 1 or > 16) throw new ArgumentOutOfRangeException(nameof(minimumWidth));
        return kind == SequenceKind.Numeric
            ? value.ToString().PadLeft(minimumWidth, '0')
            : ToLetters(value).PadLeft(minimumWidth, 'A');
    }

    // Zero-based: A, B, ... Z, AA, AB, ...
    private static string ToLetters(long value)
    {
        var result = "";
        do
        {
            result = (char)('A' + value % 26) + result;
            value = value / 26 - 1;
        } while (value >= 0);
        return result;
    }
}
