using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.AtariCas;

/// <summary>Writes retained Atari CAS chunks in their original order without interpreting unknown payloads.</summary>
public sealed class AtariCasWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.AtariCas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public AtariCasWriter(IAtomicImageFileWriter? files = null) =>
        this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.TapeAtariCas;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => AtariCasFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormatIds.Contains(targetFormatId)
            && AtariCasFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.Tape
            && TryGetChunks(document, out _);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension) || !TryGetChunks(document, out var chunks))
            throw new InvalidDataException("The sequential document cannot be written as Atari CAS without losing chunk data.");

        await files.WriteAsync(
            outputPath,
            async (output, token) =>
            {
                foreach (var chunk in chunks)
                    await WriteChunkAsync(output, chunk, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool TryGetChunks(MediaImageDocument document, out IReadOnlyList<CasChunk> chunks)
    {
        chunks = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { } segments }) return false;

        var selected = new List<CasChunk>();
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (!segment.Metadata.TryGetValue(AtariCasConstants.ChunkIdMetadataKey, out var id)) continue;
            if (id.Length != AtariCasConstants.IdentifierLength) return false;
            if (!TryReadAuxiliary(segment.Metadata, out var auxiliary)) return false;

            var length = segment.DataRange?.Length ?? 0;
            if (length > ushort.MaxValue || length < 0) return false;
            if (length > 0 && segment.DataRange is not { Kind: MediaDataRangeKind.Stored, Source: not null }) return false;
            selected.Add(new CasChunk(id, checked((ushort)length), auxiliary, segment.DataRange));
        }

        if (selected.Count == 0 || selected[0].Id != AtariCasConstants.FileMarkerChunk) return false;
        chunks = selected;
        return true;
    }

    private static bool TryReadAuxiliary(IReadOnlyDictionary<string, string> metadata, out ushort auxiliary)
    {
        auxiliary = 0;
        return !metadata.TryGetValue(AtariCasConstants.AuxiliaryMetadataKey, out var text)
            || ushort.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out auxiliary);
    }

    private static async Task WriteChunkAsync(Stream output, CasChunk chunk, CancellationToken cancellationToken)
    {
        var header = new byte[AtariCasConstants.ChunkHeaderSize];
        System.Text.Encoding.ASCII.GetBytes(chunk.Id, header);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4), chunk.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(6), chunk.Auxiliary);
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);

        if (chunk.Length == 0) return;
        var range = chunk.DataRange!;
        var buffer = new byte[Math.Min(64 * 1024, chunk.Length)];
        var completed = 0;
        while (completed < chunk.Length)
        {
            var count = Math.Min(buffer.Length, chunk.Length - completed);
            await range.Source!.ReadExactlyAsync(range.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            completed += count;
        }
    }

    private sealed record CasChunk(string Id, ushort Length, ushort Auxiliary, MediaDataRange? DataRange);
}
