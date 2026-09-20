using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaFileSystems.FileSystems.Acorn.BbcDfs;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Reconstruction.Sectors;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.AcornAtom;

/// <summary>Lit les images brutes Acorn Atom DOS de 40 pistes.</summary>
public sealed class AcornAtomDskReader : IMediaImageReader
{
    internal const int BlockSize = 256;
    internal const int Cylinders = 40;
    internal const int Heads = 1;
    internal const int SectorsPerTrack = 10;
    internal const int Capacity = BlockSize * Cylinders * Heads * SectorsPerTrack;

    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.AcornAtomDos }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Dsk }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentations = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public AcornAtomDskReader() : this(File.ReadAllBytesAsync) { }

    internal AcornAtomDskReader(Func<string, CancellationToken, Task<byte[]>> readBytes) =>
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentations;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (!SupportedExtensions.Contains(context.Extension) || context.Length != Capacity) return false;
        var catalog = await context.ReadAsync(0, BlockSize * 2, cancellationToken).ConfigureAwait(false);
        return LooksLikeAtomDos(catalog.Span);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(data.ToArray(), cancellationToken));
    }

    internal static bool LooksLikeAtomDos(ReadOnlySpan<byte> catalog)
    {
        if (catalog.Length < BlockSize * 2) return false;
        var metadata = catalog[BlockSize..];
        var entryBytes = metadata[BbcDfsFileSystemLayout.EntryCountOffset];
        var totalSectors = ((metadata[BbcDfsFileSystemLayout.TotalSectorsHighOffset] & BbcDfsFileSystemLayout.TotalSectorsHighMask) << 8)
            | metadata[BbcDfsFileSystemLayout.TotalSectorsLowOffset];
        return entryBytes % BbcDfsFileSystemLayout.EntryPartSize == 0
            && entryBytes <= BbcDfsFileSystemLayout.MaximumEntryCount * BbcDfsFileSystemLayout.EntryPartSize
            && ((totalSectors == 0 && entryBytes > 0) || totalSectors == Cylinders * Heads * SectorsPerTrack);
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
    {
        if (data.Length != Capacity || !LooksLikeAtomDos(data))
            throw new InvalidDataException($"The {data.Length}-byte DSK does not contain a valid Acorn Atom DOS catalogue.");
        var geometry = new LinearSectorImageGeometry(BlockSize, Cylinders, Heads, SectorsPerTrack);
        return LinearSectorImageBuilder.Create(data, DiskImageFormatIds.AcornAtomDos, geometry, cancellationToken);
    }
}
