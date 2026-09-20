
using IMediaSectorImage = global::GWGUI.MediaFileSystems.Interfaces.IMediaSectorImage;

namespace GWGUI.MediaFileSystems.FileSystems.Apple.ProDos;

/// <summary>Valide et décode l'en-tête du volume ProDOS.</summary>
public static class ProDosVolumeHeaderReader
{
    /// <summary>Vérifie si un bloc racine déjà chargé contient un en-tête ProDOS valide.</summary>
    public static bool IsValid(ReadOnlySpan<byte> root) => TryRead(root, out _);

    /// <summary>Tente de lire l'en-tête complet du bloc racine.</summary>
    internal static bool TryRead(IMediaSectorImage image, out ProDosVolumeHeaderInfo? header)
    {
        header = null;
        if (image.BlockSize != ProDosFileSystemLayout.BlockSize || !image.TryGetBlock(ProDosFileSystemLayout.RootBlock, out var root) || root.Data.Count != ProDosFileSystemLayout.BlockSize) return false;
        return TryRead(root.Data.ToArray(), out header);
    }

    /// <summary>Tente de valider et décoder un bloc racine déjà chargé.</summary>
    internal static bool TryRead(ReadOnlySpan<byte> root, out ProDosVolumeHeaderInfo? header)
    {
        header = null;
        if (root.Length != ProDosFileSystemLayout.BlockSize) return false;
        var storageAndLength = root[ProDosFileSystemLayout.HeaderOffset];
        var storage = (ProDosStorageType)(storageAndLength >> ProDosFileSystemLayout.StorageTypeShift);
        var nameLength = storageAndLength & ProDosFileSystemLayout.NameLengthMask;
        if (storage != ProDosStorageType.VolumeHeader || nameLength is 0 or > ProDosFileSystemLayout.MaximumNameLength || root[ProDosFileSystemLayout.HeaderEntryLengthOffset] != ProDosFileSystemLayout.EntrySize) return false;
        header = new(ProDosPrimitives.ReadName(root, ProDosFileSystemLayout.HeaderOffset), ProDosPrimitives.ReadUInt16(root, ProDosFileSystemLayout.BitmapBlockOffset), ProDosPrimitives.ReadUInt16(root, ProDosFileSystemLayout.TotalBlocksOffset), ProDosDateTime.Read(root, ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.CreatedDateOffset), Array.AsReadOnly(root.ToArray()));
        return true;
    }
}
