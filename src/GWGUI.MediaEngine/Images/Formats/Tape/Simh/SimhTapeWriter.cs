using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Simh;

/// <summary>Writes SIMH data records, even-byte padding, and metadata markers in source order.</summary>
public sealed class SimhTapeWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.SimhTap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public SimhTapeWriter(IAtomicImageFileWriter? files = null) =>
        this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.TapeSimh;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SimhTapeFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && SimhTapeFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.Tape
            && TryGetObjects(document, out _);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension) || !TryGetObjects(document, out var objects))
            throw new InvalidDataException("The sequential document cannot be represented as SIMH tape objects.");
        await files.WriteAsync(
            outputPath,
            async (output, token) =>
            {
                foreach (var item in objects)
                    await WriteObjectAsync(output, item, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool TryGetObjects(MediaImageDocument document, out IReadOnlyList<SimhObject> objects)
    {
        objects = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { } segments }) return false;
        var selected = new List<SimhObject>();
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (!segment.Metadata.TryGetValue(SimhTapeConstants.ObjectKindMetadataKey, out var kind)) return false;
            if (kind == SimhTapeConstants.DataRecordKind)
            {
                if (segment.DataRange is not { Kind: MediaDataRangeKind.Stored, Source: not null, Length: > 0 } range
                    || range.Length > SimhTapeConstants.RecordLengthMask)
                    return false;
                var hasError = segment.Metadata.TryGetValue(SimhTapeConstants.RecordErrorMetadataKey, out var errorText)
                    && bool.TryParse(errorText, out var parsedError)
                    && parsedError;
                var word = checked((uint)range.Length) | (hasError ? SimhTapeConstants.ErrorFlag : 0);
                selected.Add(new SimhObject(word, range));
                continue;
            }

            var marker = kind switch
            {
                SimhTapeConstants.TapeMarkKind => SimhTapeConstants.TapeMark,
                SimhTapeConstants.EraseGapKind => SimhTapeConstants.EraseGap,
                SimhTapeConstants.EndOfMediumKind => SimhTapeConstants.EndOfMedium,
                _ => (uint?)null
            };
            if (marker is null || segment.DataRange is not null) return false;
            selected.Add(new SimhObject(marker.Value, null));
        }
        if (selected.Count == 0) return false;
        objects = selected;
        return true;
    }

    private static async Task WriteObjectAsync(Stream output, SimhObject item, CancellationToken cancellationToken)
    {
        var word = new byte[SimhTapeConstants.WordSize];
        BinaryPrimitives.WriteUInt32LittleEndian(word, item.Word);
        await output.WriteAsync(word, cancellationToken).ConfigureAwait(false);
        if (item.Range is null) return;

        var range = item.Range;
        var buffer = new byte[64 * 1024];
        long completed = 0;
        while (completed < range.Length)
        {
            var count = checked((int)Math.Min(buffer.Length, range.Length - completed));
            await range.Source!.ReadExactlyAsync(range.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            completed += count;
        }
        if ((range.Length & 1) != 0) await output.WriteAsync(new byte[1], cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(word, cancellationToken).ConfigureAwait(false);
    }

    private sealed record SimhObject(uint Word, MediaDataRange? Range);
}
