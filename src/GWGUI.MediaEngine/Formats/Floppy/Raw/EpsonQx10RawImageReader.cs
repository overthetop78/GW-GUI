
using System.Collections.Frozen;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Reading;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Formats.Floppy.Raw;

/// <summary>Relit une image Epson QX-10 brute selon le profil explicitement sélectionné.</summary>
public sealed class EpsonQx10RawImageReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = EpsonQx10GeometryCatalog.All.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Img }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();
    private readonly Func<string, CancellationToken, Task<byte[]>> readBytes;

    public EpsonQx10RawImageReader() : this(File.ReadAllBytesAsync) { }

    internal EpsonQx10RawImageReader(Func<string, CancellationToken, Task<byte[]>> readBytes)
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

    /// <summary>Découpe les octets selon la géométrie Epson demandée.</summary>
    public async Task<SectorImage> ReadAsync(string path, string formatId, CancellationToken cancellationToken = default)
    {
        var bytes = await readBytes(path, cancellationToken).ConfigureAwait(false);
        return Read(bytes, formatId, cancellationToken);
    }

    ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.RequestedFormatId is null || !EpsonQx10GeometryCatalog.All.TryGetValue(context.RequestedFormatId, out var geometry)) return ValueTask.FromResult(false);
        var expectedLength = geometry.AllTracks.Sum(track => (long)track.Count * track.SectorSize);
        return ValueTask.FromResult(context.Length == expectedLength);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is null) throw new NotSupportedException("An Epson QX-10 raw image requires an explicit format identifier.");
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(bytes.ToArray(), context.RequestedFormatId, cancellationToken));
    }

    private static SectorImage Read(byte[] bytes, string formatId, CancellationToken cancellationToken)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        var expectedLength = geometry.AllTracks.Sum(track => track.Count * track.SectorSize);
        if (bytes.Length != expectedLength) throw new InvalidDataException($"Epson image length is {bytes.Length}; expected {expectedLength} bytes for '{formatId}'.");
        var blocks = new List<SectorBlock>();
        var offset = 0;
        var maximumSectors = 0;
        var sizes = new HashSet<int>();
        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                maximumSectors = Math.Max(maximumSectors, track.Count);
                sizes.Add(track.SectorSize);
                for (var index = 0; index < track.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var data = bytes.AsSpan(offset, track.SectorSize).ToArray();
                    blocks.Add(new(blocks.Count, new(cylinder, head, track.FirstSector + index), data));
                    offset += track.SectorSize;
                }
            }
        }
        var blockSize = sizes.OrderByDescending(size => geometry.AllTracks.Where(track => track.SectorSize == size).Sum(track => track.Count)).First();
        return new(formatId, blockSize, geometry.Cylinders, geometry.Heads, maximumSectors, blocks, sizes.Count > 1, bytes.Length, blocks.Count);
    }
}
