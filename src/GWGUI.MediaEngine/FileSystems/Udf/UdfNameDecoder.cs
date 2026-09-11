using GWGUI.MediaEngine.Constants;

namespace GWGUI.MediaEngine.FileSystems.Udf;

/// <summary>Decodes UDF CS0 compressed Unicode strings and fixed-length d-strings.</summary>
internal static class UdfNameDecoder
{
    public static string DecodeDString(ReadOnlySpan<byte> field)
    {
        if (field.IsEmpty) return string.Empty;
        var recordedLength = field[^1];
        if (recordedLength == 0) return string.Empty;
        if (recordedLength >= field.Length)
            throw new InvalidDataException("The UDF d-string length exceeds its field.");
        return DecodeCompressedUnicode(field[..recordedLength]);
    }

    public static string DecodeCompressedUnicode(ReadOnlySpan<byte> value)
    {
        if (value.IsEmpty) return string.Empty;
        return value[0] switch
        {
            UdfConstants.CompressedUnicode8 => DecodeEightBit(value[1..]),
            UdfConstants.CompressedUnicode16 => DecodeSixteenBit(value[1..]),
            _ => throw new NotSupportedException($"Unsupported UDF compression identifier {value[0]}.")
        };
    }

    private static string DecodeEightBit(ReadOnlySpan<byte> value)
    {
        var characters = new char[value.Length];
        for (var index = 0; index < value.Length; index++) characters[index] = (char)value[index];
        return new string(characters).TrimEnd('\0');
    }

    private static string DecodeSixteenBit(ReadOnlySpan<byte> value)
    {
        if ((value.Length & 1) != 0)
            throw new InvalidDataException("A 16-bit UDF compressed Unicode string has an odd byte length.");
        return System.Text.Encoding.BigEndianUnicode.GetString(value).TrimEnd('\0');
    }
}
