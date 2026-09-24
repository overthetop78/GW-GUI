using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaFileSystems.FileSystems.Atari.SpartaDos;

internal static class SpartaDosFileReader
{
    public static SpartaDosFileData Read(IMediaSectorImage image, int firstMapSector, int length)
    {
        if (length < 0 || length > image.Capacity)
            throw SpartaDosFileSystemExceptions.InvalidDirectoryLength(firstMapSector, length);
        if (length == 0) return new([], 0, false);
        var expectedSectors = checked((length + image.BlockSize - 1) / image.BlockSize);
        var allocation = SpartaDosSectorMapReader.Read(image, firstMapSector, expectedSectors);
        var content = new List<byte>(expectedSectors * image.BlockSize);
        var allocatedDataSectors = 0;
        foreach (var sectorNumber in allocation.DataSectors)
        {
            if (sectorNumber == 0)
            {
                content.AddRange(new byte[image.BlockSize]);
                continue;
            }
            var sector = SpartaDosDiskReader.ReadSector(image, sectorNumber);
            if (sector.Length < image.BlockSize)
                throw SpartaDosFileSystemExceptions.MissingSector(sectorNumber);
            content.AddRange(sector.Take(image.BlockSize));
            allocatedDataSectors++;
        }
        if (content.Count > length) content.RemoveRange(length, content.Count - length);
        var occupiedSize = checked((long)(allocation.MapSectorCount + allocatedDataSectors) * image.BlockSize);
        return new(content.AsReadOnly(), occupiedSize, allocation.DataSectors.Contains(0));
    }
}
