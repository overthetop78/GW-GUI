using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.SpectrumTap;

/// <summary>Writes only complete checksum-valid Spectrum TAP blocks.</summary>
public sealed class SpectrumTapWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.SpectrumTap }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public SpectrumTapWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();
    public string Id => MediaImageWriterIds.TapeSpectrumTap;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SpectrumTapFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        SupportedFormats.Contains(targetFormatId)
        && SpectrumTapFormat.Extensions.Contains(targetExtension)
        && document.MediaKind == MediaKind.Tape
        && TryGetBlocks(document, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath).ToLowerInvariant())
            || !TryGetBlocks(document, out var blocks))
            throw new InvalidDataException("Spectrum TAP output requires complete checksum-valid blocks.");
        await files.WriteAsync(outputPath, async (output, token) =>
        {
            foreach (var range in blocks)
            {
                var length = new byte[SpectrumTapConstants.LengthFieldSize];
                BinaryPrimitives.WriteUInt16LittleEndian(length, checked((ushort)range.Length));
                await output.WriteAsync(length, token).ConfigureAwait(false);
                await CopyAsync(range, output, token).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool TryGetBlocks(MediaImageDocument document, out IReadOnlyList<MediaDataRange> blocks)
    {
        blocks = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { Count: > 0 } segments }) return false;
        var selected = new List<MediaDataRange>();
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (!segment.Metadata.ContainsKey(SpectrumTapConstants.BlockMetadataKey)
                || !segment.Metadata.TryGetValue("checksumValid", out var validText)
                || !bool.TryParse(validText, out var valid) || !valid
                || segment.DataRange is not { Kind: MediaDataRangeKind.Stored, Source: not null } range
                || range.Length is < 2 or > ushort.MaxValue)
                return false;
            selected.Add(range);
        }
        blocks = selected;
        return true;
    }

    private static async Task CopyAsync(MediaDataRange range, Stream output, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        var completed = 0L;
        while (completed < range.Length)
        {
            var count = (int)Math.Min(buffer.Length, range.Length - completed);
            await range.Source!.ReadExactlyAsync(range.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            completed += count;
        }
    }
}
