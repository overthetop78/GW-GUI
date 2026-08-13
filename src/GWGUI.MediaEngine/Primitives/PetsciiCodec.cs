namespace GWGUI.MediaEngine.Primitives;

/// <summary>Décode les caractères PETSCII employés dans les structures Commodore.</summary>
public static class PetsciiCodec
{
    /// <summary>Octet terminant une chaîne vide.</summary>
    public const byte NullTerminator = 0;
    /// <summary>Octet de remplissage PETSCII.</summary>
    public const byte ShiftedSpaceTerminator = 0xa0;
    /// <summary>Masque supprimant le bit vidéo inversé.</summary>
    public const byte CharacterMask = 0x7f;
    /// <summary>Première valeur ASCII directement représentable.</summary>
    public const byte DirectRangeStart = 0x20;
    /// <summary>Dernière valeur ASCII directement représentable.</summary>
    public const byte DirectRangeEnd = 0x5f;
    /// <summary>Première lettre PETSCII devant être convertie en majuscule.</summary>
    public const byte LowerRangeStart = 0x60;
    /// <summary>Dernière lettre PETSCII devant être convertie en majuscule.</summary>
    public const byte LowerRangeEnd = 0x7a;
    /// <summary>Décalage séparant les plages de casse ASCII.</summary>
    public const byte CaseOffset = 0x20;

    /// <summary>Décode une séquence PETSCII et retire son remplissage final.</summary>
    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        var characters = new List<char>(bytes.Length);
        foreach (var raw in bytes)
        {
            if (raw is NullTerminator or ShiftedSpaceTerminator) break;
            var value = (byte)(raw & CharacterMask);
            characters.Add(value switch { >= DirectRangeStart and <= DirectRangeEnd => (char)value, >= LowerRangeStart and <= LowerRangeEnd => (char)(value - CaseOffset), _ => '\ufffd' });
        }
        return new string(characters.ToArray()).Trim();
    }

    /// <summary>Encode les caractères PETSCII communs et complète le champ avec des espaces décalés.</summary>
    public static byte[] Encode(string value, int length)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));
        if (value.Length > length) throw new ArgumentException($"The PETSCII value exceeds {length} characters.", nameof(value));
        var result = Enumerable.Repeat(ShiftedSpaceTerminator, length).ToArray();
        for (var index = 0; index < value.Length; index++)
        {
            var character = char.ToUpperInvariant(value[index]);
            if (character is < (char)DirectRangeStart or > (char)DirectRangeEnd) throw new ArgumentException($"The character '{value[index]}' cannot be represented by the supported PETSCII subset.", nameof(value));
            result[index] = (byte)character;
        }
        return result;
    }
}
