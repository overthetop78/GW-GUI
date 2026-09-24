using System.Buffers.Binary;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal static class SpartaDosDiskReader
{
    public static bool TryReadHeader(IMediaSectorImage image, out SpartaDosDiskHeader header)
    {
        header = null!;
        if (!TryReadSector(image, SpartaDosFileSystemLayout.BootSectorNumber, out var boot) ||
            boot.Length < SpartaDosFileSystemLayout.MinimumBootSectorLength ||
            boot[SpartaDosFileSystemLayout.IdentificationOffset] != SpartaDosFileSystemLayout.Identification)
            return false;

        var totalSectors = ReadUInt16(boot, SpartaDosFileSystemLayout.TotalSectorCountOffset);
        var rootMap = ReadUInt16(boot, SpartaDosFileSystemLayout.RootDirectoryMapOffset);
        var freeSectors = ReadUInt16(boot, SpartaDosFileSystemLayout.FreeSectorCountOffset);
        var bitmapCount = boot[SpartaDosFileSystemLayout.BitmapSectorCountOffset];
        var firstBitmap = ReadUInt16(boot, SpartaDosFileSystemLayout.FirstBitmapSectorOffset);
        var sectorSizeCode = boot[SpartaDosFileSystemLayout.SectorSizeOffset];
        var sectorSize = sectorSizeCode switch
        {
            SpartaDosFileSystemLayout.SingleDensitySectorSizeCode => SpartaDosFileSystemLayout.SingleDensitySectorSizeCode,
            SpartaDosFileSystemLayout.DoubleDensitySectorSizeCode => 256,
            _ => 0
        };
        var version = boot[SpartaDosFileSystemLayout.VersionOffset];
        if (totalSectors != image.BlockCount || totalSectors <= 0 || freeSectors > totalSectors ||
            rootMap is <= 0 || rootMap > totalSectors || bitmapCount <= 0 ||
            firstBitmap is <= 0 || firstBitmap + bitmapCount - 1 > totalSectors ||
            sectorSize != image.BlockSize || version is not (SpartaDosFileSystemLayout.Version1 or SpartaDosFileSystemLayout.Version2))
            return false;

        var rawTracks = boot[SpartaDosFileSystemLayout.TrackCountOffset];
        header = new(
            rootMap,
            totalSectors,
            freeSectors,
            bitmapCount,
            firstBitmap,
            ReadUInt16(boot, SpartaDosFileSystemLayout.FirstFileAllocationSectorOffset),
            ReadUInt16(boot, SpartaDosFileSystemLayout.FirstDirectoryAllocationSectorOffset),
            SpartaDosNameCodec.Decode(boot.AsSpan(SpartaDosFileSystemLayout.VolumeNameOffset, SpartaDosFileSystemLayout.VolumeNameLength)),
            rawTracks & 0x7f,
            (rawTracks & 0x80) != 0,
            sectorSize,
            version,
            boot[SpartaDosFileSystemLayout.VolumeSequenceOffset],
            boot[SpartaDosFileSystemLayout.VolumeRandomOffset],
            ReadUInt16(boot, SpartaDosFileSystemLayout.BootFileMapOffset),
            boot[SpartaDosFileSystemLayout.WriteLockOffset] == SpartaDosFileSystemLayout.WriteLocked);
        return true;
    }

    public static byte[] ReadSector(IMediaSectorImage image, int sectorNumber)
    {
        ValidateSectorNumber(sectorNumber, image.BlockCount);
        if (!image.TryGetBlock(sectorNumber - SpartaDosFileSystemLayout.FirstSectorNumber, out var block))
            throw SpartaDosFileSystemExceptions.MissingSector(sectorNumber);
        return block.Data.ToArray();
    }

    public static bool TryReadSector(IMediaSectorImage image, int sectorNumber, out byte[] data)
    {
        data = [];
        if (sectorNumber < SpartaDosFileSystemLayout.FirstSectorNumber || sectorNumber > image.BlockCount ||
            !image.TryGetBlock(sectorNumber - SpartaDosFileSystemLayout.FirstSectorNumber, out var block))
            return false;
        data = block.Data.ToArray();
        return true;
    }

    public static void ValidateSectorNumber(int sectorNumber, int totalSectors)
    {
        if (sectorNumber < SpartaDosFileSystemLayout.FirstSectorNumber || sectorNumber > totalSectors)
            throw SpartaDosFileSystemExceptions.InvalidSectorReference(sectorNumber, totalSectors);
    }

    private static int ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, sizeof(ushort)));
}
