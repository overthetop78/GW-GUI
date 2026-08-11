using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

using GWGUI.MediaEngine.Images;

namespace GWGUI.MediaEngine.Containers.Cp2;

/// <summary>Lit les secteurs capturés par SNATCH-IT pour Copy II PC.</summary>
public sealed class Cp2Reader
{
    /// <summary>Lit, valide et reconstruit une image sectorielle depuis un conteneur CP2.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        ValidateContainer(data);
        var sectors = ReadSectorBlocks(data, cancellationToken);
        return BuildImage(sectors, cancellationToken);
    }

    /// <summary>Valide la longueur minimale et la signature CP2.</summary>
    private static void ValidateContainer(ReadOnlySpan<byte> data)
    {
        if (data.Length < Cp2Layout.MinimumFileLength || !data.Slice(Cp2Layout.SignatureOffset, Cp2Format.SignatureLength).SequenceEqual(Cp2Format.Signature)) throw Cp2Exceptions.MissingSignature();
    }

    /// <summary>Calcule la géométrie observée et reconstruit les secteurs CP2 de 512 octets.</summary>
    private static SectorImage BuildImage(IReadOnlyDictionary<SectorAddress, byte[]> sectors, CancellationToken cancellationToken)
    {
        if (sectors.Count == 0) throw Cp2Exceptions.NoSectors();

        var cylinders = sectors.Keys.Max(address => address.Cylinder) + 1;
        var heads = sectors.Keys.Max(address => address.Head) + 1;
        var sectorsPerTrack = sectors.Keys.Max(address => address.Number);
        if (heads is <= 0 or > 2 || sectorsPerTrack <= 0)
            throw Cp2Exceptions.InvalidGeometry(heads, sectorsPerTrack);

        var linear = new byte[checked(cylinders * heads * sectorsPerTrack * Cp2Layout.ReconstructedSectorSize)];
        foreach (var (address, bytes) in sectors)
        {
            if (bytes.Length != Cp2Layout.ReconstructedSectorSize || address.Number is <= 0 || address.Number > sectorsPerTrack) continue;
            var logical = ((address.Cylinder * heads + address.Head) * sectorsPerTrack) + address.Number - 1;
            bytes.CopyTo(linear, logical * Cp2Layout.ReconstructedSectorSize);
        }
        return IbmPcImageReader.Create(linear, cancellationToken);
    }

    private static Dictionary<SectorAddress, byte[]> ReadSectorBlocks(byte[] data, CancellationToken cancellationToken)
    {
        var result = new Dictionary<SectorAddress, byte[]>();
        var groupOffset = Cp2Layout.FirstGroupOffset;
        while (groupOffset + Cp2Layout.GroupHeaderSize <= data.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(groupOffset + Cp2Layout.MetadataLengthOffset, Cp2Layout.LengthFieldSize));
            if (metadataLength == 0 || (metadataLength - Cp2Layout.MetadataLengthAdjustment) % Cp2Layout.TrackDescriptorSize != 0) throw Cp2Exceptions.InvalidDescriptionBlock(groupOffset, metadataLength, data.Length - groupOffset);

            var descriptorCount = (metadataLength - Cp2Layout.MetadataLengthAdjustment) / Cp2Layout.TrackDescriptorSize;
            var descriptors = new List<TrackDescriptor>(descriptorCount);
            for (var index = 0; index < descriptorCount; index++)
            {
                var offset = groupOffset + Cp2Layout.GroupHeaderSize + index * Cp2Layout.TrackDescriptorSize;
                if (offset + Cp2Layout.TrackDescriptorSize > data.Length) throw Cp2Exceptions.TruncatedDescriptionBlock(offset, Cp2Layout.TrackDescriptorSize, data.Length - offset);
                var descriptor = ParseTrackDescriptor(data.AsSpan(offset, Cp2Layout.TrackDescriptorSize));
                if (descriptor.Sectors.Count != 0) descriptors.Add(descriptor);
            }

            // Two bytes between the metadata and payload belong to the CP2 block
            // framing. Sector payloads then follow in physical (angular) order.
            var payloadOffset = checked(groupOffset + Cp2Layout.GroupHeaderSize + metadataLength + Cp2Layout.FramingSize);
            foreach (var track in descriptors)
            {
                foreach (var sector in track.Sectors.OrderBy(item => item.Position))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (payloadOffset > data.Length - sector.Size) throw Cp2Exceptions.TruncatedSectorData(new(sector.Cylinder, sector.Head, sector.Number), payloadOffset, sector.Size, data.Length - payloadOffset);
                    var bytes = data.AsSpan(payloadOffset, sector.Size).ToArray();
                    payloadOffset += sector.Size;
                    if (sector.Size == Cp2Layout.ReconstructedSectorSize)
                        result.TryAdd(new SectorAddress(sector.Cylinder, sector.Head, sector.Number), bytes);
                }
            }

            if (payloadOffset >= data.Length) break;
            // The first two bytes at the next group boundary close the preceding
            // payload; its metadata length follows immediately afterwards.
            groupOffset = payloadOffset - Cp2Layout.FramingSize;
        }
        return result;
    }

    private static TrackDescriptor ParseTrackDescriptor(ReadOnlySpan<byte> descriptor)
    {
        var count = descriptor[Cp2Layout.TrackSectorCountOffset];
        if (count == 0) return new([]);
        if (count > Cp2Layout.MaximumSectorDescriptorCount || Cp2Layout.TrackHeaderSize + count * Cp2Layout.SectorDescriptorSize > descriptor.Length) throw Cp2Exceptions.InvalidSectorDescriptorCount(count, Cp2Layout.MaximumSectorDescriptorCount);

        var sectors = new List<Cp2SectorDescriptor>(count);
        var trackCylinder = descriptor[Cp2Layout.TrackCylinderOffset];
        var trackHead = descriptor[Cp2Layout.TrackHeadOffset];
        for (var index = 0; index < count; index++)
        {
            var record = descriptor.Slice(Cp2Layout.TrackHeaderSize + index * Cp2Layout.SectorDescriptorSize, Cp2Layout.SectorDescriptorSize);
            var sizeCode = record[Cp2Layout.SectorSizeCodeOffset];
            // SNATCH-IT terminates some files with synthetic C/H=6 records.
            // They describe no stored payload and must not extend the geometry.
            if (record[Cp2Layout.SectorCylinderOffset] != trackCylinder || record[Cp2Layout.SectorHeadOffset] != trackHead || sizeCode > Cp2Layout.MaximumSectorSizeCode) continue;
            sectors.Add(new(record[Cp2Layout.SectorCylinderOffset], record[Cp2Layout.SectorHeadOffset], record[Cp2Layout.SectorNumberOffset], Cp2Layout.BaseSectorSize << sizeCode, BinaryPrimitives.ReadUInt16LittleEndian(record.Slice(Cp2Layout.SectorPositionOffset, Cp2Layout.SectorPositionLength))));
        }
        return new(sectors);
    }

    private sealed record TrackDescriptor(IReadOnlyList<Cp2SectorDescriptor> Sectors);
    private readonly record struct Cp2SectorDescriptor(int Cylinder, int Head, int Number, int Size, int Position);
}
