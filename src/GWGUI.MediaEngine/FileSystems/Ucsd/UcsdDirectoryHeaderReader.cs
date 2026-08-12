using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Détecte l'ordre des octets et valide l'en-tête de répertoire UCSD.</summary>
internal static class UcsdDirectoryHeaderReader
{
    /// <summary>Tente de détecter l'ordre des octets depuis la fin de répertoire.</summary>
    public static bool TryDetectByteOrder(ReadOnlySpan<byte> directory, out UcsdByteOrder byteOrder)
    {
        byteOrder = default;
        if (directory.Length < UcsdFileSystemLayout.DirectoryEndOffset + sizeof(ushort)) return false;
        if (UcsdFileSystemLayout.IsDirectoryEnd(directory[UcsdFileSystemLayout.DirectoryEndOffset]) && directory[UcsdFileSystemLayout.DirectoryEndOffset + 1] == 0)
        {
            byteOrder = UcsdByteOrder.LittleEndian;
            return true;
        }
        if (UcsdFileSystemLayout.IsDirectoryEnd(directory[UcsdFileSystemLayout.DirectoryEndOffset + 1]) && directory[UcsdFileSystemLayout.DirectoryEndOffset] == 0)
        {
            byteOrder = UcsdByteOrder.BigEndian;
            return true;
        }
        return false;
    }

    /// <summary>Tente de lire un en-tête complet cohérent avec l'image.</summary>
    public static bool TryRead(SectorImage image, out UcsdDirectoryHeader? header)
    {
        header = null;
        var first = UcsdBlockReader.Read(image, UcsdFileSystemLayout.DirectoryBlock, 1);
        if (!first.IsValid || !TryDetectByteOrder(first.Bytes.ToArray(), out var byteOrder)) return false;
        var bytes = first.Bytes.ToArray();
        var endDirectory = UcsdPrimitives.ReadUInt16(bytes, UcsdFileSystemLayout.DirectoryEndOffset, byteOrder);
        var totalBlocks = UcsdPrimitives.ReadUInt16(bytes, UcsdFileSystemLayout.TotalBlocksOffset, byteOrder);
        var declaredFiles = UcsdPrimitives.ReadUInt16(bytes, UcsdFileSystemLayout.FileCountOffset, byteOrder);
        var volumeName = UcsdName.Decode(bytes.AsSpan(UcsdFileSystemLayout.VolumeNameOffset, UcsdFileSystemLayout.VolumeNameFieldLength), UcsdFileSystemLayout.MaximumVolumeNameLength);
        if (!UcsdFileSystemLayout.IsDirectoryEnd(endDirectory) || endDirectory > totalBlocks || totalBlocks > image.BlockCount || declaredFiles > UcsdFileSystemLayout.MaximumFileCount || volumeName.Length == 0) return false;
        header = new(endDirectory, volumeName, totalBlocks, declaredFiles, UcsdDate.Decode(UcsdPrimitives.ReadUInt16(bytes, UcsdFileSystemLayout.VolumeDateOffset, byteOrder)), byteOrder, UcsdFileSystemLayout.DirectoryBlockCount(endDirectory));
        return true;
    }
}
