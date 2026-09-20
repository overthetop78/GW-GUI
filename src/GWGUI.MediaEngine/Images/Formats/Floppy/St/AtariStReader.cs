
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Reconstruction.Sectors;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.St;

/// <summary>Lit une image Atari ST brute et la construit avec des secteurs numérotés à partir de un.</summary>
public sealed class AtariStReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[]
    {
        DiskImageFormatIds.AtariSt180,
        DiskImageFormatIds.AtariSt360,
        DiskImageFormatIds.AtariSt400,
        DiskImageFormatIds.AtariSt440,
        DiskImageFormatIds.AtariSt720,
        DiskImageFormatIds.AtariSt800,
        DiskImageFormatIds.AtariSt810,
        DiskImageFormatIds.AtariSt880,
        DiskImageFormatIds.AtariSt1440
    }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.St }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public AtariStReader() : this(File.ReadAllBytesAsync) { }

    internal AtariStReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
    {
        this.readBytes = readBytes ?? throw new ArgumentNullException(nameof(readBytes));
    }

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>Charge, détecte et valide exactement la géométrie de l'image.</summary>
    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        var data = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(data, cancellationToken);
    }

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !((IMediaImageReader)this).SupportsFormatId(context.RequestedFormatId)) return false;
        if (context.Length == 0 || context.Length > int.MaxValue || context.Length % AtariStGeometry.SectorSize != 0) return false;
        try
        {
            var data = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
            _ = AtariStGeometryDetector.Detect(data.Span);
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
    {
        if (data.Length == 0 || data.Length % AtariStGeometry.SectorSize != 0) throw AtariStExceptions.InvalidLength(data.Length, AtariStGeometry.SectorSize);
        var detection = AtariStGeometryDetector.Detect(data);
        var geometry = detection.Geometry;
        if (data.Length != geometry.Capacity) throw AtariStExceptions.IncompatibleGeometry(data.Length, geometry.Capacity, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack);
        var linear = new LinearSectorImageGeometry(AtariStGeometry.SectorSize, geometry.Cylinders, geometry.Heads, geometry.SectorsPerTrack, SectorNumbering.OneBased);
        return LinearSectorImageBuilder.Create(data, geometry.FormatId, linear, cancellationToken);
    }
}
