namespace GWGUI.MediaEngine.FileSystems.Dec.Rt11;

/// <summary>Décode les mots RADIX-50 utilisés par RT-11.</summary>
public static class Rt11Radix50
{
    /// <summary>Définit la valeur RT-11 nommée <c>Alphabet</c>.</summary>
    public const string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ$.%0123456789";
    /// <summary>Définit la valeur RT-11 nommée <c>Base</c>.</summary>
    public const int Base = 40;
    /// <summary>Définit la valeur RT-11 nommée <c>CharacterCount</c>.</summary>
    public const int CharacterCount = 3;
    /// <summary>Définit la valeur RT-11 nommée <c>FirstDivisor</c>.</summary>
    public const int FirstDivisor = Base * Base;
    /// <summary>Définit la valeur RT-11 nommée <c>SecondDivisor</c>.</summary>
    public const int SecondDivisor = Base;

    /// <summary>Décode un mot en trois caractères.</summary>
    public static string Decode(ushort word)
    {
        Span<char> result = stackalloc char[CharacterCount];
        result[0] = Alphabet[word / FirstDivisor % Base];
        result[1] = Alphabet[word / SecondDivisor % Base];
        result[2] = Alphabet[word % Base];
        return new(result);
    }
}
