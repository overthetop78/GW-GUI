using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Valide le VTOC Apple DOS et compte ses secteurs libres.</summary>
public static class AppleDosVtocReader
{
    /// <summary>Tente de lire le VTOC correspondant à la géométrie réelle.</summary>
    public static bool TryRead(SectorImage image, out AppleDosVtoc? vtoc)
    {
        vtoc = null;
        var sectors = image.SectorsPerTrack;
        if (image.BlockSize != AppleDosFileSystemLayout.SectorSize || sectors is not (AppleDosFileSystemLayout.Dos32SectorsPerTrack or AppleDosFileSystemLayout.Dos33SectorsPerTrack) || image.Cylinders < AppleDosFileSystemLayout.TrackCount) return false;
        if (!image.TryGetBlock(AppleDosFileSystemLayout.VtocTrack * sectors, out var block) || block.Data.Count != AppleDosFileSystemLayout.SectorSize) return false;
        var data = block.Data.ToArray();
        var tracks = data[AppleDosFileSystemLayout.VtocTrackCountOffset];
        var declaredSectors = data[AppleDosFileSystemLayout.VtocSectorsPerTrackOffset];
        var declaredSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(AppleDosFileSystemLayout.VtocSectorSizeOffset));
        var catalogTrack = data[AppleDosFileSystemLayout.VtocCatalogTrackOffset];
        var catalogSector = data[AppleDosFileSystemLayout.VtocCatalogSectorOffset];
        if (tracks <= 0 || tracks > image.Cylinders || declaredSectors != sectors || declaredSize != AppleDosFileSystemLayout.SectorSize || !AppleDosFileSystemLayout.IsValidAddress(catalogTrack, catalogSector, tracks, sectors) || catalogTrack == 0) return false;
        vtoc = new(data, tracks, sectors, catalogTrack, catalogSector, data[AppleDosFileSystemLayout.VtocVolumeNumberOffset], CountFree(data, tracks, sectors));
        return true;
    }

    /// <summary>Compte les bits libres du bitmap big-endian limité à la géométrie validée.</summary>
    private static int CountFree(ReadOnlySpan<byte> data, int tracks, int sectors)
    {
        var free = 0;
        for (var track = 0; track < tracks; track++)
        {
            var offset = AppleDosFileSystemLayout.VtocFreeBitmapOffset + track * AppleDosFileSystemLayout.VtocTrackBitmapSize;
            if (offset > data.Length - AppleDosFileSystemLayout.VtocTrackBitmapSize) break;
            var bits = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(offset, AppleDosFileSystemLayout.VtocTrackBitmapSize));
            for (var sector = 0; sector < sectors; sector++) if ((bits & (1u << (sizeof(uint) * 8 - sectors + sector))) != 0) free++;
        }
        return free;
    }
}
