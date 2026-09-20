using GWGUI.MediaFileSystems.Formats.Commodore;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Reconstruction.Sectors;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.D81;

/// <summary>Lit un conteneur Commodore D81 dans sa vue logique de 3 200 blocs.</summary>
public sealed class D81Reader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.Commodore1581 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.D81 }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public D81Reader() : this(File.ReadAllBytesAsync) { }

    internal D81Reader(Func<string, CancellationToken, Task<byte[]>> readBytes)
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

    /// <summary>Lit un D81 de longueur exacte et reconstruit ses blocs logiques.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestedFormatMatches = context.RequestedFormatId is null || SupportedFormatIds.Contains(context.RequestedFormatId);
        return ValueTask.FromResult(requestedFormatMatches && context.Length == D81Layout.ImageLength);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(data.ToArray(), cancellationToken));
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
    {
        if (data.Length != D81Layout.ImageLength) throw D81Exceptions.InvalidLength(data.Length, D81Layout.ImageLength);
        var geometry = new LinearSectorImageGeometry(Commodore1581Geometry.LogicalBlockSize, Commodore1581Geometry.LogicalCylinderCount, Commodore1581Geometry.LogicalHeadCount, Commodore1581Geometry.LogicalBlocksPerTrack, SectorNumbering.ZeroBased);
        return LinearSectorImageBuilder.Create(data, DiskImageFormatIds.Commodore1581, geometry, cancellationToken);
    }
}
