namespace GWGUI.MediaEngine.FileSystems.Commodore;

/// <summary>Décode les caractères PETSCII utilisés par les répertoires Commodore.</summary>
public static class PetsciiDecoder
{
    /// <summary>Décode une séquence PETSCII en supprimant le remplissage final.</summary>
    /// <param name="bytes">Octets PETSCII à décoder.</param>
    /// <returns>Texte Unicode décodé.</returns>
    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        var chars = new List<char>(bytes.Length);
        foreach (var raw in bytes)
        {
            if (raw is 0 or 0xa0) break;
            var value = (byte)(raw & 0x7f);
            chars.Add(value switch
            {
                >= 0x20 and <= 0x5f => (char)value,
                >= 0x60 and <= 0x7a => (char)(value - 0x20),
                _ => '\ufffd'
            });
        }
        return new string(chars.ToArray()).Trim();
    }
}
