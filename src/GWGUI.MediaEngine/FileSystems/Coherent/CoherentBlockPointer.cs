namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Décode les pointeurs 24 bits dans l'ordre particulier de COHERENT.</summary>
public static class CoherentBlockPointer
{
    /// <summary>Lit un pointeur borné de trois octets.</summary>
    public static int Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < CoherentFileSystemLayout.InodePointerSize) throw new ArgumentOutOfRangeException(nameof(data));
        return data[1] | data[2] << 8 | data[0] << 16;
    }
}
