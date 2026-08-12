namespace GWGUI.MediaEngine.FileSystems.Acorn.Adfs;

/// <summary>Décode les noms ADFS codés sur sept bits.</summary>
public static class AcornAdfsNameCodec
{
    private const byte SevenBitMask = 0x7f;
    private const byte NullTerminator = 0;
    private const byte CarriageReturn = 0x0d;
    private const char SpacePadding = ' ';

    /// <summary>Décode un champ de nom ADFS.</summary>
    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        Span<byte> clean = stackalloc byte[bytes.Length];
        var length = 0;
        foreach (var value in bytes)
        {
            var character = (byte)(value & SevenBitMask);
            if (character is NullTerminator or CarriageReturn) break;
            clean[length++] = character;
        }
        return System.Text.Encoding.ASCII.GetString(clean[..length]).TrimEnd(SpacePadding);
    }
}
