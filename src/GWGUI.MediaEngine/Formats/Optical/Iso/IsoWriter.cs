using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Reading.Optical;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.MediaEngine.Formats.Optical.Iso;

/// <summary>Writes one continuous 2048-byte optical data track without discarding optical structure.</summary>
public sealed class IsoWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.Iso }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;
    private readonly OpticalSectorReader sectors = new();

    public IsoWriter(IAtomicImageFileWriter? files = null)
    {
        this.files = files ?? new AtomicImageFileWriter();
    }

    public string Id => MediaImageWriterIds.OpticalIso;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => IsoFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && IsoFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.Optical
            && document.Representation is OpticalMediaImageRepresentation optical
            && IsLosslessIsoProfile(optical);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || document.Representation is not OpticalMediaImageRepresentation optical)
            throw new InvalidDataException("The optical document cannot be written as a lossless single-track ISO image.");

        var track = optical.Tracks![0];
        await files.WriteAsync(
            outputPath,
            (output, token) => WriteTrackAsync(track, output, token),
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool IsLosslessIsoProfile(OpticalMediaImageRepresentation optical)
    {
        if (optical.Tracks is not { Count: 1 } || optical.Sessions is not { Count: 1 }
            || optical.LayerCount is not null || optical.FaceCount is not null)
            return false;
        var track = optical.Tracks[0];
        return track.SessionNumber == 1
            && track.TrackNumber == 1
            && track.FirstSector == 0
            && track.Mode == OpticalTrackMode.Mode1Data2048
            && track.StoredSectorSize == IsoFormat.SectorSize
            && track.UserDataOffset == 0
            && track.UserDataLength == IsoFormat.SectorSize
            && !track.HasSubchannels
            && track.PregapSectors == 0
            && track.PostgapSectors == 0
            && track.Flags.Count == 0
            && track.CatalogNumber is null
            && track.Isrc is null
            && track.Indexes.All(index => index.Number == 1 && index.RelativeSector == 0);
    }

    private async Task WriteTrackAsync(
        OpticalTrackDescriptor track,
        Stream output,
        CancellationToken cancellationToken)
    {
        for (long sector = 0; sector < track.SectorCount; sector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bytes = await sectors.ReadStoredSectorAsync(track, sector, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
    }
}
