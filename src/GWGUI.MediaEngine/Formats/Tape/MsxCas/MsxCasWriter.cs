using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;

using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.MediaEngine.Formats.Tape.MsxCas;

/// <summary>Writes retained MSX CAS separators and byte groups without inventing tape timing.</summary>
public sealed class MsxCasWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { TapeImageFormatIds.MsxCas }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds =
        new[] { MediaRepresentationKind.Sequential }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public MsxCasWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();
    public string Id => MediaImageWriterIds.TapeMsxCas;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentationKinds;
    public IReadOnlySet<string> ProducedFileExtensions => MsxCasFormat.Extensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension) =>
        SupportedFormatIds.Contains(targetFormatId)
        && MsxCasFormat.Extensions.Contains(targetExtension)
        && document.MediaKind == MediaKind.Tape
        && TryGetParts(document, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(outputPath).ToLowerInvariant();
        if (!CanWrite(document, targetFormatId, extension) || !TryGetParts(document, out var parts))
            throw new InvalidDataException("The sequential document cannot be written as MSX CAS without losing byte groups.");

        await files.WriteAsync(
            outputPath,
            async (output, token) =>
            {
                foreach (var part in parts)
                {
                    if (part.IsSeparator)
                    {
                        await output.WriteAsync(MsxCasConstants.Separator.ToArray(), token).ConfigureAwait(false);
                    }
                    else
                    {
                        await CopyAsync(part.Range!, output, token).ConfigureAwait(false);
                    }
                }
            },
            cancellationToken).ConfigureAwait(false);
        return [outputPath];
    }

    private static bool TryGetParts(MediaImageDocument document, out IReadOnlyList<CasPart> parts)
    {
        parts = [];
        if (document.Representation is not SequentialMediaImageRepresentation { Segments: { Count: > 0 } segments }) return false;
        var selected = new List<CasPart>(segments.Count);
        var separatorCount = 0;
        foreach (var segment in segments.OrderBy(segment => segment.Position))
        {
            if (segment.Kind == SequentialSegmentKind.TapeMark
                && segment.Metadata.ContainsKey(MsxCasConstants.SeparatorMetadataKey))
            {
                selected.Add(new CasPart(true, null));
                separatorCount++;
                continue;
            }
            if (segment.DataRange is not { Kind: MediaDataRangeKind.Stored, Source: not null } range) return false;
            selected.Add(new CasPart(false, range));
        }
        if (separatorCount == 0) return false;
        parts = selected;
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

    private sealed record CasPart(bool IsSeparator, MediaDataRange? Range);
}
