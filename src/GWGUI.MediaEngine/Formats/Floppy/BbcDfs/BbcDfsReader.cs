using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Reconstruction.Sectors;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.BbcDfs;

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

    /// <summary>Charge l'image, sélectionne SSD ou DSD depuis l'extension et exige une capacité exacte de 40 ou 80 cylindres.</summary>
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
        var matches = context.Length <= int.MaxValue && BbcDfsGeometry.Find(heads, (int)context.Length) is not null;
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
        if (data.Length == 0 || data.Length % (BbcDfsGeometry.TrackSize * heads) != 0) throw BbcDfsExceptions.IncompleteTrack(data.Length, heads, BbcDfsGeometry.TrackSize);
        var cylinders = data.Length / (BbcDfsGeometry.TrackSize * heads);
        var geometry = BbcDfsGeometry.Find(heads, data.Length) ?? throw BbcDfsExceptions.UnsupportedCylinderCount(data.Length, cylinders, heads);
        var linear = new LinearSectorImageGeometry(BbcDfsGeometry.SectorSize, geometry.Cylinders, geometry.Heads, BbcDfsGeometry.SectorsPerTrack, SectorNumbering.ZeroBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }

    private static BbcDfsContainerKind? GetContainerKind(string extension)
        => extension.Equals(DiskImageFileExtensions.Ssd, StringComparison.OrdinalIgnoreCase)
            ? BbcDfsContainerKind.Ssd
            : extension.Equals(DiskImageFileExtensions.Dsd, StringComparison.OrdinalIgnoreCase)
                ? BbcDfsContainerKind.Dsd
                : null;
}
