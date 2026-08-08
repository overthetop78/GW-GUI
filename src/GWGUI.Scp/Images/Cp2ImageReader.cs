using System.Buffers.Binary;
using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

/// <summary>Reads sector data captured by SNATCH-IT for Copy II PC.</summary>
public sealed class Cp2ImageReader
{
    private const int TrackDescriptorSize = 387;
    private const int SectorDescriptorSize = 16;
    private const int TrackHeaderSize = 7;

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        if (data.Length < 34 || !data.AsSpan(0, 16).SequenceEqual("SOFTWARE PIRATES"u8))
            throw new InvalidDataException("The file does not contain a SNATCH-IT CP2 signature.");

        var sectors = ReadSectorBlocks(data, cancellationToken);
        if (sectors.Count == 0) throw new InvalidDataException("The CP2 image contains no readable sectors.");

        var cylinders = sectors.Keys.Max(address => address.Cylinder) + 1;
        var heads = sectors.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = sectors.Keys.Max(address => address.Number);
        if (heads is <= 0 or > 2 || sectorsPerTrack <= 0)
            throw new InvalidDataException("The CP2 image geometry is invalid.");

        var linear = new byte[checked(cylinders * heads * sectorsPerTrack * 512)];
        foreach (var (address, bytes) in sectors)
        {
            if (bytes.Length != 512 || address.Number is <= 0 || address.Number > sectorsPerTrack) continue;
            var logical = ((address.Cylinder * heads + address.Head) * sectorsPerTrack) + address.Number - 1;
            bytes.CopyTo(linear, logical * 512);
        }
        return IbmPcImageReader.Create(linear, cancellationToken);
    }

    private static Dictionary<SectorAddress, byte[]> ReadSectorBlocks(byte[] data, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, byte[]>();
        var groupOffset = 28;
        while (groupOffset + 4 <= data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(groupOffset + 2, 2));
            if (metadataLength == 0 || (metadataLength - 1) % TrackDescriptorSize != 0)
                throw new InvalidDataException("The CP2 track-description block is invalid.");

            var descriptorCount = (metadataLength - 1) / TrackDescriptorSize;
            var descriptors = new List<TrackDescriptor>(descriptorCount);
            for (var index = 0; index < descriptorCount; index++)
            {
                var offset = groupOffset + 4 + index * TrackDescriptorSize;
                if (offset + TrackDescriptorSize > data.Length)
                    throw new InvalidDataException("The CP2 track-description block is truncated.");
                var descriptor = ParseTrackDescriptor(data.AsSpan(offset, TrackDescriptorSize));
                if (descriptor.Sectors.Count != 0) descriptors.Add(descriptor);
            }

            // Two bytes between the metadata and payload belong to the CP2 block
            // framing. Sector payloads then follow in physical (angular) order.
            var payloadOffset = checked(groupOffset + 4 + metadataLength + 2);
            foreach (var track in descriptors)
            {
                foreach (var sector in track.Sectors.OrderBy(item => item.Position))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (payloadOffset + sector.Size > data.Length)
                        throw new InvalidDataException("The CP2 sector-data block is truncated.");
                    var bytes = data.AsSpan(payloadOffset, sector.Size).ToArray();
                    payloadOffset += sector.Size;
                    if (sector.Size == 512)
                        result.TryAdd(new SectorAddress(sector.Cylinder, sector.Head, sector.Number), bytes);
                }
            }

            if (payloadOffset >= data.Length) break;
            // The first two bytes at the next group boundary close the preceding
            // payload; its metadata length follows immediately afterwards.
            groupOffset = payloadOffset - 2;
        }
        return result;
    }

    private static TrackDescriptor ParseTrackDescriptor(ReadOnlySpan<byte> descriptor)
    {
        var count = descriptor[2];
        if (count == 0) return new([]);
        if (count > 23 || TrackHeaderSize + count * SectorDescriptorSize > descriptor.Length)
            throw new InvalidDataException("The CP2 sector-description count is invalid.");

        var sectors = new List<Cp2SectorDescriptor>(count);
        var trackCylinder = descriptor[0];
        var trackHead = descriptor[1];
        for (var index = 0; index < count; index++)
        {
            var record = descriptor.Slice(TrackHeaderSize + index * SectorDescriptorSize, SectorDescriptorSize);
            var sizeCode = record[3];
            // SNATCH-IT terminates some files with synthetic C/H=6 records.
            // They describe no stored payload and must not extend the geometry.
            if (record[0] != trackCylinder || record[1] != trackHead || sizeCode > 7) continue;
            sectors.Add(new(record[0], record[1], record[2], 128 << sizeCode,
                BinaryPrimitives.ReadUInt16LittleEndian(record[5..7])));
        }
        return new(sectors);
    }

    private sealed record TrackDescriptor(IReadOnlyList<Cp2SectorDescriptor> Sectors);
    private readonly record struct Cp2SectorDescriptor(int Cylinder, int Head, int Number, int Size, int Position);
}
