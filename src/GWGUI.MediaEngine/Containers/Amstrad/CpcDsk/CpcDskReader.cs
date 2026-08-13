using System.Buffers.Binary;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>Lit les conteneurs CPCEMU DSK standard et étendu sans leur attribuer une machine CPC ou PCW.</summary>
public sealed class CpcDskReader
{
    /// <summary>Lit le conteneur et retourne son image sectorielle neutre.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => (await ReadDetailedAsync(path, cancellationToken).ConfigureAwait(false)).SectorImage;

    /// <summary>Lit en mémoire le conteneur et retourne son image sectorielle neutre.</summary>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(ReadDetailed(bytes.Span, cancellationToken).SectorImage);

    /// <summary>Lit le conteneur en conservant ses en-têtes, ses descripteurs et ses données stockées.</summary>
    public async Task<CpcDskImage> ReadDetailedAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        return ReadDetailed(bytes, cancellationToken);
    }

    /// <summary>Lit en mémoire le conteneur en conservant toutes les informations réinscriptibles.</summary>
    public Task<CpcDskImage> ReadDetailedAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(ReadDetailed(bytes.Span, cancellationToken));

    private static CpcDskImage ReadDetailed(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        var (kind, cylinders, heads, trackCount) = ReadDiskHeader(bytes);
        ValidateExtendedTrackSizeTable(kind, trackCount);
        var blocks = new List<SectorBlock>();
        var tracks = new List<CpcDskTrack>(trackCount);
        var sectorSizes = new Dictionary<int, int>();
        var maximumSectors = 0;
        var position = CpcDskLayout.DiskInformationBlockSize;
        for (var trackIndex = 0; trackIndex < trackCount; trackIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = ReadTrack(bytes, kind, trackIndex, position, blocks, sectorSizes, heads);
            tracks.Add(result.Track);
            position = result.NextPosition;
            maximumSectors = Math.Max(maximumSectors, result.Track.Sectors.Count);
        }
        if (blocks.Count == 0) throw CpcDskExceptions.NoSectors();
        var dominantSize = sectorSizes.OrderByDescending(item => item.Value).First().Key;
        var image = new SectorImage(CpcDskFormat.FormatId, dominantSize, cylinders, heads, Math.Max(CpcDskLayout.MinimumSectorsPerTrack, maximumSectors), blocks, sectorSizes.Count > 1, blocks.Sum(block => (long)block.Data.Count), blocks.Count);
        return new(kind, checked((byte)cylinders), checked((byte)heads), tracks, image);
    }

    private static (CpcDskContainerKind Kind, int Cylinders, int Heads, int TrackCount) ReadDiskHeader(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < CpcDskLayout.DiskInformationBlockSize) throw CpcDskExceptions.TruncatedHeader();
        var signature = bytes[..CpcDskLayout.DiskSignatureLength];
        var kind = signature.StartsWith(CpcDskFormat.ExtendedSignatureBytes) ? CpcDskContainerKind.Extended : CpcDskContainerKind.Standard;
        if (kind == CpcDskContainerKind.Standard && !signature.StartsWith(CpcDskFormat.StandardSignatureBytes)) throw CpcDskExceptions.UnrecognizedSignature();
        var cylinders = bytes[CpcDskLayout.CylinderCountOffset];
        var heads = bytes[CpcDskLayout.HeadCountOffset];
        if (cylinders is < CpcDskLayout.MinimumCylinderCount or > CpcDskLayout.MaximumCylinderCount || heads is < CpcDskLayout.MinimumHeadCount or > CpcDskLayout.MaximumHeadCount) throw CpcDskExceptions.InvalidGeometry();
        return (kind, cylinders, heads, checked(cylinders * heads));
    }

    private static void ValidateExtendedTrackSizeTable(CpcDskContainerKind kind, int trackCount)
    {
        if (kind != CpcDskContainerKind.Extended || CpcDskLayout.ExtendedTrackSizeTableOffset + trackCount <= CpcDskLayout.DiskInformationBlockSize) return;
        throw CpcDskExceptions.InvalidExtendedTrackTable(CpcDskLayout.DiskInformationBlockSize - CpcDskLayout.ExtendedTrackSizeTableOffset);
    }

    private static (int NextPosition, CpcDskTrack Track) ReadTrack(ReadOnlySpan<byte> bytes, CpcDskContainerKind kind, int trackIndex, int position, List<SectorBlock> blocks, Dictionary<int, int> sectorSizes, int heads)
    {
        var trackSize = kind == CpcDskContainerKind.Extended ? bytes[CpcDskLayout.ExtendedTrackSizeTableOffset + trackIndex] * CpcDskLayout.ExtendedTrackSizeUnit : BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(CpcDskLayout.StandardTrackSizeOffset, CpcDskLayout.StoredSizeFieldLength));
        var expectedCylinder = checked((byte)(trackIndex / heads));
        var expectedHead = checked((byte)(trackIndex % heads));
        if (trackSize == 0) return (position, new(trackIndex, false, expectedCylinder, expectedHead, 0, 0, 0, []));
        if (position + trackSize > bytes.Length || trackSize < CpcDskLayout.TrackInformationBlockSize) throw CpcDskExceptions.TruncatedTrack(trackIndex);
        if (!bytes.Slice(position, CpcDskLayout.TrackSignatureLength).StartsWith(CpcDskFormat.TrackSignatureBytes)) throw CpcDskExceptions.InvalidTrackHeader(trackIndex);
        var cylinder = bytes[position + CpcDskLayout.TrackCylinderOffset];
        var head = bytes[position + CpcDskLayout.TrackHeadOffset];
        var sizeCode = bytes[position + CpcDskLayout.TrackSectorSizeCodeOffset];
        var sectorCount = bytes[position + CpcDskLayout.TrackSectorCountOffset];
        var gap3 = bytes[position + CpcDskLayout.TrackGap3LengthOffset];
        var filler = bytes[position + CpcDskLayout.TrackFillerByteOffset];
        var sectors = ReadSectors(bytes, kind, trackIndex, position, trackSize, blocks, sectorSizes, cylinder, head, sectorCount);
        return (position + trackSize, new(trackIndex, true, cylinder, head, sizeCode, gap3, filler, sectors));
    }

    private static IReadOnlyList<CpcDskSector> ReadSectors(ReadOnlySpan<byte> bytes, CpcDskContainerKind kind, int trackIndex, int position, int trackSize, List<SectorBlock> blocks, Dictionary<int, int> sectorSizes, byte cylinder, byte head, int sectorCount)
    {
        if (position + CpcDskLayout.SectorDescriptorTableOffset + sectorCount * CpcDskLayout.SectorDescriptorSize > position + CpcDskLayout.TrackInformationBlockSize) throw CpcDskExceptions.InvalidSectorTable(trackIndex);
        var sectors = new List<CpcDskSector>(sectorCount);
        var dataPosition = position + CpcDskLayout.TrackInformationBlockSize;
        for (var sectorIndex = 0; sectorIndex < sectorCount; sectorIndex++)
        {
            var descriptor = position + CpcDskLayout.SectorDescriptorTableOffset + sectorIndex * CpcDskLayout.SectorDescriptorSize;
            var sectorCylinder = bytes[descriptor + CpcDskLayout.SectorCylinderOffset];
            var sectorHead = bytes[descriptor + CpcDskLayout.SectorHeadOffset];
            var sectorId = bytes[descriptor + CpcDskLayout.SectorIdOffset];
            var sectorSizeCode = checked((byte)(bytes[descriptor + CpcDskLayout.SectorSizeCodeOffset] & CpcDskLayout.SectorSizeCodeMask));
            var nominalSize = CpcDskLayout.MinimumSectorSize << sectorSizeCode;
            var storedSize = kind == CpcDskContainerKind.Extended ? BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(descriptor + CpcDskLayout.SectorStoredSizeOffset, CpcDskLayout.StoredSizeFieldLength)) : nominalSize;
            if (storedSize == 0) storedSize = nominalSize;
            if (dataPosition + storedSize > position + trackSize) throw CpcDskExceptions.TruncatedSector(cylinder, head, sectorId);
            var status1 = bytes[descriptor + CpcDskLayout.SectorStatus1Offset];
            var status2 = bytes[descriptor + CpcDskLayout.SectorStatus2Offset];
            var storedData = bytes.Slice(dataPosition, storedSize).ToArray();
            var sectorData = storedData.AsSpan(0, Math.Min(nominalSize, storedSize)).ToArray();
            var integrityValid = (status1 & CpcDskLayout.DataErrorMask) == 0 && (status2 & CpcDskLayout.DataErrorMask) == 0;
            sectors.Add(new(sectorCylinder, sectorHead, sectorId, sectorSizeCode, status1, status2, storedData));
            blocks.Add(new(blocks.Count, new(sectorCylinder, sectorHead, sectorId), sectorData, integrityValid, FormatCode: sectorSizeCode));
            sectorSizes[nominalSize] = sectorSizes.GetValueOrDefault(nominalSize) + 1;
            dataPosition += storedSize;
        }
        return sectors;
    }
}
