using System.Collections.Frozen;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Optical;

namespace GWGUI.MediaEngine.Formats.Optical.CloneCd;

/// <summary>Reads a CloneCD descriptor, mandatory raw IMG data, and optional SUB data as one optical document.</summary>
public sealed class CloneCdReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { OpticalImageFormatIds.CloneCd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds =
        new[] { MediaKind.Optical }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.OpticalTracks }.ToFrozenSet();
    private readonly CloneCdDescriptorReader descriptors;

    public CloneCdReader(CloneCdDescriptorReader? descriptors = null)
    {
        this.descriptors = descriptors ?? new CloneCdDescriptorReader();
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => CloneCdFormat.Extensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => CloneCdFormat.AssociatedFileExtensions;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId))
            return false;
        if (!CloneCdFormat.Extensions.Contains(context.Extension)) return false;
        try
        {
            _ = await descriptors.ReadAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
            return File.Exists(Path.ChangeExtension(context.Source.PrimaryPath, DiskImageFileExtensions.Img));
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or NotSupportedException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(
        MediaRecognitionContext context,
        CancellationToken cancellationToken)
    {
        var descriptor = await descriptors.ReadAsync(context.Source.PrimaryPath, cancellationToken).ConfigureAwait(false);
        var imagePath = Path.GetFullPath(Path.ChangeExtension(context.Source.PrimaryPath, DiskImageFileExtensions.Img));
        if (!File.Exists(imagePath)) throw new InvalidDataException("The mandatory CloneCD IMG file is missing.");
        var image = new FileRandomAccessData(imagePath);
        if (image.Length == 0 || image.Length % CloneCdConstants.RawSectorSize != 0)
            throw new InvalidDataException("The CloneCD IMG file is not aligned to 2352-byte sectors.");
        var totalSectors = image.Length / CloneCdConstants.RawSectorSize;

        var subPath = Path.GetFullPath(Path.ChangeExtension(context.Source.PrimaryPath, DiskImageFileExtensions.Sub));
        FileRandomAccessData? subchannels = null;
        if (File.Exists(subPath))
        {
            subchannels = new FileRandomAccessData(subPath);
            if (subchannels.Length != checked(totalSectors * CloneCdConstants.SubchannelSize))
                throw new InvalidDataException("The CloneCD SUB file does not contain one 96-byte record per IMG sector.");
        }

        var starts = descriptor.Tracks.Select(GetIndexOne).ToArray();
        if (!starts.SequenceEqual(starts.Order()))
            throw new InvalidDataException("CloneCD INDEX 1 positions must increase across tracks.");
        var tracks = new List<OpticalTrackDescriptor>(descriptor.Tracks.Count);
        for (var index = 0; index < descriptor.Tracks.Count; index++)
        {
            var declaration = descriptor.Tracks[index];
            var start = starts[index];
            var end = index + 1 < starts.Length ? starts[index + 1] : totalSectors;
            if (start < 0 || start >= totalSectors || end <= start || end > totalSectors)
                throw new InvalidDataException($"CloneCD track {declaration.Number} exceeds the IMG file.");
            var indexZero = declaration.Indexes.SingleOrDefault(item => item.Number == 0);
            var storedPregap = indexZero is null ? 0 : start - indexZero.RelativeSector;
            if (storedPregap < 0) throw new InvalidDataException($"CloneCD track {declaration.Number} has INDEX 0 after INDEX 1.");
            var session = descriptor.TocEntries
                .Where(entry => entry.Point == declaration.Number)
                .Select(entry => entry.SessionNumber)
                .FirstOrDefault();
            if (session <= 0) session = 1;
            var (userOffset, userLength) = GetUserDataWindow(declaration.Mode);
            tracks.Add(new OpticalTrackDescriptor(
                session,
                declaration.Number,
                declaration.Mode,
                start,
                end - start,
                CloneCdConstants.RawSectorSize,
                userOffset,
                userLength,
                image,
                checked(start * CloneCdConstants.RawSectorSize),
                declaration.Indexes.Select(item => new OpticalTrackIndex(item.Number, item.RelativeSector - start)).ToArray(),
                flags: declaration.Flags,
                catalogNumber: descriptor.CatalogNumber,
                isrc: declaration.Isrc,
                subchannelSource: subchannels,
                subchannelSourceOffset: subchannels is null ? 0 : checked(start * CloneCdConstants.SubchannelSize),
                subchannelBytesPerSector: subchannels is null ? 0 : CloneCdConstants.SubchannelSize,
                storedPregapSectors: storedPregap,
                storedPregapSourceOffset: storedPregap > 0
                    ? checked((start - storedPregap) * CloneCdConstants.RawSectorSize)
                    : null));
        }

        var associatedPaths = subchannels is null ? new[] { imagePath } : new[] { imagePath, subPath };
        var source = new MediaSourceDescriptor(
            context.Source.PrimaryPath,
            associatedPaths,
            context.Source.KnownLength,
            context.Source.RequestedFormatId);
        var representation = new OpticalMediaImageRepresentation(
            tracks.Sum(track => checked(track.SectorCount * track.UserDataLength)),
            tracks,
            associatedFiles: [context.Source.PrimaryPath, .. associatedPaths]);
        return new MediaImageDocument(
            source,
            OpticalImageFormatIds.CloneCd,
            MediaKind.Optical,
            representation,
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["version"] = descriptor.Version.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["sessionCount"] = descriptor.SessionCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["trackCount"] = tracks.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["subchannels"] = (subchannels is not null).ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
    }

    private static long GetIndexOne(CueTrackDeclaration track) =>
        track.Indexes.SingleOrDefault(index => index.Number == 1)?.RelativeSector
        ?? throw new InvalidDataException($"CloneCD track {track.Number} does not contain INDEX 1.");

    private static (int Offset, int Length) GetUserDataWindow(OpticalTrackMode mode) => mode switch
    {
        OpticalTrackMode.Audio => (0, OpticalSectorConstants.AudioSectorSize),
        OpticalTrackMode.Mode1Raw2352 => (OpticalSectorConstants.Mode1RawUserDataOffset, OpticalSectorConstants.Mode1RawUserDataLength),
        OpticalTrackMode.Mode2Raw2352 => (OpticalSectorConstants.Mode2RawUserDataOffset, OpticalSectorConstants.Mode2RawUserDataLength),
        _ => throw new NotSupportedException($"Unsupported CloneCD track mode '{mode}'.")
    };
}
