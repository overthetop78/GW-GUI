using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Xdvdfs;

/// <summary>Lit les images de jeu Xbox dont le système de fichiers est XDVDFS/XISO.</summary>
public sealed class XdvdfsReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> FormatIds =
        new[] { OpticalImageFormatIds.XboxXdvdfs }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> MediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> Representations =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => FormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => XdvdfsFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [XdvdfsFormat.Magic];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => MediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => Representations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => FormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!XdvdfsFormat.Extensions.Contains(context.Extension)
            || !XdvdfsFormat.IsLengthCompatible(context.Length))
            return false;
        if (context.RequestedFormatId is not null
            && !FormatIds.Contains(context.RequestedFormatId))
            return false;
        return await TryReadDescriptorAsync(context, cancellationToken).ConfigureAwait(false) is not null;
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!XdvdfsFormat.IsLengthCompatible(context.Length))
            throw new InvalidDataException("The Xbox XDVDFS image is empty or is not aligned to 2048-byte sectors.");
        if (context.RequestedFormatId is not null
            && !FormatIds.Contains(context.RequestedFormatId))
            throw new InvalidDataException("The requested format is not an Xbox XDVDFS image.");

        var descriptor = await TryReadDescriptorAsync(context, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidDataException("The Xbox XDVDFS volume descriptor is invalid.");
        var source = new FileRandomAccessData(context.Source.PrimaryPath);
        var sectorCount = context.Length / XdvdfsFormat.SectorSize;
        var track = new OpticalTrackDescriptor(
            1, 1, OpticalTrackMode.Mode1Data2048, 0, sectorCount,
            XdvdfsFormat.SectorSize, 0, XdvdfsFormat.SectorSize, source, 0,
            [new OpticalTrackIndex(1, 0)]);
        var representation = new OpticalMediaImageRepresentation(
            context.Length, [track], associatedFiles: [context.Source.PrimaryPath]);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [XdvdfsMetadataConstants.RootDirectorySector] = descriptor.RootSector.ToString(CultureInfo.InvariantCulture),
            [XdvdfsMetadataConstants.RootDirectorySize] = descriptor.RootSize.ToString(CultureInfo.InvariantCulture),
            [XdvdfsMetadataConstants.SectorSize] = XdvdfsFormat.SectorSize.ToString(CultureInfo.InvariantCulture)
        };
        var volume = new MediaVolumeDescriptor(
            0,
            context.Length,
            MediaVolumeOrigins.OpticalTrack,
            sessionNumber: 1,
            trackNumber: 1,
            fileSystemId: FileSystemIds.Xdvdfs,
            name: XdvdfsMetadataConstants.VolumeName);
        return new MediaImageDocument(
            context.Source,
            OpticalImageFormatIds.XboxXdvdfs,
            MediaKind.Optical,
            representation,
            [volume],
            [],
            metadata);
    }

    private static async ValueTask<(uint RootSector, uint RootSize)?> TryReadDescriptorAsync(
        MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var offset = checked((long)XdvdfsFormat.VolumeDescriptorSector * XdvdfsFormat.SectorSize);
        var descriptor = await context.ReadAsync(offset, XdvdfsFormat.VolumeDescriptorLength, cancellationToken)
            .ConfigureAwait(false);
        if (!descriptor.Span.Slice(XdvdfsFormat.MagicOffset, XdvdfsFormat.Magic.Length)
                .SequenceEqual(XdvdfsFormat.Magic)
            || !descriptor.Span.Slice(XdvdfsFormat.TrailerMagicOffset, XdvdfsFormat.Magic.Length)
                .SequenceEqual(XdvdfsFormat.Magic))
            return null;

        var rootSector = BinaryPrimitives.ReadUInt32LittleEndian(
            descriptor.Span.Slice(XdvdfsFormat.RootDirectorySectorOffset, sizeof(uint)));
        var rootSize = BinaryPrimitives.ReadUInt32LittleEndian(
            descriptor.Span.Slice(XdvdfsFormat.RootDirectorySizeOffset, sizeof(uint)));
        var sectors = context.Length / XdvdfsFormat.SectorSize;
        if (rootSize == 0)
            return rootSector == 0 ? (0, 0) : null;
        if (rootSize % XdvdfsFormat.SectorSize != 0
            || rootSector >= sectors
            || rootSize > checked((ulong)(sectors - rootSector) * XdvdfsFormat.SectorSize))
            return null;
        return (rootSector, rootSize);
    }
}
