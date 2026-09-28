using System.Collections.Frozen;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Reading.Optical;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Optical.Xdvdfs;

/// <summary>Réécrit une image XDVDFS sans perdre ses secteurs ni sa structure optique.</summary>
public sealed class XdvdfsWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.XboxXdvdfs }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> Representations =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;
    private readonly OpticalSectorReader sectors = new();

    public XdvdfsWriter(IAtomicImageFileWriter? files = null) =>
        this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.OpticalXboxXdvdfs;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => Representations;
    public IReadOnlySet<string> ProducedFileExtensions => XdvdfsFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        document.MediaKind == MediaKind.Optical
        && SupportedFormatIds.Contains(targetFormatId)
        && XdvdfsFormat.Extensions.Contains(NormalizeExtension(targetExtension))
        && document.Representation is OpticalMediaImageRepresentation optical
        && optical.Tracks is { Count: 1 } track
        && track[0].Mode == OpticalTrackMode.Mode1Data2048
        && track[0].StoredSectorSize == XdvdfsFormat.SectorSize
        && track[0].UserDataOffset == 0
        && track[0].UserDataLength == XdvdfsFormat.SectorSize
        && track[0].SectorCount > XdvdfsFormat.VolumeDescriptorSector;

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath))
            || document.Representation is not OpticalMediaImageRepresentation optical
            || optical.Tracks is not { Count: 1 } tracks)
            throw new InvalidDataException("The optical document cannot be written as an Xbox XDVDFS image.");

        var track = tracks[0];
        await files.WriteAsync(outputPath, async (output, token) =>
        {
            for (long sector = 0; sector < track.SectorCount; sector++)
            {
                token.ThrowIfCancellationRequested();
                var bytes = await sectors.ReadStoredSectorAsync(track, sector, token).ConfigureAwait(false);
                await output.WriteAsync(bytes, token).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith(".", StringComparison.Ordinal)
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
}
