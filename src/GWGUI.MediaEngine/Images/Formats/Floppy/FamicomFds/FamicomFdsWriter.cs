using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.FamicomFds;

/// <summary>Réécrit les faces Famicom Disk System sans compléter une face absente.</summary>
public sealed class FamicomFdsWriter : IMediaImageWriter
{
    private static readonly IReadOnlySet<string> SupportedFormatIds =
        new[] { DiskImageFormatIds.NintendoFamicomDisk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions =
        new[] { DiskImageFileExtensions.Fds }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations =
        new[] { MediaRepresentationKind.Blocks }.ToFrozenSet();
    private readonly IAtomicImageFileWriter files;

    public FamicomFdsWriter(IAtomicImageFileWriter? files = null) => this.files = files ?? new AtomicImageFileWriter();

    public string Id => MediaImageWriterIds.NintendoFamicomDisk;
    public IReadOnlySet<string> FormatIds => SupportedFormatIds;
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds => SupportedRepresentations;
    public IReadOnlySet<string> ProducedFileExtensions => SupportedExtensions;
    public bool ProducesMultipleFiles => false;

    public bool CanWrite(MediaImageDocument document, string targetFormatId, string targetExtension)
        => SupportedFormatIds.Contains(targetFormatId)
            && SupportedExtensions.Contains(targetExtension)
            && TryGetSides(document, out _, out _);

    public async Task<IReadOnlyList<string>> WriteAsync(
        MediaImageDocument document,
        string outputPath,
        string targetFormatId,
        CancellationToken cancellationToken = default)
    {
        if (!CanWrite(document, targetFormatId, Path.GetExtension(outputPath).ToLowerInvariant())
            || !TryGetSides(document, out var sides, out var headered))
            throw new InvalidDataException("The block document does not contain complete Famicom Disk System sides.");

        await files.WriteAsync(outputPath, (output, token) => WriteFdsAsync(output, document, sides, headered, token), cancellationToken)
            .ConfigureAwait(false);
        return [outputPath];
    }

    private static async Task WriteFdsAsync(
        Stream output,
        MediaImageDocument document,
        IReadOnlyList<MediaDataRange> sides,
        bool headered,
        CancellationToken cancellationToken)
    {
        if (headered)
        {
            var header = new byte[FamicomFdsConstants.HeaderLength];
            await using var input = new FileStream(
                document.Source.PrimaryPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FamicomFdsConstants.HeaderLength,
                FileOptions.Asynchronous | FileOptions.RandomAccess);
            await input.ReadExactlyAsync(header, cancellationToken).ConfigureAwait(false);
            if (!header.AsSpan(0, FamicomFdsConstants.Signature.Length).SequenceEqual(FamicomFdsConstants.Signature))
                throw new InvalidDataException("The source FDS header is missing.");
            await output.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        }

        var buffer = new byte[64 * 1024];
        foreach (var side in sides)
        {
            var completed = 0L;
            while (completed < side.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var count = (int)Math.Min(buffer.Length, side.Length - completed);
                await side.Source!.ReadExactlyAsync(side.SourceOffset + completed, buffer.AsMemory(0, count), cancellationToken)
                    .ConfigureAwait(false);
                await output.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                completed += count;
            }
        }
    }

    private static bool TryGetSides(
        MediaImageDocument document,
        out IReadOnlyList<MediaDataRange> sides,
        out bool headered)
    {
        sides = [];
        headered = false;
        if (document.Representation is not BlockMediaImageRepresentation blocks
            || blocks.Ranges.Count == 0
            || blocks.LogicalBlockSize != FamicomFdsConstants.SideLength
            || blocks.Capacity % FamicomFdsConstants.SideLength != 0)
            return false;

        if (document.Metadata.TryGetValue(FamicomFdsConstants.HeaderLengthMetadata, out var headerLengthText)
            && int.TryParse(headerLengthText, NumberStyles.None, CultureInfo.InvariantCulture, out var headerLength))
        {
            if (headerLength is not 0 and not FamicomFdsConstants.HeaderLength) return false;
            headered = headerLength == FamicomFdsConstants.HeaderLength;
        }

        var ordered = blocks.Ranges.OrderBy(range => range.Address).ToArray();
        var sideCount = checked((int)(blocks.Capacity / FamicomFdsConstants.SideLength));
        if (ordered.Length != sideCount) return false;
        for (var side = 0; side < ordered.Length; side++)
        {
            var range = ordered[side];
            if (range.Kind != MediaDataRangeKind.Stored
                || range.Source is null
                || range.Address != side * (long)FamicomFdsConstants.SideLength
                || range.Length != FamicomFdsConstants.SideLength)
                return false;
        }
        sides = ordered;
        return true;
    }
}
