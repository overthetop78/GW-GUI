using System.Buffers.Binary;
using System.Collections.Frozen;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.Atx;

/// <summary>Lit les pistes ATX et expose leur meilleure occurrence de chaque secteur logique Atari.</summary>
public sealed class AtxReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.AtariAtx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Atx }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures = [AtxFormat.CreateSignature()];
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Sectors }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => FrozenSet<string>.Empty;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    public async Task<SectorImage> ReadAsync(string path, CancellationToken cancellationToken = default)
        => Read(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), cancellationToken);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (context.Length < AtxLayout.FileHeaderSize) return false;
        var signature = await context.ReadHeaderAsync(AtxFormat.SignatureLength, cancellationToken).ConfigureAwait(false);
        return BinaryPrimitives.ReadUInt32LittleEndian(signature.Span) == AtxFormat.Signature;
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var bytes = await context.ReadBytesAsync(cancellationToken).ConfigureAwait(false);
        return MediaImageDocumentFactory.CreateFloppySector(context.Source, Read(bytes.ToArray(), cancellationToken));
    }

    internal static SectorImage Read(byte[] data, CancellationToken cancellationToken = default)
    {
        if (data.Length < AtxLayout.FileHeaderSize || BinaryPrimitives.ReadUInt32LittleEndian(data) != AtxFormat.Signature)
            throw new InvalidDataException("The ATX file header is missing or invalid.");

        var trackOffset = ReadOffset(data, AtxLayout.TrackDataOffset, "track data");
        if (trackOffset < AtxLayout.FileHeaderSize || trackOffset > data.Length - AtxLayout.TrackHeaderSize)
            throw new InvalidDataException("The ATX track-data offset is outside the file.");

        var candidates = new Dictionary<(int Track, int Sector), List<SectorCandidate>>();
        var visited = new HashSet<int>();
        while (trackOffset > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!visited.Add(trackOffset)) throw new InvalidDataException("The ATX track chain contains a cycle.");
            EnsureRange(data, trackOffset, AtxLayout.TrackHeaderSize, "track header");
            var relativeNext = ReadUInt32(data, trackOffset + AtxLayout.TrackNextOffset);
            var trackEnd = relativeNext == 0 ? data.Length : checked(trackOffset + ToInt(relativeNext, "next track"));
            if (trackEnd <= trackOffset || trackEnd > data.Length) throw new InvalidDataException("An ATX track extends outside the file.");

            var trackType = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(trackOffset + AtxLayout.TrackTypeOffset));
            var trackNumber = data[trackOffset + AtxLayout.TrackNumberOffset];
            var sectorCount = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(trackOffset + AtxLayout.TrackSectorCountOffset));
            if (trackNumber >= AtxLayout.TrackCount) throw new InvalidDataException($"ATX track {trackNumber} is outside the supported 40-track geometry.");
            if (sectorCount > AtxLayout.MaximumSectorRecordsPerTrack) throw new InvalidDataException($"ATX track {trackNumber} contains too many sector records.");

            if (trackType == 0 && sectorCount > 0)
            {
                var sectorListOffset = checked(trackOffset + ReadOffset(data, trackOffset + AtxLayout.TrackDataRelativeOffset, "sector list"));
                var headersLength = checked(AtxLayout.SectorListHeaderSize + sectorCount * AtxLayout.SectorHeaderSize);
                EnsureRange(data, sectorListOffset, headersLength, "sector list");
                if (sectorListOffset + headersLength > trackEnd) throw new InvalidDataException($"The sector list of ATX track {trackNumber} crosses the track boundary.");

                for (var index = 0; index < sectorCount; index++)
                {
                    var header = sectorListOffset + AtxLayout.SectorListHeaderSize + index * AtxLayout.SectorHeaderSize;
                    var sectorNumber = data[header + AtxLayout.SectorNumberOffset];
                    if (sectorNumber is < 1 or > AtxLayout.MaximumSectorsPerTrack) throw new InvalidDataException($"ATX track {trackNumber} contains invalid sector {sectorNumber}.");
                    var status = data[header + AtxLayout.SectorStatusOffset];
                    var position = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(header + AtxLayout.SectorPositionOffset));
                    if ((status & AtxLayout.SectorStatusMissingData) != 0) continue;
                    var dataOffset = checked(trackOffset + ReadOffset(data, header + AtxLayout.SectorDataRelativeOffset, "sector data"));
                    EnsureRange(data, dataOffset, AtxLayout.SectorSize, "sector data");
                    if (dataOffset + AtxLayout.SectorSize > trackEnd) throw new InvalidDataException($"The data of ATX track {trackNumber}, sector {sectorNumber}, crosses the track boundary.");
                    var address = ((int)trackNumber, (int)sectorNumber);
                    if (!candidates.TryGetValue(address, out var list)) candidates[address] = list = [];
                    list.Add(new(status, position, data.AsSpan(dataOffset, AtxLayout.SectorSize).ToArray()));
                }
            }

            if (relativeNext == 0 || trackEnd == data.Length) break;
            trackOffset = trackEnd;
        }

        if (candidates.Count == 0) throw new InvalidDataException("The ATX image contains no readable sector record.");
        var sectorsPerTrack = Math.Max(AtxLayout.StandardSectorsPerTrack, candidates.Keys.Max(address => address.Sector));
        var blocks = candidates.OrderBy(item => item.Key.Track).ThenBy(item => item.Key.Sector).Select(item =>
        {
            var selected = item.Value.OrderBy(candidate => candidate.Status == 0 ? 0 : 1).ThenBy(candidate => candidate.Position).First();
            var logical = item.Key.Track * sectorsPerTrack + item.Key.Sector - 1;
            return new SectorBlock(logical, new(item.Key.Track, 0, item.Key.Sector), selected.Data, selected.Status == 0, DiagnosticCode: selected.Status);
        }).ToArray();
        var logicalSectorCount = AtxLayout.TrackCount * sectorsPerTrack;
        return new(DiskImageFormatIds.AtariAtx, AtxLayout.SectorSize, AtxLayout.TrackCount, 1, sectorsPerTrack, blocks, capacity: (long)logicalSectorCount * AtxLayout.SectorSize, logicalBlockCount: logicalSectorCount);
    }

    private static int ReadOffset(byte[] data, int offset, string field)
    {
        EnsureRange(data, offset, sizeof(uint), field);
        return ToInt(ReadUInt32(data, offset), field);
    }

    private static uint ReadUInt32(byte[] data, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, sizeof(uint)));
    private static int ToInt(uint value, string field) => value <= int.MaxValue ? (int)value : throw new InvalidDataException($"The ATX {field} offset is too large.");
    private static void EnsureRange(byte[] data, int offset, int length, string field)
    {
        if (offset < 0 || length < 0 || offset > data.Length - length) throw new InvalidDataException($"The ATX {field} is truncated.");
    }

    private sealed record SectorCandidate(byte Status, ushort Position, byte[] Data);
}
