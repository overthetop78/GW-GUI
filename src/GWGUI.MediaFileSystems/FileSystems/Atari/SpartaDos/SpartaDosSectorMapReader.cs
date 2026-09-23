using System.Buffers.Binary;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal static class SpartaDosSectorMapReader
{
    public static SpartaDosSectorAllocation Read(IMediaSectorImage image, int firstMapSector, int expectedDataSectorCount)
    {
        if (expectedDataSectorCount <= 0) return new([], 0);
        SpartaDosDiskReader.ValidateSectorNumber(firstMapSector, image.BlockCount);
        var dataSectors = new List<int>(expectedDataSectorCount);
        var visited = new HashSet<int>();
        var current = firstMapSector;
        var previous = 0;
        while (current != 0 && dataSectors.Count < expectedDataSectorCount)
        {
            if (!visited.Add(current)) throw SpartaDosFileSystemExceptions.SectorMapLoop(current);
            var map = SpartaDosDiskReader.ReadSector(image, current);
            if (map.Length < SpartaDosFileSystemLayout.SectorMapDataOffset)
                throw SpartaDosFileSystemExceptions.MissingSector(current);
            var observedPrevious = BinaryPrimitives.ReadUInt16LittleEndian(map.AsSpan(SpartaDosFileSystemLayout.SectorMapPreviousOffset, sizeof(ushort)));
            if (observedPrevious != previous)
                throw SpartaDosFileSystemExceptions.SectorMapBackLink(current, previous, observedPrevious);
            for (var offset = SpartaDosFileSystemLayout.SectorMapDataOffset;
                 offset + SpartaDosFileSystemLayout.SectorNumberSize <= map.Length && dataSectors.Count < expectedDataSectorCount;
                 offset += SpartaDosFileSystemLayout.SectorNumberSize)
            {
                var sector = BinaryPrimitives.ReadUInt16LittleEndian(map.AsSpan(offset, sizeof(ushort)));
                if (sector != 0) SpartaDosDiskReader.ValidateSectorNumber(sector, image.BlockCount);
                dataSectors.Add(sector);
            }
            previous = current;
            current = BinaryPrimitives.ReadUInt16LittleEndian(map.AsSpan(SpartaDosFileSystemLayout.SectorMapNextOffset, sizeof(ushort)));
            if (current != 0) SpartaDosDiskReader.ValidateSectorNumber(current, image.BlockCount);
        }
        if (dataSectors.Count < expectedDataSectorCount)
            throw SpartaDosFileSystemExceptions.MissingSector(0);
        return new(dataSectors.AsReadOnly(), visited.Count);
    }

    public static int ReadFirstDataSector(IMediaSectorImage image, int firstMapSector)
    {
        var allocation = Read(image, firstMapSector, 1);
        var sector = allocation.DataSectors[0];
        if (sector == 0) throw SpartaDosFileSystemExceptions.InvalidSectorReference(sector, image.BlockCount);
        return sector;
    }
}
