using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO;
using System.Security.Cryptography;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaFileSystems.Interfaces;

namespace GWGUI.MediaEngine.Images.Formats.Optical.WiiU;

/// <summary>Écrit une image Wii U logique complète au format WUD.</summary>
public sealed class WiiUWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.NintendoWiiU }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Wud, DiskImageFileExtensions.Wux }
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public WiiUWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.NintendoWiiU;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return document.MediaKind == MediaKind.Optical
            && SupportedFormatIds.Contains(targetFormatId)
            && SupportedExtensions.Contains(NormalizeExtension(targetExtension))
            && document.Representation is BlockMediaImageRepresentation blocks
            && WiiUFormat.IsWudLengthCompatible(blocks.Capacity)
            && HasCompleteReadableCoverage(blocks);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath))
            || document.Representation is not BlockMediaImageRepresentation blocks)
            throw new InvalidDataException("The Nintendo Wii U document cannot be written as a WUD or WUX image.");
        var readable = (IMediaBlockRepresentation)blocks;

        if (NormalizeExtension(Path.GetExtension(outputPath))
            .Equals(DiskImageFileExtensions.Wux, StringComparison.OrdinalIgnoreCase))
        {
            await WriteWuxAsync(outputPath, blocks.Capacity, readable, cancellationToken).ConfigureAwait(false);
            return [outputPath];
        }

        await files.WriteAsync(outputPath, async (output, token) =>
        {
            var buffer = new byte[256 * 1024];
            long offset = 0;
            while (offset < blocks.Capacity)
            {
                token.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, blocks.Capacity - offset);
                await readable.ReadExactlyAsync(offset, buffer.AsMemory(0, count), token).ConfigureAwait(false);
                await output.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                offset += count;
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private async Task WriteWuxAsync(
        string outputPath,
        long logicalLength,
        IMediaBlockRepresentation blocks,
        CancellationToken cancellationToken)
    {
        var chunkSize = WiiUFormat.DefaultWuxSectorSize;
        var chunkCount = checked((logicalLength + chunkSize - 1) / chunkSize);
        if (chunkCount <= 0 || chunkCount > int.MaxValue)
            throw new InvalidDataException("The Wii U image has too many WUX sectors for this writer.");

        var indexes = new uint[checked((int)chunkCount)];
        var knownChunks = new Dictionary<string, uint>(StringComparer.Ordinal);
        var temporaryPath = Path.GetTempFileName();
        try
        {
            await using (var temporary = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[chunkSize];
                var uniqueChunk = 0u;
                for (var chunk = 0; chunk < indexes.Length; chunk++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Array.Clear(buffer);
                    var offset = checked((long)chunk * chunkSize);
                    var count = (int)Math.Min(chunkSize, logicalLength - offset);
                    await blocks.ReadExactlyAsync(offset, buffer.AsMemory(0, count), cancellationToken)
                        .ConfigureAwait(false);
                    var hash = Convert.ToHexString(SHA256.HashData(buffer));
                    if (!knownChunks.TryGetValue(hash, out var storedChunk))
                    {
                        storedChunk = uniqueChunk++;
                        knownChunks.Add(hash, storedChunk);
                        await temporary.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
                    }
                    indexes[chunk] = storedChunk;
                }
            }

            await files.WriteAsync(outputPath, async (output, token) =>
            {
                var header = new byte[WiiUFormat.WuxHeaderSize];
                BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(0, 4), WiiUFormat.WuxMagic0);
                BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4, 4), WiiUFormat.WuxMagic1);
                BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8, 4), WiiUFormat.DefaultWuxSectorSize);
                BinaryPrimitives.WriteUInt64LittleEndian(header.AsSpan(12, 8), checked((ulong)logicalLength));
                await output.WriteAsync(header, token).ConfigureAwait(false);
                await WriteIndexTableAsync(output, indexes, token).ConfigureAwait(false);

                var written = checked((long)WiiUFormat.WuxHeaderSize + (long)indexes.Length * sizeof(uint));
                var padding = WiiUFormat.Align(written, chunkSize) - written;
                await WriteZerosAsync(output, padding, token).ConfigureAwait(false);

                await using var temporary = new FileStream(
                    temporaryPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    64 * 1024,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                await temporary.CopyToAsync(output, 256 * 1024, token).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
            catch
            {
                // The conversion result is already committed; a later cleanup can remove this temporary file.
            }
        }
    }

    private static async Task WriteIndexTableAsync(
        Stream output,
        IReadOnlyList<uint> indexes,
        CancellationToken cancellationToken)
    {
        var entriesPerBuffer = 16 * 1024;
        var bytes = new byte[entriesPerBuffer * sizeof(uint)];
        for (var offset = 0; offset < indexes.Count; offset += entriesPerBuffer)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = Math.Min(entriesPerBuffer, indexes.Count - offset);
            var span = bytes.AsSpan(0, count * sizeof(uint));
            for (var index = 0; index < count; index++)
                BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(index * sizeof(uint), sizeof(uint)), indexes[offset + index]);
            await output.WriteAsync(bytes.AsMemory(0, span.Length), cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task WriteZerosAsync(
        Stream output,
        long length,
        CancellationToken cancellationToken)
    {
        var zeros = new byte[64 * 1024];
        while (length > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var count = (int)Math.Min(zeros.Length, length);
            await output.WriteAsync(zeros.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            length -= count;
        }
    }

    private static bool HasCompleteReadableCoverage(BlockMediaImageRepresentation blocks)
    {
        if (blocks.Capacity <= 0 || blocks.Ranges.Count == 0)
            return false;
        long expected = 0;
        foreach (var range in blocks.Ranges.OrderBy(range => range.Address))
        {
            if (range.Address != expected || range.Length <= 0 || range.Kind == MediaDataRangeKind.Unavailable)
                return false;
            expected = checked(expected + range.Length);
        }
        return expected == blocks.Capacity;
    }

    private static string NormalizeExtension(string extension) =>
        extension.StartsWith(".", StringComparison.Ordinal)
            ? extension.ToLowerInvariant()
            : $".{extension.ToLowerInvariant()}";
}
