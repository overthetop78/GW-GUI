namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Normalise un secteur physique vers la taille de bloc logique déclarée par son système de fichiers.</summary>
internal static class IsoSectorDataNormalizer
{
    /// <summary>Complète un secteur court avec des zéros et refuse de tronquer un secteur trop long.</summary>
    public static byte[] PadTo(IReadOnlyList<byte> data, int size)
    {
        if (data.Count > size) return [];
        var normalized = new byte[size];
        for (var index = 0; index < data.Count; index++) normalized[index] = data[index];
        return normalized;
    }
}
