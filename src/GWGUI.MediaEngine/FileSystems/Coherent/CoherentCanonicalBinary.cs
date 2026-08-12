using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Lit les entiers enregistrés dans l'ordre canonique COHERENT 2, 3, 0, 1.</summary>
internal static class CoherentCanonicalBinary
{
    /// <summary>Nombre d'octets d'un entier canonique 32 bits.</summary>
    public const int UInt32Length = sizeof(uint);
    /// <summary>Position de l'octet de poids faible.</summary>
    public const int LowByteOffset = 2;
    /// <summary>Position du deuxième octet.</summary>
    public const int LowMiddleByteOffset = 3;
    /// <summary>Position du troisième octet.</summary>
    public const int HighMiddleByteOffset = 0;
    /// <summary>Position de l'octet de poids fort.</summary>
    public const int HighByteOffset = 1;

    /// <summary>Lit un entier 32 bits dans l'ordre canonique COHERENT.</summary>
    public static uint ReadUInt32(ReadOnlySpan<byte> value)
    {
        if (value.Length < UInt32Length) throw CoherentCanonicalBinaryExceptions.InsufficientLength(value.Length, UInt32Length, nameof(value));
        return (uint)(value[LowByteOffset] | value[LowMiddleByteOffset] << BitPrimitives.BitsPerByte | value[HighMiddleByteOffset] << 16 | value[HighByteOffset] << 24);
    }
}
