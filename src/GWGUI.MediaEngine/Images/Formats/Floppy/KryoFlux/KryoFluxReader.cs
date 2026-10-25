using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.KryoFlux;

/// <summary>Lit une image multipiste constituée de flux bruts KryoFlux NN.S.raw.</summary>
public sealed partial class KryoFluxReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.RawKryoFlux }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.Raw }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Flux }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => [];
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => SupportedExtensions;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (!SupportedExtensions.Contains(context.Extension) || !TrackFileName().IsMatch(Path.GetFileName(context.Source.PrimaryPath)) || context.Length < 8) return false;
        var header = await context.ReadHeaderAsync((int)Math.Min(512, context.Length), cancellationToken).ConfigureAwait(false);
        return ContainsKryoFluxInfo(header.Span);
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var files = FindTrackFiles(context.Source.PrimaryPath);
        var tracks = new List<ProtectedTrack>(files.Count);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decoded = await ReadTrackAsync(file.Path, cancellationToken).ConfigureAwait(false);
            tracks.Add(new ProtectedTrack(file.Cylinder, file.Head, null, [], [], [], decoded.Revolutions));
            foreach (var pair in decoded.Metadata) metadata.TryAdd(pair.Key, pair.Value);
        }

        var associated = files.Select(file => file.Path).Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(context.Source.PrimaryPath), StringComparison.OrdinalIgnoreCase)).ToArray();
        var source = new MediaSourceDescriptor(context.Source.PrimaryPath, associated, files.Sum(file => new FileInfo(file.Path).Length), context.Source.RequestedFormatId);
        return MediaImageDocumentFactory.CreateFloppyFlux(source, DiskImageFormatIds.RawKryoFlux, new ProtectedTrackImage(tracks, true), metadata);
    }

    private static bool ContainsKryoFluxInfo(ReadOnlySpan<byte> data)
    {
        var offset = 0;
        while (offset < data.Length)
        {
            var opcode = data[offset];
            if (opcode == KryoFluxFormat.OutOfBand)
            {
                if (offset > data.Length - 4) return false;
                var type = data[offset + 1];
                var size = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + 2, 2));
                if (offset + 4 > data.Length - size) return false;
                if (type == KryoFluxFormat.KryoFluxInfo) return true;
                offset += 4 + size;
            }
            else if (opcode is KryoFluxFormat.Nop3 or KryoFluxFormat.Flux3) offset += 3;
            else if (opcode <= 7 || opcode == KryoFluxFormat.Nop2) offset += 2;
            else offset++;
        }

        return false;
    }

    private static IReadOnlyList<TrackFile> FindTrackFiles(string primaryPath)
    {
        var fullPath = Path.GetFullPath(primaryPath);
        var match = TrackFileName().Match(Path.GetFileName(fullPath));
        if (!match.Success) throw new InvalidDataException("The KryoFlux file name must end with NN.S.raw.");
        var prefix = match.Groups[1].Value;
        var directory = Path.GetDirectoryName(fullPath) ?? throw new InvalidDataException("The KryoFlux source has no parent directory.");
        var files = new List<TrackFile>();
        foreach (var path in Directory.EnumerateFiles(directory, $"{prefix}*.raw", SearchOption.TopDirectoryOnly))
        {
            var candidate = TrackFileName().Match(Path.GetFileName(path));
            if (!candidate.Success || !candidate.Groups[1].Value.Equals(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var cylinder = int.Parse(candidate.Groups[2].Value, CultureInfo.InvariantCulture);
            var head = int.Parse(candidate.Groups[3].Value, CultureInfo.InvariantCulture);
            if (cylinder > KryoFluxFormat.MaximumTrack || head > KryoFluxFormat.MaximumHead) continue;
            files.Add(new TrackFile(path, cylinder, head));
        }

        if (files.Count == 0) throw new InvalidDataException("No KryoFlux track file was found.");
        return files.OrderBy(file => file.Cylinder).ThenBy(file => file.Head).ToArray();
    }

    private static async Task<DecodedTrack> ReadTrackAsync(string path, CancellationToken cancellationToken)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var indexPositions = new List<uint>();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        var sampleClock = KryoFluxFormat.DefaultSampleClock;
        ReadOutOfBand(data, indexPositions, metadata, ref sampleClock);
        if (sampleClock <= 0 || double.IsNaN(sampleClock) || double.IsInfinity(sampleClock)) throw new InvalidDataException("The KryoFlux sample clock is invalid.");
        var revolutions = DecodeFlux(data, indexPositions, sampleClock);
        if (revolutions.Count == 0) throw new InvalidDataException("The KryoFlux track contains no complete flux revolution.");
        return new DecodedTrack(revolutions, metadata);
    }

    private static void ReadOutOfBand(byte[] data, ICollection<uint> indexPositions, IDictionary<string, string> metadata, ref double sampleClock)
    {
        var offset = 0;
        while (offset < data.Length)
        {
            var opcode = data[offset];
            if (opcode == KryoFluxFormat.OutOfBand)
            {
                if (IsTrailingFill(data, offset)) break;
                Require(data, offset, 4);
                var type = data[offset + 1];
                var size = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset + 2, 2));
                Require(data, offset + 4, size);
                var payload = data.AsSpan(offset + 4, size);
                if (type == KryoFluxFormat.Index)
                {
                    if (size < 4) throw new InvalidDataException("The KryoFlux index record is truncated.");
                    indexPositions.Add(BinaryPrimitives.ReadUInt32LittleEndian(payload[..4]));
                }
                else if (type == KryoFluxFormat.KryoFluxInfo)
                {
                    ReadInfo(payload, metadata, ref sampleClock);
                }
                else if (type == KryoFluxFormat.EndOfFile)
                {
                    break;
                }
                offset += 4 + size;
            }
            else if (opcode is KryoFluxFormat.Nop3 or KryoFluxFormat.Flux3) offset += 3;
            else if (opcode <= 7 || opcode == KryoFluxFormat.Nop2) offset += 2;
            else offset++;
            if (offset > data.Length) throw new InvalidDataException("The KryoFlux stream is truncated.");
        }
    }

    private static IReadOnlyList<TrackFluxRevolution> DecodeFlux(byte[] data, IReadOnlyList<uint> indexPositions, double sampleClock)
    {
        var segments = new List<List<uint>> { new() };
        long overflow = 0;
        uint streamPosition = 0;
        var indexNumber = 0;
        var offset = 0;
        while (offset < data.Length)
        {
            while (indexNumber < indexPositions.Count && streamPosition >= indexPositions[indexNumber])
            {
                segments.Add(new List<uint>());
                indexNumber++;
            }

            var opcode = data[offset];
            if (opcode <= 7)
            {
                Require(data, offset, 2);
                AddFlux(segments[^1], overflow + (opcode << 8) + data[offset + 1], sampleClock);
                overflow = 0;
                streamPosition += 2;
                offset += 2;
            }
            else if (opcode is KryoFluxFormat.Nop1 or KryoFluxFormat.Nop2 or KryoFluxFormat.Nop3)
            {
                var count = opcode - 7;
                Require(data, offset, count);
                streamPosition += checked((uint)count);
                offset += count;
            }
            else if (opcode == KryoFluxFormat.Overflow16)
            {
                overflow += 0x10000;
                streamPosition++;
                offset++;
            }
            else if (opcode == KryoFluxFormat.Flux3)
            {
                Require(data, offset, 3);
                AddFlux(segments[^1], overflow + (data[offset + 1] << 8) + data[offset + 2], sampleClock);
                overflow = 0;
                streamPosition += 3;
                offset += 3;
            }
            else if (opcode == KryoFluxFormat.OutOfBand)
            {
                if (IsTrailingFill(data, offset)) break;
                Require(data, offset, 4);
                var type = data[offset + 1];
                var size = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(offset + 2, 2));
                Require(data, offset + 4, size);
                if (type is KryoFluxFormat.StreamInfo or KryoFluxFormat.StreamEnd)
                {
                    if (size < 4 || BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 4, 4)) != streamPosition)
                        throw new InvalidDataException("The KryoFlux stream position is inconsistent.");
                }
                offset += 4 + size;
                if (type == KryoFluxFormat.EndOfFile) break;
            }
            else
            {
                AddFlux(segments[^1], overflow + opcode, sampleClock);
                overflow = 0;
                streamPosition++;
                offset++;
            }
        }

        IEnumerable<List<uint>> complete = indexPositions.Count > 1 ? segments.Skip(1).Take(indexPositions.Count - 1) : segments;
        return complete.Where(segment => segment.Count != 0).Select(segment =>
        {
            var duration = segment.Aggregate<uint, ulong>(0, (sum, value) => sum + value);
            if (duration > uint.MaxValue) throw new InvalidDataException("The KryoFlux revolution duration exceeds the supported range.");
            return new TrackFluxRevolution(1, new FluxRevolution((uint)duration, segment));
        }).ToArray();
    }

    private static void AddFlux(ICollection<uint> destination, long sourceTicks, double sampleClock)
    {
        if (sourceTicks <= 0) throw new InvalidDataException("The KryoFlux stream contains a non-positive flux interval.");
        var nanoseconds = Math.Round(sourceTicks * 1_000_000_000d / sampleClock, MidpointRounding.AwayFromZero);
        if (nanoseconds is < 1 or > uint.MaxValue) throw new InvalidDataException("The KryoFlux interval exceeds the supported timing range.");
        destination.Add((uint)nanoseconds);
    }

    private static void ReadInfo(ReadOnlySpan<byte> payload, IDictionary<string, string> metadata, ref double sampleClock)
    {
        var text = Encoding.UTF8.GetString(payload).TrimEnd('\0');
        foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');
            if (separator <= 0) continue;
            var key = part[..separator].Trim();
            var value = part[(separator + 1)..].Trim();
            metadata.TryAdd(key, value);
            if (key.Equals("sck", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)) sampleClock = parsed;
        }
    }

    private static void Require(byte[] data, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset > data.Length - count) throw new InvalidDataException("The KryoFlux stream is truncated.");
    }

    private static bool IsTrailingFill(byte[] data, int offset)
    {
        for (var index = offset; index < data.Length; index++)
            if (data[index] != KryoFluxFormat.OutOfBand) return false;
        return true;
    }

    [GeneratedRegex(@"^(.*?)(\d{2})\.([01])\.raw$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TrackFileName();

    private sealed record TrackFile(string Path, int Cylinder, int Head);
    private sealed record DecodedTrack(IReadOnlyList<TrackFluxRevolution> Revolutions, IReadOnlyDictionary<string, string> Metadata);
}
