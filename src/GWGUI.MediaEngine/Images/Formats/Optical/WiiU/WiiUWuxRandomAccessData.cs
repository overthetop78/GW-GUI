using System.IO;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Optical.WiiU;

/// <summary>Provides bounded logical reads from an indexed WUX image.</summary>
internal sealed class WiiUWuxRandomAccessData : IMediaRandomAccessData
{
    private readonly WiiUWuxHeader header;
    private readonly uint[] indexTable;

    public WiiUWuxRandomAccessData(string path, WiiUWuxHeader header, uint[] indexTable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(indexTable);
        if (indexTable.Length != header.EntryCount)
            throw new ArgumentException("The WUX index table length does not match its header.", nameof(indexTable));

        Path = System.IO.Path.GetFullPath(path);
        if (new FileInfo(Path).Length < header.SectorArrayOffset)
            throw new InvalidDataException("The WUX sector array is outside the source file.");
        foreach (var storedSector in indexTable)
            if (storedSector >= header.StoredSectorCount)
                throw new InvalidDataException("The WUX index table references a missing stored sector.");

        this.header = header;
        this.indexTable = indexTable;
        Length = header.UncompressedSize;
    }

    public string Path { get; }

    public long Length { get; }

    public async ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > Length || destination.Length > Length - offset)
            throw new ArgumentOutOfRangeException(nameof(destination), "The requested range exceeds the WUX logical data.");
        if (destination.IsEmpty)
            return;

        await using var stream = new FileStream(
            Path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.RandomAccess);
        var logicalOffset = offset;
        var destinationOffset = 0;
        while (destinationOffset < destination.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectorOffset = (int)(logicalOffset % header.SectorSize);
            var sectorIndex = checked((int)(logicalOffset / header.SectorSize));
            var count = Math.Min(
                destination.Length - destinationOffset,
                header.SectorSize - sectorOffset);
            var storedSector = indexTable[sectorIndex];
            var storedOffset = checked(header.SectorArrayOffset
                + (long)storedSector * header.SectorSize
                + sectorOffset);
            stream.Seek(storedOffset, SeekOrigin.Begin);
            await ReadExactlyFromStreamAsync(
                stream,
                destination.Slice(destinationOffset, count),
                storedOffset,
                cancellationToken).ConfigureAwait(false);
            logicalOffset += count;
            destinationOffset += count;
        }
    }

    private static async Task ReadExactlyFromStreamAsync(
        FileStream stream,
        Memory<byte> destination,
        long offset,
        CancellationToken cancellationToken)
    {
        var totalRead = 0;
        while (totalRead < destination.Length)
        {
            var read = await stream.ReadAsync(destination[totalRead..], cancellationToken).ConfigureAwait(false);
            if (read == 0)
                throw new EndOfStreamException($"The WUX source ended at byte {offset + totalRead}.");
            totalRead += read;
        }
    }
}
