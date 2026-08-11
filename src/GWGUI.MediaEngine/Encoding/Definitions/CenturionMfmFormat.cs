namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Centurion Mfm.</summary>
internal static class CenturionMfmFormat
{
    /// <summary>Définit supported données key utilisé par ce format.</summary>
    public const byte SupportedDataKey = 0;
    /// <summary>Définit en-tête octet nombre utilisé par ce format.</summary>
    public const int HeaderByteCount = 4;
    /// <summary>Définit données préfixe octet nombre utilisé par ce format.</summary>
    public const int DataPrefixByteCount = 3;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit allocation bloc taille utilisé par ce format.</summary>
    public const int AllocationBlockSize = 256;
    /// <summary>Définit en-tête intervalle bit nombre utilisé par ce format.</summary>
    public const int HeaderGapBitCount = 400;
    /// <summary>Définit données intervalle bit nombre utilisé par ce format.</summary>
    public const int DataGapBitCount = 128;
    /// <summary>Définit crc polynôme utilisé par ce format.</summary>
    public const ushort CrcPolynomial = Primitives.Crc16Calculator.CcittPolynomial;
    /// <summary>Définit crc initiale valeur utilisé par ce format.</summary>
    public const ushort CrcInitialValue = Primitives.Crc16Calculator.ZeroInitialValue;
    /// <summary>Expose secteur marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> SectorMark { get; } = Array.AsReadOnly<byte>([0x91,0x22,0x44,0x89]);
    /// <summary>Expose données marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> DataMark { get; } = Array.AsReadOnly<byte>([0xaa,0xaa,0xaa,0xa9]);
}
