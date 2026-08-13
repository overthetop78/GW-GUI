using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>Écrit les conteneurs CPCEMU DSK standard et EDSK en conservant leurs descripteurs.</summary>
public sealed class CpcDskWriter
{
    /// <summary>Valide puis écrit atomiquement le modèle de conteneur fourni.</summary>
    public async Task WriteAsync(CpcDskImage image, string path, CancellationToken cancellationToken = default)
    {
        var bytes = Build(image);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private static byte[] Build(CpcDskImage image)
    {
        var trackCount = checked(image.Cylinders * image.Heads);
        if (image.Cylinders is < CpcDskLayout.MinimumCylinderCount or > CpcDskLayout.MaximumCylinderCount || image.Heads is < CpcDskLayout.MinimumHeadCount or > CpcDskLayout.MaximumHeadCount || image.Tracks.Count != trackCount) throw CpcDskExceptions.InvalidContainer("the declared geometry and track collection disagree");
        var serializedTracks = image.Tracks.Select((track, index) => SerializeTrack(track, index, image.Kind)).ToArray();
        if (image.Kind == CpcDskContainerKind.Standard) ValidateStandardTracks(serializedTracks);
        var header = new byte[CpcDskLayout.DiskInformationBlockSize];
        (image.Kind == CpcDskContainerKind.Extended ? CpcDskFormat.ExtendedHeaderBytes : CpcDskFormat.StandardHeaderBytes).CopyTo(header);
        System.Text.Encoding.ASCII.GetBytes(CpcDskFormat.Creator).CopyTo(header, CpcDskLayout.CreatorOffset);
        header[CpcDskLayout.CylinderCountOffset] = image.Cylinders;
        header[CpcDskLayout.HeadCountOffset] = image.Heads;
        if (image.Kind == CpcDskContainerKind.Standard)
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(CpcDskLayout.StandardTrackSizeOffset), checked((ushort)serializedTracks[0].Length));
        else
            for (var index = 0; index < serializedTracks.Length; index++) header[CpcDskLayout.ExtendedTrackSizeTableOffset + index] = checked((byte)(serializedTracks[index].Length / CpcDskLayout.ExtendedTrackSizeUnit));
        var output = new byte[header.Length + serializedTracks.Sum(track => track.Length)];
        header.CopyTo(output, 0);
        var position = header.Length;
        foreach (var track in serializedTracks)
        {
            track.CopyTo(output, position);
            position += track.Length;
        }
        return output;
    }

    private static byte[] SerializeTrack(CpcDskTrack track, int expectedIndex, CpcDskContainerKind kind)
    {
        if (track.Index != expectedIndex) throw CpcDskExceptions.InvalidContainer($"track index {track.Index} appears at position {expectedIndex}");
        if (!track.IsPresent)
        {
            if (kind == CpcDskContainerKind.Standard) throw CpcDskExceptions.StandardTrackNotRepresentable(track.Index);
            return [];
        }
        if (track.Sectors.Count > CpcDskLayout.MaximumSectorsPerTrack) throw CpcDskExceptions.InvalidContainer($"track {track.Index} contains too many sector descriptors");
        var unpaddedSize = checked(CpcDskLayout.TrackInformationBlockSize + track.Sectors.Sum(sector => sector.Data.Count));
        var trackSize = kind == CpcDskContainerKind.Extended ? RoundToExtendedUnit(unpaddedSize) : unpaddedSize;
        if (kind == CpcDskContainerKind.Extended && trackSize / CpcDskLayout.ExtendedTrackSizeUnit > byte.MaxValue) throw CpcDskExceptions.ExtendedTrackTooLarge(track.Index, trackSize);
        var bytes = new byte[trackSize];
        CpcDskFormat.TrackHeaderBytes.CopyTo(bytes);
        bytes[CpcDskLayout.TrackCylinderOffset] = track.Cylinder;
        bytes[CpcDskLayout.TrackHeadOffset] = track.Head;
        bytes[CpcDskLayout.TrackSectorSizeCodeOffset] = track.SectorSizeCode;
        bytes[CpcDskLayout.TrackSectorCountOffset] = checked((byte)track.Sectors.Count);
        bytes[CpcDskLayout.TrackGap3LengthOffset] = track.Gap3Length;
        bytes[CpcDskLayout.TrackFillerByteOffset] = track.FillerByte;
        var dataPosition = CpcDskLayout.TrackInformationBlockSize;
        for (var sectorIndex = 0; sectorIndex < track.Sectors.Count; sectorIndex++)
        {
            var sector = track.Sectors[sectorIndex];
            var nominalSize = CpcDskLayout.MinimumSectorSize << (sector.SizeCode & CpcDskLayout.SectorSizeCodeMask);
            if (kind == CpcDskContainerKind.Standard && sector.Data.Count != nominalSize) throw CpcDskExceptions.StandardTrackNotRepresentable(track.Index);
            var descriptor = CpcDskLayout.SectorDescriptorTableOffset + sectorIndex * CpcDskLayout.SectorDescriptorSize;
            bytes[descriptor + CpcDskLayout.SectorCylinderOffset] = sector.Cylinder;
            bytes[descriptor + CpcDskLayout.SectorHeadOffset] = sector.Head;
            bytes[descriptor + CpcDskLayout.SectorIdOffset] = sector.Id;
            bytes[descriptor + CpcDskLayout.SectorSizeCodeOffset] = sector.SizeCode;
            bytes[descriptor + CpcDskLayout.SectorStatus1Offset] = sector.Status1;
            bytes[descriptor + CpcDskLayout.SectorStatus2Offset] = sector.Status2;
            if (kind == CpcDskContainerKind.Extended) BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(descriptor + CpcDskLayout.SectorStoredSizeOffset), checked((ushort)sector.Data.Count));
            sector.Data.ToArray().CopyTo(bytes, dataPosition);
            dataPosition += sector.Data.Count;
        }
        return bytes;
    }

    private static void ValidateStandardTracks(IReadOnlyList<byte[]> tracks)
    {
        var size = tracks[0].Length;
        if (size > ushort.MaxValue || tracks.Any(track => track.Length != size)) throw CpcDskExceptions.InvalidContainer("standard DSK requires one uniform track size");
    }

    private static int RoundToExtendedUnit(int value) => checked((value + CpcDskLayout.ExtendedTrackSizeUnit - 1) / CpcDskLayout.ExtendedTrackSizeUnit * CpcDskLayout.ExtendedTrackSizeUnit);
}
