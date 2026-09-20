using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO.Compression;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.Uef;

/// <summary>Writes retained UEF chunks directly or through explicitly requested gzip compression.</summary>
public sealed class UefWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.Uef }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public UefWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();
    public string Id => MediaImageWriterIds.TapeUef;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => UefFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        SupportedFormatIds.Contains(targetFormatId)
        && UefFormat.Extensions.Contains(targetExtension)
        && document.MediaKind == MediaKind.Tape
        && TryGetProfile(document, out _)
        && TryGetChunks(document, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension)
            || !TryGetProfile(document, out var profile)
            || !TryGetChunks(document, out var chunks))
            throw new InvalidDataException("The sequential document cannot be written as a lossless UEF chunk stream.");

        await files.WriteAsync(
            outputPath,
            async (output, token) =>
            {
                if (!profile.Gzip)
                {
                    await WriteLogicalAsync(output, profile, chunks, token).ConfigureAwait(false);
                    return;
                }
                await using var compressed = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true);
                await WriteLogicalAsync(compressed, profile, chunks, token).ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteLogicalAsync(
        Stream output,
        UefProfile profile,
        IReadOnlyList<UefChunk> chunks,
        CancellationToken cancellationToken)
    {
        var header = new byte[UefConstants.HeaderSize];
        System.Text.Encoding.ASCII.GetBytes(UefConstants.Signature, header);
        header[10] = profile.MinorVersion;
        header[11] = profile.MajorVersion;
        await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        var chunkHeader = new byte[UefConstants.ChunkHeaderSize];
        foreach (var chunk in chunks)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(chunkHeader, chunk.Id);
            BinaryPrimitives.WriteUInt32LittleEndian(chunkHeader.AsSpan(2), checked((uint)(chunk.Range?.Length ?? 0)));
            await output.WriteAsync(chunkHeader, cancellationToken).ConfigureAwait(false);
            if (chunk.Range is not null) await CopyAsync(chunk.Range, output, cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool TryGetProfile(MediaImageDocument document, out UefProfile profile)
    {
        profile = default;
        var gzip = false;
        if (document.Metadata.TryGetValue(UefConstants.GzipMetadataKey, out var gzipText)
            && !bool.TryParse(gzipText, out gzip))
            return false;
        var major = UefConstants.SupportedMajorVersion;
        var minor = UefConstants.SupportedMinorVersion;
        if (document.Metadata.TryGetValue(UefConstants.MajorVersionMetadataKey, out var majorText)
            && !byte.TryParse(majorText, NumberStyles.None, CultureInfo.InvariantCulture, out major))
            return false;
        if (document.Metadata.TryGetValue(UefConstants.MinorVersionMetadataKey, out var minorText)
            && !byte.TryParse(minorText, NumberStyles.None, CultureInfo.InvariantCulture, out minor))
            return false;
        if (major != UefConstants.SupportedMajorVersion || minor > UefConstants.SupportedMinorVersion) return false;
        profile = new UefProfile(gzip, major, minor);
        return true;
    }

    private static bool TryGetChunks(MediaImageDocument document, out IReadOnlyList<UefChunk> chunks)
    {
        chunks = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { } segments }) return false;
        var selected = new List<UefChunk>(segments.Count);
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (!segment.Metadata.TryGetValue(UefConstants.ChunkIdMetadataKey, out var idText)
                || !ushort.TryParse(idText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id))
                return false;
            if (segment.DataRange is { } range)
            {
                if (range.Kind != MediaDataRangeKind.Stored || range.Source is null || range.Length > uint.MaxValue) return false;
                selected.Add(new UefChunk(id, range));
            }
            else
            {
                selected.Add(new UefChunk(id, null));
            }
        }
        chunks = selected;
        return true;
    }

    private static async Task CopyAsync(MediaDataRange range, Stream output, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        var completed = 0L;
        while (completed < range.Length)
        {
            var count = (int)Math.Min(buffer.Length, range.Length - completed);
            await range.Source!.ReadExactlyAsync(
                range.SourceOffset + completed,
                buffer.AsMemory(0, count),
                cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            completed += count;
        }
    }

    private readonly record struct UefProfile(bool Gzip, byte MajorVersion, byte MinorVersion);
    private sealed record UefChunk(ushort Id, MediaDataRange? Range);
}
