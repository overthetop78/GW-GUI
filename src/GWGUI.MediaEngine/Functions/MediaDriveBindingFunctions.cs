using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Associe les accès des représentations aux adresses du média, indépendamment du conteneur.</summary>
internal static class MediaDriveBindingFunctions
{
    internal static IMediaImageRepresentation Bind(IMediaDriveMechanics? drive, IMediaImageRepresentation representation, bool writing)
    {
        if (drive is null) return representation;
        return representation switch
        {
            SectorMediaImageRepresentation sectors => BindSectors(drive, sectors, writing),
            BlockMediaImageRepresentation blocks => BindBlocks(drive, blocks, writing),
            OpticalMediaImageRepresentation optical when optical.Tracks is not null => BindOptical(drive, optical, writing),
            _ => representation
        };
    }

    private static SectorMediaImageRepresentation BindSectors(IMediaDriveMechanics drive, SectorMediaImageRepresentation representation, bool writing)
    {
        var image = representation.Image;
        if (image.AvailableBlocks.All(block => block.Data is MediaDriveSectorData data && data.Drive == drive && data.Writing == writing)) return representation;
        var blocks = image.AvailableBlocks.Select(block => block with
        {
            Data = new MediaDriveSectorData(block.Data, drive,
                block.Address.Cylinder >= 0 && block.Address.Head >= 0
                    ? new(block.Address.Cylinder, block.Address.Head, block.Address.Number)
                    : new(0, 0, LogicalBlock: block.LogicalBlock), writing)
        });
        return new(new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack,
            blocks, image.AllowsVariableBlockSize, image.Capacity, image.BlockCount, image.AddressingKind));
    }

    private static BlockMediaImageRepresentation BindBlocks(IMediaDriveMechanics drive, BlockMediaImageRepresentation image, bool writing)
    {
        if (image.Drive == drive && image.DriveWriting == writing) return image;
        var ranges = image.Ranges.Select(range => range.Source is null ? range : new MediaDataRange(range.Address, range.Length, range.Kind,
            new MediaDriveRandomAccessData(range.Source, drive,
                offset => BlockPosition(image, checked(range.Address + offset - range.SourceOffset)),
                offset => BlockContiguousLength(image, checked(range.Address + offset - range.SourceOffset)), writing), range.SourceOffset)).ToArray();
        return new(image.Capacity, image.LogicalBlockSize, ranges, image.Geometry) { Drive = drive, DriveWriting = writing };
    }

    internal static MediaDrivePosition BlockPosition(BlockMediaImageRepresentation image, long address)
    {
        if (image.Geometry is not { Heads: > 0, SectorsPerTrack: > 0 } geometry)
            return new(0, 0, LogicalBlock: address / image.LogicalBlockSize);
        var sector = address / geometry.SectorSize;
        return new(checked((int)(sector / geometry.SectorsPerTrack / geometry.Heads)),
            (int)(sector / geometry.SectorsPerTrack % geometry.Heads), (int)(sector % geometry.SectorsPerTrack));
    }

    internal static long BlockContiguousLength(BlockMediaImageRepresentation image, long address)
    {
        if (image.Geometry is not { SectorsPerTrack: > 0 } geometry) return image.Capacity - address;
        var unit = checked((long)geometry.SectorsPerTrack * geometry.SectorSize);
        return unit - address % unit;
    }

    private static OpticalMediaImageRepresentation BindOptical(IMediaDriveMechanics drive, OpticalMediaImageRepresentation image, bool writing)
    {
        if (image.Tracks!.All(track => track.DataSource is MediaDriveRandomAccessData source && source.Drive == drive && source.Writing == writing)) return image;
        var tracks = image.Tracks!.Select(track => new OpticalTrackDescriptor(
            track.SessionNumber, track.TrackNumber, track.Mode, track.FirstSector, track.SectorCount,
            track.StoredSectorSize, track.UserDataOffset, track.UserDataLength,
            new MediaDriveRandomAccessData(track.DataSource, drive,
                offset => new(0, 0, LogicalBlock: track.FirstSector + (offset - track.SourceOffset) / track.StoredSectorSize),
                offset => track.StoredSectorSize - (offset - track.SourceOffset) % track.StoredSectorSize, writing),
            track.SourceOffset, track.Indexes, track.PregapSectors, track.PostgapSectors, track.Flags, track.CatalogNumber, track.Isrc,
            track.SubchannelSource is not { } subchannels ? null : new MediaDriveRandomAccessData(subchannels, drive,
                offset => new(0, 0, LogicalBlock: track.FirstSector + (offset - track.SubchannelSourceOffset) / track.SubchannelStride),
                offset => track.SubchannelStride - (offset - track.SubchannelSourceOffset) % track.SubchannelStride, writing),
            track.SubchannelSourceOffset, track.SubchannelBytesPerSector, track.StoredPregapSectors,
            track.StoredPregapSourceOffset, track.SubchannelStride)).ToArray();
        return new(image.LogicalLength, tracks, image.LayerCount, image.FaceCount, image.AssociatedFiles);
    }
}
