
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Reconstruction.Sectors;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Raw;

/// <summary>Lit une image sectorielle brute IBM IMG ou IMA.</summary>
public sealed class IbmRawImageReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = IbmPcGeometryCatalog.All.Select(geometry => geometry.FormatId).ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Ima, DiskImageFileExtensions.Img }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public IbmRawImageReader() : this(File.ReadAllBytesAsync) { }

    internal IbmRawImageReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>Lit le fichier, dÃ©tecte sa gÃ©omÃ©trie puis appelle le constructeur sectoriel IBM commun.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !((IMediaImageReader)this).SupportsFormatId(context.RequestedFormatId)) return false;
        try
        {
            var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
            _ = IbmRawImageGeometryDetector.Detect(data.Span);
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(data.ToArray(), cancellationToken));
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
        => IbmRawSectorImageBuilder.Create(data, IbmRawImageGeometryDetector.Detect(data), cancellationToken);
}
