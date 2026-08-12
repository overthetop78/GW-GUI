namespace GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;

/// <summary>Décode les champs compactés sur 18 ou 10 bits d'une entrée BBC DFS.</summary>
public static class BbcDfsEntryDecoder
{
    /// <summary>Décode la longueur sur 18 bits.</summary>
    public static int Length(ReadOnlySpan<byte> data, int offset) => data[offset + BbcDfsFileSystemLayout.LengthOffset] | data[offset + BbcDfsFileSystemLayout.LengthOffset + 1] << 8 | (data[offset + BbcDfsFileSystemLayout.PackedOffset] & BbcDfsFileSystemLayout.LengthHighMask) << BbcDfsFileSystemLayout.LengthHighShift;
    /// <summary>Décode le secteur initial sur 10 bits.</summary>
    public static int StartSector(ReadOnlySpan<byte> data, int offset) => data[offset + BbcDfsFileSystemLayout.StartSectorOffset] | (data[offset + BbcDfsFileSystemLayout.PackedOffset] & BbcDfsFileSystemLayout.StartSectorHighMask) << 8;
    /// <summary>Décode l'adresse load sur 18 bits.</summary>
    public static int Load(ReadOnlySpan<byte> data, int offset) => data[offset] | data[offset + 1] << 8 | (data[offset + BbcDfsFileSystemLayout.PackedOffset] & BbcDfsFileSystemLayout.LoadHighMask) << BbcDfsFileSystemLayout.LoadHighShift;
    /// <summary>Décode l'adresse execute sur 18 bits.</summary>
    public static int Execute(ReadOnlySpan<byte> data, int offset) => data[offset + 2] | data[offset + 3] << 8 | (data[offset + BbcDfsFileSystemLayout.PackedOffset] & BbcDfsFileSystemLayout.ExecuteHighMask) << BbcDfsFileSystemLayout.ExecuteHighShift;
}
