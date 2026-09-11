using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Decoding.Sequential;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Interfaces.Exploration;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Exploration.Sequential;

/// <summary>Exposes decoded sequential blocks, or stored raw records, as explorable files.</summary>
public sealed class SequentialContentFileSystemReader : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly SequentialDecoderRegistry decoders;

    public SequentialContentFileSystemReader(SequentialDecoderRegistry decoders)
    {
        ArgumentNullException.ThrowIfNull(decoders);
        this.decoders = decoders;
    }

    public string Id => FileSystemIds.SequentialContent;

    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;

    public bool CanRead(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        return document.MediaKind == MediaKind.Tape
            && document.Representation is SequentialMediaImageRepresentation
            && (volume.FileSystemId?.Equals(Id, StringComparison.OrdinalIgnoreCase) == true
                || volume.Origin.Equals(MediaVolumeOrigins.SequentialContent, StringComparison.OrdinalIgnoreCase));
    }

    public FileSystemVolume Read(MediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (!CanRead(document, volume))
            throw new InvalidDataException("The requested volume is not decoded sequential media content.");

        var representation = (SequentialMediaImageRepresentation)document.Representation;
        var warnings = new List<string>();
        var decoded = DecodeBest(document, warnings);
        IReadOnlyList<FileSystemEntry> entries = decoded is { Blocks.Count: > 0 }
            ? CreateDecodedEntries(decoded, warnings)
            : CreateRawEntries(representation.Segments ?? [], warnings);

        foreach (var segment in decoded?.UndecodedSegments ?? [])
            warnings.Add($"Undecoded {segment.Kind} segment at position {segment.Position}.");
        if (entries.Count == 0)
            warnings.Add("No decoded block or stored raw record is available for exploration.");

        return new FileSystemVolume(
            Path.GetFileNameWithoutExtension(document.Source.PrimaryPath),
            Id,
            volume.Length,
            0,
            null,
            null,
            entries,
            warnings.Distinct(StringComparer.Ordinal).ToArray(),
            freeSpaceKnown: false,
            attributes: decoded is null ? [document.FormatId] : [document.FormatId, decoded.DecoderId]);
    }

    private SequentialDecodeResult? DecodeBest(MediaImageDocument document, ICollection<string> warnings)
    {
        var compatible = decoders.FindCompatible(document);
        if (compatible.Count == 0) return null;

        var results = compatible
            .Select(decoder => decoder.DecodeAsync(document).GetAwaiter().GetResult())
            .OrderByDescending(result => result.Blocks.Count > 0)
            .ThenByDescending(result => result.Confidence)
            .ThenByDescending(result => result.Blocks.Count)
            .ToArray();
        var selected = results[0];
        foreach (var diagnostic in selected.Diagnostics) warnings.Add(diagnostic);
        if (results.Length > 1)
            warnings.Add($"Selected decoder '{selected.DecoderId}' from {results.Length} compatible sequential decoders.");
        return selected;
    }

    private static IReadOnlyList<FileSystemEntry> CreateDecodedEntries(
        SequentialDecodeResult decoded,
        ICollection<string> warnings)
    {
        var entries = new List<FileSystemEntry>(decoded.Blocks.Count);
        for (var index = 0; index < decoded.Blocks.Count; index++)
        {
            var block = decoded.Blocks[index];
            var name = CreateDecodedName(block, index, out var syntheticName);
            var diagnostics = block.IntegrityValid == false
                ? new[] { "The decoded block failed its integrity check." }
                : [];
            entries.Add(new FileSystemEntry(
                name,
                FileSystemEntryKind.File,
                block.Data.Length,
                null,
                string.Empty,
                0,
                ClampStorageReference(block.Position),
                true,
                [],
                block.Data.ToArray(),
                nativeTypeId: ReadMetadata(block.Metadata, "fileType", "msxCasFileType"),
                dataValid: block.IntegrityValid,
                syntheticName: syntheticName,
                diagnostics: diagnostics));
        }
        if (decoded.Blocks.Any(block => block.IntegrityValid == false))
            warnings.Add("One or more decoded blocks failed their integrity check.");
        return entries;
    }

    private static IReadOnlyList<FileSystemEntry> CreateRawEntries(
        IReadOnlyList<SequentialMediaSegment> segments,
        ICollection<string> warnings)
    {
        var entries = new List<FileSystemEntry>();
        foreach (var segment in segments)
        {
            if (segment.Kind is SequentialSegmentKind.Unknown)
            {
                warnings.Add($"Unknown sequential segment at position {segment.Position}.");
                continue;
            }
            if (segment.Kind is not (SequentialSegmentKind.DataBlock or SequentialSegmentKind.Record)) continue;
            if (segment.DataRange is not { } range)
            {
                warnings.Add($"{segment.Kind} segment at position {segment.Position} has no readable data range.");
                continue;
            }

            IReadOnlyList<byte>? content = null;
            var diagnostics = new List<string>();
            if (range.Kind == MediaDataRangeKind.Stored && range.Source is not null && range.Length <= int.MaxValue)
            {
                var data = new byte[(int)range.Length];
                range.Source.ReadExactlyAsync(range.SourceOffset, data).AsTask().GetAwaiter().GetResult();
                content = data;
            }
            else
            {
                diagnostics.Add("The raw record content is not stored in an addressable range.");
            }

            var ordinal = entries.Count + 1;
            entries.Add(new FileSystemEntry(
                $"{segment.Kind} {ordinal:D4}.bin",
                FileSystemEntryKind.File,
                range.Length,
                null,
                string.Empty,
                0,
                ClampStorageReference(segment.Position),
                true,
                [],
                content,
                nativeTypeId: segment.Kind.ToString(),
                dataValid: content is null ? null : true,
                syntheticName: true,
                diagnostics: diagnostics));
        }
        return entries;
    }

    private static string CreateDecodedName(SequentialDecodedBlock block, int index, out bool synthetic)
    {
        var storedName = ReadMetadata(block.Metadata, "fileName", "name");
        synthetic = string.IsNullOrWhiteSpace(storedName);
        var baseName = synthetic ? $"Block {index + 1:D4}" : SanitizeName(storedName!);
        var blockNumber = ReadMetadata(block.Metadata, "blockNumber");
        return blockNumber is null ? $"{baseName}.bin" : $"{baseName}.block-{blockNumber}.bin";
    }

    private static string? ReadMetadata(IReadOnlyDictionary<string, string> metadata, params string[] keys)
    {
        foreach (var key in keys)
            if (metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)) return value;
        return null;
    }

    private static string SanitizeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Trim().Select(character => invalid.Contains(character) ? '_' : character).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "Block" : sanitized;
    }

    private static int ClampStorageReference(long position) =>
        position > int.MaxValue ? int.MaxValue : checked((int)position);
}
