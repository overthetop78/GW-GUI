using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.FileSystems.Cpm;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Reconstruction.Sectors;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Raw;

/// <summary>Lit les images IMG ambiguës et départage les interprétations IBM, Amstrad CPC et Amstrad PCW prises en charge.</summary>
internal sealed class RawImgReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = IbmPcGeometryCatalog.All
        .Select(geometry => geometry.FormatId)
        .Append(DiskImageFormatIds.AmstradCpc)
        .Append(DiskImageFormatIds.AmstradPcw)
        .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Img }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public RawImgReader() : this(File.ReadAllBytesAsync) { }

    internal RawImgReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId)
        => SupportedFormatIds.Contains(formatId) || formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit le fichier IMG puis conserve ou réidentifie l'image construite par le Reader IBM selon son contenu.</summary>
    /// <param name="path">Chemin de l'image IMG brute.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture et la construction.</param>
    /// <returns>Image sectorielle IBM, Amstrad CPC ou Amstrad PCW validée.</returns>
    /// <exception cref="InvalidDataException">La géométrie ne peut pas être déterminée par le Reader IBM.</exception>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var bytes = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(bytes, cancellationToken);
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !((IMediaImageReader)this).SupportsFormatId(context.RequestedFormatId)) return false;
        try
        {
            var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
            _ = Read(bytes.ToArray(), cancellationToken);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(bytes.ToArray(), cancellationToken));
    }

    private static SectorImage Read(byte[] bytes, CancellationToken cancellationToken)
    {
        var hasFatBpb = FatBpbGeometryDetector.TryDetect(bytes, bytes.Length, out _);
        var geometry = IbmRawImageGeometryDetector.Detect(bytes);
        var image = IbmRawSectorImageBuilder.Create(bytes, geometry, cancellationToken);
        if (!hasFatBpb)
        {
            var logical = CpmDirectoryReader.Flatten(image);
            if (CpmDirectoryReader.FindDirectory(logical, AmstradCpmLayout.CpcSystem, AmstradCpmLayout.CpcSectorSize, allowEmpty: false, rejectLowercase: false) is not null) return image.WithFormatId(DiskImageFormatIds.AmstradCpc);
        }
        if (!hasFatBpb && AmstradCpmDiskSpecification.TryParse(bytes, out _)) return image.WithFormatId(DiskImageFormatIds.AmstradPcw);
        return image;
    }
}
