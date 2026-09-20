using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Rx02;

/// <summary>Lit un dump DEC RX02 en ordre physique et produit les blocs logiques RT-11.</summary>
public sealed class DecRx02Reader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.DecRx02 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Img }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    /// <summary>Lit un dump RX02 depuis son chemin.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    /// <summary>Lit un dump RX02 déjà chargé en mémoire.</summary>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(Read(bytes.Span, cancellationToken));

    ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestedFormatMatches = context.RequestedFormatId is null || SupportedFormatIds.Contains(context.RequestedFormatId);
        return ValueTask.FromResult(requestedFormatMatches && context.Length == DecRx02Geometry.Capacity);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var image = await ReadAsync(bytes, cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, image);
    }

    /// <summary>Valide la capacité puis remet les secteurs du dump en ordre logique.</summary>
    private static SectorImage Read(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        if (bytes.Length != DecRx02Geometry.Capacity) throw DecRx02Exceptions.IncompleteImage(bytes.Length, DecRx02Geometry.Capacity);
        var logicalSectors = ReadLogicalSectors(bytes, cancellationToken);
        return AssembleLogicalBlocks(logicalSectors, cancellationToken);
    }

    /// <summary>Replace les secteurs physiques de 256 octets dans leur ordre logique.</summary>
    private static byte[][] ReadLogicalSectors(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        var sectors = new byte[DecRx02Geometry.PhysicalSectorCount][];
        for (var logicalSector = 0; logicalSector < sectors.Length; logicalSector++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sectors[logicalSector] = new byte[DecRx02Geometry.PhysicalSectorSize];
            DecRx02SectorOrder.CopyLogicalSector(bytes, logicalSector, sectors[logicalSector]);
        }
        return sectors;
    }

    /// <summary>Assemble chaque paire de secteurs physiques consécutifs en bloc logique RT-11 de 512 octets.</summary>
    private static SectorImage AssembleLogicalBlocks(IReadOnlyList<byte[]> logicalSectors, CancellationToken cancellationToken)
    {
        var blocks = new SectorBlock[DecRx02Geometry.LogicalBlockCount];
        for (var block = 0; block < blocks.Length; block++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var data = new byte[DecRx02Geometry.LogicalBlockSize];
            for (var part = 0; part < DecRx02Geometry.PhysicalSectorsPerLogicalBlock; part++) logicalSectors[block * DecRx02Geometry.PhysicalSectorsPerLogicalBlock + part].CopyTo(data, part * DecRx02Geometry.PhysicalSectorSize);
            blocks[block] = new(block, new(block / DecRx02Geometry.LogicalBlocksPerTrack, DecRx02Geometry.FirstHead, block % DecRx02Geometry.LogicalBlocksPerTrack + DecRx02Geometry.FirstLogicalSectorNumber), data, true);
        }
        return new(DiskImageFormatIds.DecRx02, DecRx02Geometry.LogicalBlockSize, DecRx02Geometry.TrackCount, DecRx02Geometry.HeadCount, DecRx02Geometry.LogicalBlocksPerTrack, blocks, capacity: DecRx02Geometry.Capacity, logicalBlockCount: DecRx02Geometry.LogicalBlockCount);
    }
}
