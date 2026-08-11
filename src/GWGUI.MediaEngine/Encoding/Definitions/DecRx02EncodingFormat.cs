using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding.Definitions;

/// <summary>Regroupe les définitions techniques du format Dec Rx02 Encoding.</summary>
internal static class DecRx02EncodingFormat
{
    /// <summary>Définit en-tête marque hex utilisé par ce format.</summary>
    public const string HeaderMarkHex = "55111554";
    /// <summary>Définit en-tête adresse marque utilisé par ce format.</summary>
    public const byte HeaderAddressMark = 0xfe;
    /// <summary>Définit fm supprimées données marque utilisé par ce format.</summary>
    public const byte FmDeletedDataMark = 0xf8;
    /// <summary>Définit m2 fm données marque utilisé par ce format.</summary>
    public const byte M2FmDataMark = 0xf9;
    /// <summary>Définit données marque fa utilisé par ce format.</summary>
    public const byte DataMarkFa = 0xfa;
    /// <summary>Définit fm données marque utilisé par ce format.</summary>
    public const byte FmDataMark = 0xfb;
    /// <summary>Définit données marque fc utilisé par ce format.</summary>
    public const byte DataMarkFc = 0xfc;
    /// <summary>Définit m2 fm supprimées données marque utilisé par ce format.</summary>
    public const byte M2FmDeletedDataMark = 0xfd;
    /// <summary>Définit marque octet nombre utilisé par ce format.</summary>
    public const int MarkByteCount = 4;
    /// <summary>Définit marque bit nombre utilisé par ce format.</summary>
    public const int MarkBitCount = MarkByteCount * BitPrimitives.BitsPerByte;
    /// <summary>Définit données marque octet nombre utilisé par ce format.</summary>
    public const int DataMarkByteCount = 1;
    /// <summary>Définit en-tête décodé octet nombre utilisé par ce format.</summary>
    public const int HeaderDecodedByteCount = 6;
    /// <summary>Définit crc octet nombre utilisé par ce format.</summary>
    public const int CrcByteCount = 2;
    /// <summary>Définit fm secteur octet nombre utilisé par ce format.</summary>
    public const int FmSectorByteCount = 128;
    /// <summary>Définit m2 fm secteur octet nombre utilisé par ce format.</summary>
    public const int M2FmSectorByteCount = DecRx02Geometry.PhysicalSectorSize;
    /// <summary>Définit fm secteur taille code utilisé par ce format.</summary>
    public const byte FmSectorSizeCode = 0;
    /// <summary>Définit m2 fm secteur taille code utilisé par ce format.</summary>
    public const byte M2FmSectorSizeCode = 1;
    /// <summary>Définit encodé mfm octet bit nombre utilisé par ce format.</summary>
    public const int EncodedMfmByteBitCount = 16;
    /// <summary>Définit encodé fm octet bit nombre utilisé par ce format.</summary>
    public const int EncodedFmByteBitCount = 32;
    /// <summary>Définit m2 fm phase bit nombre utilisé par ce format.</summary>
    public const int M2FmPhaseBitCount = 1;
    /// <summary>Définit en-tête bit nombre utilisé par ce format.</summary>
    public const int HeaderBitCount = MarkBitCount + HeaderDecodedByteCount * EncodedFmByteBitCount;
    /// <summary>Définit intervalle bit nombre utilisé par ce format.</summary>
    public const int GapBitCount = 64;
    /// <summary>Définit données recherche octet nombre utilisé par ce format.</summary>
    public const int DataSearchByteCount = 88 + 16;
    /// <summary>Définit crc polynôme utilisé par ce format.</summary>
    public const ushort CrcPolynomial = Crc16Calculator.CcittPolynomial;
    /// <summary>Définit crc initiale valeur utilisé par ce format.</summary>
    public const ushort CrcInitialValue = Crc16Calculator.AllBitsSetInitialValue;
    /// <summary>Expose normal m2 fm règle utilisé par ce format.</summary>
    public static IReadOnlyList<bool> NormalM2FmRule { get; } = Array.AsReadOnly([false,false,true,false,true,false,true,false,true,false,false]);
    /// <summary>Expose encodé m2 fm règle utilisé par ce format.</summary>
    public static IReadOnlyList<bool> EncodedM2FmRule { get; } = Array.AsReadOnly([false,true,false,false,false,true,false,false,false,true,false]);
    /// <summary>Expose en-tête marque utilisé par ce format.</summary>
    public static IReadOnlyList<byte> HeaderMark { get; } = Array.AsReadOnly(Convert.FromHexString(HeaderMarkHex));
    /// <summary>Expose données marks utilisé par ce format.</summary>
    public static IReadOnlyList<(IReadOnlyList<byte> Pattern, byte Mark)> DataMarks { get; } = Array.AsReadOnly<(IReadOnlyList<byte>, byte)>([
        (Array.AsReadOnly(Convert.FromHexString("55111444")), FmDeletedDataMark),
        (Array.AsReadOnly(Convert.FromHexString("55111445")), M2FmDataMark),
        (Array.AsReadOnly(Convert.FromHexString("55111454")), DataMarkFa),
        (Array.AsReadOnly(Convert.FromHexString("55111455")), FmDataMark),
        (Array.AsReadOnly(Convert.FromHexString("55111544")), DataMarkFc),
        (Array.AsReadOnly(Convert.FromHexString("55111545")), M2FmDeletedDataMark)]);
    /// <summary>Crée l'exception signalant invalide secteur taille.</summary>
    /// <param name="actualSize">Valeur observée utilisée pour décrire précisément l'erreur.</param>
    /// <returns>Exception contenant les valeurs attendues et observées.</returns>
    public static ArgumentException InvalidSectorSize(int actualSize) => new($"DEC RX sectors contain {FmSectorByteCount} or {M2FmSectorByteCount} bytes; received {actualSize} bytes.");
}
