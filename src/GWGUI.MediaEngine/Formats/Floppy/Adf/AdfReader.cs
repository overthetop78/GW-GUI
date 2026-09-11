using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Reconstruction;
using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.MediaEngine.Recognition;

namespace GWGUI.MediaEngine.Formats.Floppy.Adf;

/// <summary>Lit les conteneurs ADF Acorn et Amiga dont la géométrie est déterminée par la taille exacte.</summary>
public sealed class AdfReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[]
    {
        DiskImageFormatIds.AcornAdfs800,
        DiskImageFormatIds.AmigaDos,
        DiskImageFormatIds.AmigaDosHighDensity
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[]
    {
        DiskImageFileExtensions.Adf
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[]
    {
        MediaKind.Floppy
    }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[]
    {
        MediaRepresentationKind.Sectors
    }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public AdfReader() : this(File.ReadAllBytesAsync) { }

    internal AdfReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    private static readonly int[] AcceptedSizes = [AcornAdfGeometry.Capacity, AcornAdfGeometry.PaddedCapacity, AmigaAdfGeometry.DoubleDensityCapacity, AmigaAdfGeometry.HighDensityCapacity];

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;

    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;

    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];

    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;

    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;

    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;

    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    /// <summary>Lit le fichier et reconstruit ses secteurs avec une numérotation commençant à zéro.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestedFormatMatches = context.RequestedFormatId is null || SupportedFormatIds.Contains(context.RequestedFormatId);
        var lengthMatches = context.Length <= int.MaxValue && AcceptedSizes.Contains((int)context.Length);
        return ValueTask.FromResult(requestedFormatMatches && lengthMatches);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        var image = Read(data.ToArray(), cancellationToken);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, image);
    }

    private static SectorImage Read(byte[] data, CancellationToken cancellationToken)
    {
        if (data.Length == AcornAdfGeometry.Capacity) return RegularSectorImageBuilder.Create(data, AcornAdfGeometry.Geometry, cancellationToken);
        if (data.Length == AcornAdfGeometry.PaddedCapacity) return RegularSectorImageBuilder.Create(data, AcornAdfGeometry.Geometry, cancellationToken, AcornAdfGeometry.PaddedTrailingByteCount);
        if (data.Length == AmigaAdfGeometry.DoubleDensity.Capacity) return RegularSectorImageBuilder.Create(data, AmigaAdfGeometry.DoubleDensity, cancellationToken);
        if (data.Length == AmigaAdfGeometry.HighDensity.Capacity) return RegularSectorImageBuilder.Create(data, AmigaAdfGeometry.HighDensity, cancellationToken);
        throw AdfExceptions.InvalidSize(data.Length, AcceptedSizes);
    }
}
