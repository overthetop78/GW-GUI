using System.Collections.Frozen;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.MediaEngine.Formats.Optical.BinCue;

/// <summary>Resolves a CUE sheet and its binary track files into one optical media document.</summary>
public sealed class BinCueReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.BinCue }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly CueSheetReader cueSheets;

    public BinCueReader(CueSheetReader? cueSheets = null)
    {
        this.cueSheets = cueSheets ?? new CueSheetReader();
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => BinCueFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => BinCueFormat.AssociatedFileExtensions;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return false;
        if (!BinCueFormat.Extensions.Contains(context.Extension)) return false;
        try
        {
            var sheet = await cueSheets.ReadAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
            return sheet.Files.All(file => file.Kind == CueFileKind.Binary);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var sheet = await cueSheets.ReadAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
        var cueDirectory = Path.GetDirectoryName(Path.GetFullPath(context.Source.PrimaryPath))
            ?? Directory.GetCurrentDirectory();
        var associatedPaths = new List<string>();
        var tracks = new List<OpticalTrackDescriptor>();
        long firstSector = 0;

        foreach (var file in sheet.Files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.Kind != CueFileKind.Binary)
                throw new NotSupportedException("WAVE CUE tracks require the separate PCM audio adapter, which is not registered yet.");
            var path = ResolveAssociatedPath(cueDirectory, file.DeclaredPath);
            if (!File.Exists(path)) throw new InvalidDataException($"CUE track file '{file.DeclaredPath}' is missing.");
            if (path.Equals(Path.GetFullPath(context.Source.PrimaryPath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("A CUE sheet cannot use itself as track data.");

            var source = new FileRandomAccessData(path);
            var sectorSizes = file.Tracks.Select(track => GetStoredSectorSize(track.Mode)).Distinct().ToArray();
            if (sectorSizes.Length != 1)
                throw new NotSupportedException("Tracks with different stored sector sizes must use separate CUE files in the initial profile.");
            var sectorSize = sectorSizes[0];
            if (source.Length == 0 || source.Length % sectorSize != 0)
                throw new InvalidDataException($"CUE track file '{file.DeclaredPath}' is not aligned to {sectorSize}-byte sectors.");
            var fileSectorCount = source.Length / sectorSize;
            var starts = file.Tracks.Select(GetIndexOnePosition).ToArray();
            if (!starts.SequenceEqual(starts.Order()))
                throw new InvalidDataException("CUE INDEX 01 positions must increase within each file.");

            for (var index = 0; index < file.Tracks.Count; index++)
            {
                var declaration = file.Tracks[index];
                var start = starts[index];
                var end = index + 1 < starts.Length ? starts[index + 1] : fileSectorCount;
                if (start < 0 || start >= fileSectorCount || end <= start || end > fileSectorCount)
                    throw new InvalidDataException($"CUE track {declaration.Number} exceeds its source file.");
                var storedPregap = GetStoredPregap(declaration, start);
                var (userOffset, userLength) = GetUserDataWindow(declaration.Mode);
                tracks.Add(new OpticalTrackDescriptor(
                    1,
                    declaration.Number,
                    declaration.Mode,
                    firstSector,
                    end - start,
                    sectorSize,
                    userOffset,
                    userLength,
                    source,
                    checked(start * sectorSize),
                    declaration.Indexes.Select(item => new OpticalTrackIndex(item.Number, item.RelativeSector - start)).ToArray(),
                    declaration.PregapSectors,
                    declaration.PostgapSectors,
                    declaration.Flags,
                    sheet.CatalogNumber,
                    declaration.Isrc,
                    storedPregapSectors: storedPregap,
                    storedPregapSourceOffset: storedPregap > 0
                        ? checked((start - storedPregap) * sectorSize)
                        : null));
                firstSector = checked(firstSector + end - start);
            }

            associatedPaths.Add(path);
        }

        var sourceDescriptor = new MediaSourceDescriptor(
            context.Source.PrimaryPath,
            associatedPaths,
            context.Source.KnownLength,
            context.Source.RequestedFormatId);
        var representation = new OpticalMediaImageRepresentation(
            tracks.Sum(track => checked(track.SectorCount * track.UserDataLength)),
            tracks,
            associatedFiles: [context.Source.PrimaryPath, .. associatedPaths]);
        return new MediaImageDocument(
            sourceDescriptor,
            OpticalImageFormatIds.BinCue,
            MediaKind.Optical,
            representation,
            [],
            [],
            new Dictionary<string, string>(sheet.Metadata, StringComparer.OrdinalIgnoreCase)
            {
                ["trackCount"] = tracks.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["fileCount"] = associatedPaths.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static string ResolveAssociatedPath(string cueDirectory, string declaredPath)
    {
        if (Path.IsPathFullyQualified(declaredPath))
            return Path.GetFullPath(declaredPath);
        return Path.GetFullPath(Path.Combine(cueDirectory, declaredPath));
    }

    private static long GetIndexOnePosition(CueTrackDeclaration track) =>
        track.Indexes.SingleOrDefault(index => index.Number == 1)?.RelativeSector
        ?? throw new InvalidDataException($"CUE track {track.Number} does not contain INDEX 01.");

    private static long GetStoredPregap(CueTrackDeclaration track, long indexOne)
    {
        var indexZero = track.Indexes.SingleOrDefault(index => index.Number == 0);
        if (indexZero is null) return 0;
        if (indexZero.RelativeSector > indexOne)
            throw new InvalidDataException($"CUE track {track.Number} has INDEX 00 after INDEX 01.");
        return indexOne - indexZero.RelativeSector;
    }

    private static int GetStoredSectorSize(OpticalTrackMode mode) => mode switch
    {
        OpticalTrackMode.Audio => OpticalSectorConstants.AudioSectorSize,
        OpticalTrackMode.Mode1Data2048 => OpticalSectorConstants.Data2048Size,
        OpticalTrackMode.Mode1Raw2352 => OpticalSectorConstants.RawSectorSize,
        OpticalTrackMode.Mode2Data2336 => OpticalSectorConstants.Mode2Data2336Size,
        OpticalTrackMode.Mode2Raw2352 => OpticalSectorConstants.RawSectorSize,
        _ => throw new NotSupportedException($"Unsupported optical track mode '{mode}'.")
    };

    private static (int Offset, int Length) GetUserDataWindow(OpticalTrackMode mode) => mode switch
    {
        OpticalTrackMode.Audio => (0, OpticalSectorConstants.AudioSectorSize),
        OpticalTrackMode.Mode1Data2048 => (0, OpticalSectorConstants.Data2048Size),
        OpticalTrackMode.Mode1Raw2352 => (OpticalSectorConstants.Mode1RawUserDataOffset, OpticalSectorConstants.Mode1RawUserDataLength),
        OpticalTrackMode.Mode2Data2336 => (0, OpticalSectorConstants.Mode2Data2336Size),
        OpticalTrackMode.Mode2Raw2352 => (OpticalSectorConstants.Mode2RawUserDataOffset, OpticalSectorConstants.Mode2RawUserDataLength),
        _ => throw new NotSupportedException($"Unsupported optical track mode '{mode}'.")
    };
}
