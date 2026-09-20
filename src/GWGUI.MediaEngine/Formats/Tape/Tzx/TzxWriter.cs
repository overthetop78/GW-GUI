using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.Tzx;

/// <summary>Writes retained TZX blocks verbatim after validating that every payload remains accessible.</summary>
public sealed class TzxWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormats =
        new[] { TapeImageFormatIds.Tzx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public TzxWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.TapeTzx;
    public IReadOnlySet<string> FormatIds => SupportedFormats;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => TzxFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
    {
        ArgumentNullException.ThrowIfNull(document);
        return SupportedFormats.Contains(targetFormatId)
            && TzxFormat.Extensions.Contains(targetExtension)
            && document.MediaKind == MediaKind.Tape
            && TryGetBlocks(document, out _);
    }

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension) || !TryGetBlocks(document, out var blocks))
            throw new InvalidDataException("The sequential document cannot be written as TZX without losing block payloads.");
        var major = ReadVersion(document.Metadata, "majorVersion", TzxConstants.MajorVersion);
        var minor = ReadVersion(document.Metadata, "minorVersion", TzxConstants.MinorVersion);
        if (major != TzxConstants.MajorVersion || minor > TzxConstants.MinorVersion)
            throw new NotSupportedException($"TZX version {major}.{minor:D2} is outside the supported 1.20 profile.");

        await files.WriteAsync(outputPath, async (output, token) =>
        {
            var header = new byte[TzxConstants.HeaderSize];
            System.Text.Encoding.ASCII.GetBytes(TzxConstants.Signature, header);
            header[7] = TzxConstants.EndOfTextMarker;
            header[8] = major;
            header[9] = minor;
            await output.WriteAsync(header, token).ConfigureAwait(false);
            foreach (var block in blocks)
            {
                await output.WriteAsync(new[] { block.Id }, token).ConfigureAwait(false);
                if (block.Range is not null) await CopyAsync(block.Range, output, token).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool TryGetBlocks(MediaImageDocument document, out IReadOnlyList<TzxBlock> blocks)
    {
        blocks = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { Count: > 0 } segments }) return false;
        var selected = new List<TzxBlock>(segments.Count);
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (!segment.Metadata.TryGetValue(TzxConstants.BlockIdMetadataKey, out var idText)
                || !byte.TryParse(idText, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var id))
                return false;
            if (segment.Length is null)
            {
                if (segment.DataRange is not null) return false;
                selected.Add(new TzxBlock(id, null));
                continue;
            }
            if (segment.DataRange is not { Kind: MediaDataRangeKind.Stored, Source: not null } range
                || range.Length != segment.Length)
                return false;
            selected.Add(new TzxBlock(id, range));
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

    private static byte ReadVersion(IReadOnlyDictionary<string, string> metadata, string key, byte defaultValue) =>
        metadata.TryGetValue(key, out var text)
        && byte.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var value)
            ? value
            : defaultValue;

    private sealed record TzxBlock(byte Id, MediaDataRange? Range);
}
