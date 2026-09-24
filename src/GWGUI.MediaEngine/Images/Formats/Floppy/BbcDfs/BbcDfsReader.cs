using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.BbcDfs;

/// <summary>Lit les images BBC DFS SSD et DSD dans leur ordre cylindre, face et secteur à base zéro.</summary>
public sealed class BbcDfsReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = BbcDfsGeometry.Supported.Select(geometry => geometry.FormatId).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Ssd, DiskImageFileExtensions.Dsd }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public BbcDfsReader() : this(File.ReadAllBytesAsync) { }

    internal BbcDfsReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;

    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;

    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];

    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;

    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;

    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    /// <summary>Charge l'image et sélectionne SSD ou DSD depuis l'extension, y compris une image tronquée après son dernier secteur complet.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(path);
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, extension, cancellationToken);
    }

    ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return ValueTask.FromResult(false);
        var kind = GetContainerKind(context.Extension);
        if (kind is null) return ValueTask.FromResult(false);
        var heads = kind == BbcDfsContainerKind.Ssd ? DiskGeometryConstants.SingleSidedHeadCount : DiskGeometryConstants.DoubleSidedHeadCount;
        var maximumCapacity = BbcDfsGeometry.Supported.Where(geometry => geometry.Heads == heads).Max(geometry => geometry.Capacity);
        var matches = context.Length >= BbcDfsGeometry.SectorSize
            && context.Length <= maximumCapacity + BbcDfsGeometry.SectorSize - 1;
        return ValueTask.FromResult(matches);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var image = Read(data.ToArray(), context.Extension, cancellationToken);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, image);
    }

    private static SectorImage Read(byte[] data, string extension, CancellationToken cancellationToken)
    {
        var kind = GetContainerKind(extension) ?? throw BbcDfsExceptions.UnknownExtension(extension);
        var heads = kind == BbcDfsContainerKind.Ssd ? DiskGeometryConstants.SingleSidedHeadCount : DiskGeometryConstants.DoubleSidedHeadCount;
        var completeLength = data.Length / BbcDfsGeometry.SectorSize * BbcDfsGeometry.SectorSize;
        if (completeLength == 0)
            throw BbcDfsExceptions.IncompleteTrack(data.Length, heads, BbcDfsGeometry.SectorSize);
        var geometry = BbcDfsGeometry.Supported.Where(candidate => candidate.Heads == heads && candidate.Capacity >= completeLength)
            .OrderBy(candidate => candidate.Capacity).FirstOrDefault()
            ?? throw BbcDfsExceptions.UnsupportedCylinderCount(data.Length, (int)Math.Ceiling(data.Length / (double)(BbcDfsGeometry.TrackSize * heads)), heads);
        var blocks = new SectorBlock[completeLength / BbcDfsGeometry.SectorSize];
        for (var logical = 0; logical < blocks.Length; logical++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var perCylinder = geometry.Heads * BbcDfsGeometry.SectorsPerTrack;
            var address = new SectorAddress(logical / perCylinder, logical / BbcDfsGeometry.SectorsPerTrack % geometry.Heads, logical % BbcDfsGeometry.SectorsPerTrack);
            blocks[logical] = new(logical, address, data.AsSpan(logical * BbcDfsGeometry.SectorSize, BbcDfsGeometry.SectorSize).ToArray());
        }
        return new SectorImage(geometry.FormatId, BbcDfsGeometry.SectorSize, geometry.Cylinders, geometry.Heads, BbcDfsGeometry.SectorsPerTrack, blocks);
    }

    private static BbcDfsContainerKind? GetContainerKind(string extension)
        => extension.Equals(DiskImageFileExtensions.Ssd, StringComparison.OrdinalIgnoreCase)
            ? BbcDfsContainerKind.Ssd
            : extension.Equals(DiskImageFileExtensions.Dsd, StringComparison.OrdinalIgnoreCase)
                ? BbcDfsContainerKind.Dsd
                : null;
}
