using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Images.Models.Optical;
using GWGUI.MediaEngine.Images.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Optical.BinCue;

/// <summary>Writes one autonomous binary file per optical track and publishes its CUE descriptor last.</summary>
public sealed class BinCueWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.BinCue }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly CueSheetWriter cueSheets;

    public BinCueWriter(CueSheetWriter? cueSheets = null)
    {
        this.cueSheets = cueSheets ?? new CueSheetWriter();
    }

    public string Id => MediaImageWriterIds.OpticalBinCue;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => BinCueFormat.Extensions;
    public bool ProducesMultipleFiles => true;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (!SupportedFormatIds.Contains(targetFormatId)
            || !BinCueFormat.Extensions.Contains(targetExtension)
            || document.MediaKind != MediaKind.Optical
            || document.Representation is not OpticalMediaImageRepresentation { Tracks.Count: > 0 } optical
            || optical.LayerCount is not null
            || optical.FaceCount is not null)
            return false;
        if (optical.Tracks.Any(track => track.SessionNumber != 1
            || track.HasSubchannels
            || !IsSupportedMode(track.Mode)
            || track.StoredPregapSectors > 0 && track.StoredPregapSourceOffset is null))
            return false;
        var catalogs = optical.Tracks.Select(track => track.CatalogNumber).Distinct(StringComparer.Ordinal).ToArray();
        return catalogs.Length == 1;
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var fullCuePath = Path.GetFullPath(outputPath);
        var extension = Path.GetExtension(fullCuePath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || document.Representation is not OpticalMediaImageRepresentation optical)
            throw new InvalidDataException("The optical document cannot be written as a lossless BIN/CUE set.");

        var directory = Path.GetDirectoryName(fullCuePath) ?? Directory.GetCurrentDirectory();
        var baseName = Path.GetFileNameWithoutExtension(fullCuePath);
        var files = new Dictionary<string, Func<Stream, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase);
        var cueFiles = new List<CueFileDescriptor>();
        foreach (var track in optical.Tracks!)
        {
            var binaryName = $"{baseName}.track{track.TrackNumber:D2}{DiskImageFileExtensions.Bin}";
            var binaryPath = Path.Combine(directory, binaryName);
            files.Add(binaryPath, (output, token) => WriteTrackAsync(track, output, token));
            cueFiles.Add(new CueFileDescriptor(
                binaryName,
                CueFileKind.Binary,
                [CreateTrackDeclaration(track)]));
        }

        var sheet = new CueSheetDocument(
            cueFiles,
            optical.Tracks![0].CatalogNumber,
            SelectCueMetadata(document.Metadata));
        files.Add(fullCuePath, (output, token) => cueSheets.WriteAsync(sheet, output, token));
        await AtomicMediaFileSetWriter.WriteAsync(files, cancellationToken).ConfigureAwait(false);
        return files.Keys.ToArray();
    }

    private static CueTrackDeclaration CreateTrackDeclaration(OpticalTrackDescriptor track)
    {
        var indexes = new List<OpticalTrackIndex>();
        if (track.StoredPregapSectors > 0)
            indexes.Add(new OpticalTrackIndex(0, 0));
        indexes.Add(new OpticalTrackIndex(1, track.StoredPregapSectors));
        indexes.AddRange(track.Indexes
            .Where(index => index.Number > 1)
            .Select(index => new OpticalTrackIndex(index.Number, checked(track.StoredPregapSectors + index.RelativeSector))));
        return new CueTrackDeclaration(
            track.TrackNumber,
            track.Mode,
            indexes,
            track.PregapSectors,
            track.PostgapSectors,
            track.Flags,
            track.Isrc);
    }

    private static IReadOnlyDictionary<string, string> SelectCueMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        return metadata.Where(pair =>
                pair.Key.Equals(CueSheetConstants.Title, StringComparison.OrdinalIgnoreCase)
                || pair.Key.Equals(CueSheetConstants.Performer, StringComparison.OrdinalIgnoreCase)
                || pair.Key.Equals(CueSheetConstants.Songwriter, StringComparison.OrdinalIgnoreCase)
                || pair.Key.StartsWith(CueSheetConstants.Rem + ":", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task WriteTrackAsync(
        OpticalTrackDescriptor track,
        Stream output,
        CancellationToken cancellationToken)
    {
        if (track.StoredPregapSectors > 0)
            await CopyAsync(
                track.DataSource,
                track.StoredPregapSourceOffset!.Value,
                checked(track.StoredPregapSectors * track.StoredSectorSize),
                output,
                cancellationToken).ConfigureAwait(false);
        await CopyAsync(
            track.DataSource,
            track.SourceOffset,
            checked(track.SectorCount * track.StoredSectorSize),
            output,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task CopyAsync(
        GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData source,
        long sourceOffset,
        long length,
        Stream output,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * DataSizeConstants.BytesPerKibibyte];
        long copied = 0;
        while (copied < length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = (int)Math.Min(buffer.Length, length - copied);
            var memory = buffer.AsMemory(0, count);
            await source.ReadExactlyAsync(checked(sourceOffset + copied), memory, cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
            copied += count;
        }
    }

    private static bool IsSupportedMode(OpticalTrackMode mode) => mode is
        OpticalTrackMode.Audio
        or OpticalTrackMode.Mode1Data2048
        or OpticalTrackMode.Mode1Raw2352
        or OpticalTrackMode.Mode2Data2336
        or OpticalTrackMode.Mode2Raw2352;
}
