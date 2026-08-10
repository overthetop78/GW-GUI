using System.Buffers.Binary;
using GWGUI.MediaEngine.Recognition.Definitions;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class AtariStImageReader : ISectorImageReader
{
    public bool CanRead(string path) => Path.GetExtension(path).Equals(DiskImageFileExtensions.St, StringComparison.OrdinalIgnoreCase);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length == 0 || data.Length % 512 != 0) throw new InvalidDataException("The ST image size is not a multiple of 512 bytes.");
        var geometry = AtariStGeometry.Detect(data);
        return AtariStGeometry.CreateSectorImage(data, geometry);
    }
}

internal readonly record struct AtariStGeometry(int Cylinders, int Heads, int SectorsPerTrack)
{
    public string FormatId => $"atarist.{(Cylinders * Heads * SectorsPerTrack * 512) / 1024}";

    public static AtariStGeometry Detect(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 32)
        {
            var bytesPerSector = BinaryPrimitives.ReadUInt16LittleEndian(data[11..]);
            var totalSectors = BinaryPrimitives.ReadUInt16LittleEndian(data[19..]);
            if (totalSectors == 0) totalSectors = checked((ushort)Math.Min(ushort.MaxValue, BinaryPrimitives.ReadUInt32LittleEndian(data[32..])));
            var sectors = BinaryPrimitives.ReadUInt16LittleEndian(data[24..]);
            var heads = BinaryPrimitives.ReadUInt16LittleEndian(data[26..]);
            if (bytesPerSector == 512 && totalSectors == data.Length / 512 && sectors is > 0 and <= 36 && heads is > 0 and <= 2 && totalSectors % (sectors * heads) == 0)
                return new(totalSectors / (sectors * heads), heads, sectors);
        }
        var sectorCount = data.Length / 512;
        foreach (var sectors in new[] { 9, 10, 11, 18 })
            foreach (var heads in new[] { 2, 1 })
                if (sectorCount % (sectors * heads) == 0 && sectorCount / (sectors * heads) is >= 35 and <= 90)
                    return new(sectorCount / (sectors * heads), heads, sectors);
        throw new InvalidDataException("The ST image geometry could not be determined from its boot sector or size.");
    }

    public static SectorImage CreateSectorImage(ReadOnlySpan<byte> data, AtariStGeometry geometry)
    {
        var count = geometry.Cylinders * geometry.Heads * geometry.SectorsPerTrack;
        if (data.Length != count * 512) throw new InvalidDataException("The ST image length does not match its geometry.");
        var blocks = new SectorBlock[count];
        for (var logical = 0; logical < count; logical++)
        {
            var track = logical / geometry.SectorsPerTrack;
            blocks[logical] = new(logical, new(track / geometry.Heads, track % geometry.Heads, logical % geometry.SectorsPerTrack + 1), data.Slice(logical * 512, 512).ToArray());
        }
        return new(geometry.FormatId, 512, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, blocks);
    }
}
