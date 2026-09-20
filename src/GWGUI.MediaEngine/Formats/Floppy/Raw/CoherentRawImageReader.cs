using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaFileSystems.FileSystems.Coherent;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Reconstruction.Sectors;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Raw;

/// <summary>Lit les dumps sectoriels bruts des disquettes COHERENT, notamment ceux du Commodore 900.</summary>
public sealed class CoherentRawImageReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.Commodore900Coherent }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Bin, DiskImageFileExtensions.Img }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    /// <summary>Lit puis valide un dump COHERENT depuis son chemin.</summary>
    /// <param name="path">Chemin du dump à lire.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la lecture.</param>
    /// <returns>Image sectorielle reconstruite selon la géométrie du Commodore 900.</returns>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    /// <summary>Valide et reconstruit un dump COHERENT déjà chargé en mémoire.</summary>
    /// <param name="bytes">Contenu intégral du dump en lecture seule.</param>
    /// <param name="cancellationToken">Jeton permettant d'annuler la reconstruction.</param>
    /// <returns>Image sectorielle reconstruite selon la géométrie du Commodore 900.</returns>
    public Task<SectorImage> ReadAsync(ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default) => Task.FromResult(Read(bytes.Span, cancellationToken));

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length > Commodore900Geometry.Capacity || context.Length % Commodore900Geometry.SectorSize != 0) return false;
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return CoherentFormat.LooksLikeCoherent(bytes.Span);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var image = await ReadAsync(bytes, cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, image);
    }

    /// <summary>Valide le superbloc et reconstruit les blocs sectoriels du dump.</summary>
    private static SectorImage Read(ReadOnlySpan<byte> bytes, CancellationToken cancellationToken)
    {
        if (!CoherentFormat.LooksLikeCoherent(bytes)) throw CoherentRawImageExceptions.ContentNotCoherent(bytes.Length);
        if (bytes.Length % Commodore900Geometry.SectorSize != 0) throw CoherentRawImageExceptions.NonSectorAlignedLength(bytes.Length, Commodore900Geometry.SectorSize);
        var availableBlocks = bytes.Length / Commodore900Geometry.SectorSize;
        var declaredBlocks = CoherentFormat.ReadDeclaredFileSystemBlockCount(bytes);
        if (declaredBlocks < 3 || declaredBlocks > availableBlocks) throw CoherentRawImageExceptions.InvalidDeclaredBlockCount(declaredBlocks, availableBlocks);
        if (availableBlocks > Commodore900Geometry.BlockCount) throw CoherentRawImageExceptions.GeometryCapacityExceeded(availableBlocks, Commodore900Geometry.BlockCount);
        return Commodore900SectorImageBuilder.Create(bytes, cancellationToken);
    }
}
