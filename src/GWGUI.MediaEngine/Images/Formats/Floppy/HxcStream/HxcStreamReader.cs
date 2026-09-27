using System.Buffers.Binary;
using System.Collections.Frozen;
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
using K4os.Compression.LZ4;

namespace GWGUI.MediaEngine.Images.Formats.Floppy.HxcStream;

/// <summary>Lit une image HxC Stream multipiste à partir de ses fichiers trackNN.S.hxcstream.</summary>
public sealed partial class HxcStreamReader : IMediaImageReader
{
    private static readonly IReadOnlySet<string> SupportedFormatIds = new[] { DiskImageFormatIds.RawHxcStream }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> SupportedExtensions = new[] { DiskImageFileExtensions.HxcStream }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyList<ReadOnlyMemory<byte>> SupportedSignatures = ["CHKH"u8.ToArray()];
    private static readonly IReadOnlySet<MediaKind> SupportedMediaKinds = new[] { MediaKind.Floppy }.ToFrozenSet();
    private static readonly IReadOnlySet<MediaRepresentationKind> SupportedRepresentationKinds = new[] { MediaRepresentationKind.Flux }.ToFrozenSet();

    IReadOnlySet<string> IMediaImageReader.FormatIds => SupportedFormatIds;
    IReadOnlySet<string> IMediaImageReader.Extensions => SupportedExtensions;
    IReadOnlyList<ReadOnlyMemory<byte>> IMediaImageReader.Signatures => SupportedSignatures;
    IReadOnlySet<string> IMediaImageReader.AssociatedFileExtensions => SupportedExtensions;
    IReadOnlySet<MediaKind> IMediaImageReader.MediaKinds => SupportedMediaKinds;
    IReadOnlySet<MediaRepresentationKind> IMediaImageReader.RepresentationKinds => SupportedRepresentationKinds;
    bool IMediaImageReader.SupportsFormatId(string formatId) => SupportedFormatIds.Contains(formatId);

    async ValueTask<bool> IMediaImageReader.CanReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        if (context.RequestedFormatId is not null && !SupportedFormatIds.Contains(context.RequestedFormatId)) return false;
        if (!SupportedExtensions.Contains(context.Extension) || context.Length < HxcStreamFormat.ChunkHeaderSize + HxcStreamFormat.ChunkCrcSize) return false;
        var header = await context.ReadHeaderAsync(HxcStreamFormat.ChunkHeaderSize, cancellationToken).ConfigureAwait(false);
        return BinaryPrimitives.ReadUInt32LittleEndian(header.Span) == HxcStreamFormat.ChunkSignature;
    }

    async Task<MediaImageDocument> IMediaImageReader.ReadAsync(MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var files = FindTrackFiles(context.Source.PrimaryPath);
        var tracks = new List<ProtectedTrack>(files.Count);
        var writeProtected = false;
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var decoded = await ReadTrackAsync(file.Path, cancellationToken).ConfigureAwait(false);
            tracks.Add(new ProtectedTrack(file.Cylinder, file.Head, null, [], [], [], decoded.Revolutions));
            writeProtected |= decoded.WriteProtected;
            foreach (var pair in decoded.Metadata) metadata.TryAdd(pair.Key, pair.Value);
        }

        var associated = files.Select(file => file.Path).Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(context.Source.PrimaryPath), StringComparison.OrdinalIgnoreCase)).ToArray();
        var source = new MediaSourceDescriptor(context.Source.PrimaryPath, associated, files.Sum(file => new FileInfo(file.Path).Length), context.Source.RequestedFormatId);
        return MediaImageDocumentFactory.CreateFloppyFlux(source, DiskImageFormatIds.RawHxcStream, new ProtectedTrackImage(tracks, writeProtected), metadata);
    }

    private static IReadOnlyList<TrackFile> FindTrackFiles(string primaryPath)
    {
        var fullPath = Path.GetFullPath(primaryPath);
        var match = TrackFileName().Match(Path.GetFileName(fullPath));
        if (!match.Success) throw new InvalidDataException("The HxC Stream file name must end with NN.S.hxcstream.");
        var prefix = match.Groups[1].Value;
        var directory = Path.GetDirectoryName(fullPath) ?? throw new InvalidDataException("The HxC Stream source has no parent directory.");
        var files = new List<TrackFile>();
        foreach (var path in Directory.EnumerateFiles(directory, $"{prefix}*.hxcstream", SearchOption.TopDirectoryOnly))
        {
            var candidate = TrackFileName().Match(Path.GetFileName(path));
            if (!candidate.Success || !candidate.Groups[1].Value.Equals(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var cylinder = int.Parse(candidate.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
            var head = int.Parse(candidate.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
            if (cylinder > HxcStreamFormat.MaximumTrack || head > HxcStreamFormat.MaximumHead) continue;
            files.Add(new TrackFile(path, cylinder, head));
        }

        if (files.Count == 0) throw new InvalidDataException("No HxC Stream track file was found.");
        return files.OrderBy(file => file.Cylinder).ThenBy(file => file.Head).ToArray();
    }

    private static async Task<DecodedTrack> ReadTrackAsync(string path, CancellationToken cancellationToken)
    {
        var data = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var flux = new List<uint>();
        var io = new List<ushort>();
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        var resolutionNanoseconds = HxcStreamFormat.DefaultResolutionNanoseconds;
        var offset = 0;
        while (offset < data.Length)
        {
            Require(data, offset, HxcStreamFormat.ChunkHeaderSize + HxcStreamFormat.ChunkCrcSize);
            var chunkSize = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 4, 4)));
            if (BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4)) != HxcStreamFormat.ChunkSignature || chunkSize < HxcStreamFormat.ChunkHeaderSize + HxcStreamFormat.ChunkCrcSize || chunkSize > HxcStreamFormat.MaximumChunkSize)
                throw new InvalidDataException($"Invalid HxC Stream chunk at byte {offset}.");
            Require(data, offset, chunkSize);
            var storedCrc = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + chunkSize - HxcStreamFormat.ChunkCrcSize, HxcStreamFormat.ChunkCrcSize));
            if (ComputeCrc32(data.AsSpan(offset, chunkSize - HxcStreamFormat.ChunkCrcSize), uint.MaxValue) != storedCrc)
                throw new InvalidDataException($"Invalid HxC Stream CRC at byte {offset}.");

            var blockOffset = offset + HxcStreamFormat.ChunkHeaderSize;
            var chunkEnd = offset + chunkSize - HxcStreamFormat.ChunkCrcSize;
            while (blockOffset < chunkEnd)
            {
                Require(data, blockOffset, HxcStreamFormat.BlockHeaderSize, chunkEnd);
                var type = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(blockOffset, 4));
                var payloadSize = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(blockOffset + 4, 4)));
                switch (type)
                {
                    case HxcStreamFormat.MetadataBlock:
                        Require(data, blockOffset + HxcStreamFormat.BlockHeaderSize, payloadSize, chunkEnd);
                        ReadMetadata(data.AsSpan(blockOffset + HxcStreamFormat.BlockHeaderSize, payloadSize), metadata, ref resolutionNanoseconds);
                        blockOffset += HxcStreamFormat.BlockHeaderSize + payloadSize;
                        break;
                    case HxcStreamFormat.PackedIoBlock:
                        blockOffset = ReadPackedIo(data, blockOffset, chunkEnd, io);
                        break;
                    case HxcStreamFormat.PackedStreamBlock:
                        blockOffset = ReadPackedStream(data, blockOffset, chunkEnd, flux);
                        break;
                    default:
                        throw new InvalidDataException($"Unknown HxC Stream block type {type} at byte {blockOffset}.");
                }

                blockOffset = checked((blockOffset + 3) & ~3);
            }

            offset += chunkSize;
        }

        if (flux.Count == 0) throw new InvalidDataException("The HxC Stream track contains no flux transitions.");
        return new DecodedTrack(CreateRevolutions(flux, io, resolutionNanoseconds), io.Any(value => (value & (1 << 5)) != 0), metadata);
    }

    private static int ReadPackedIo(byte[] data, int offset, int end, ICollection<ushort> destination)
    {
        Require(data, offset, HxcStreamFormat.PackedIoHeaderSize, end);
        var packedSize = ReadBoundedSize(data, offset + 8);
        var unpackedSize = ReadBoundedSize(data, offset + 12);
        if ((unpackedSize & 1) != 0) throw new InvalidDataException("The HxC Stream I/O block has an odd unpacked size.");
        Require(data, offset + HxcStreamFormat.PackedIoHeaderSize, packedSize, end);
        var unpacked = DecodeLz4(data.AsSpan(offset + HxcStreamFormat.PackedIoHeaderSize, packedSize), unpackedSize);
        for (var index = 0; index < unpacked.Length; index += 2) destination.Add(BinaryPrimitives.ReadUInt16LittleEndian(unpacked.AsSpan(index, 2)));
        return offset + HxcStreamFormat.PackedIoHeaderSize + packedSize;
    }

    private static int ReadPackedStream(byte[] data, int offset, int end, ICollection<uint> destination)
    {
        Require(data, offset, HxcStreamFormat.PackedStreamHeaderSize, end);
        var packedSize = ReadBoundedSize(data, offset + 8);
        var unpackedSize = ReadBoundedSize(data, offset + 12);
        var pulseCount = ReadBoundedSize(data, offset + 16, HxcStreamFormat.MaximumPulseCount);
        Require(data, offset + HxcStreamFormat.PackedStreamHeaderSize, packedSize, end);
        var unpacked = DecodeLz4(data.AsSpan(offset + HxcStreamFormat.PackedStreamHeaderSize, packedSize), unpackedSize);
        DecodePulses(unpacked, pulseCount, destination);
        return offset + HxcStreamFormat.PackedStreamHeaderSize + packedSize;
    }

    private static byte[] DecodeLz4(ReadOnlySpan<byte> packed, int unpackedSize)
    {
        var unpacked = new byte[unpackedSize];
        var decoded = LZ4Codec.Decode(packed, unpacked);
        if (decoded != unpackedSize) throw new InvalidDataException("The HxC Stream LZ4 block is incomplete.");
        return unpacked;
    }

    private static void DecodePulses(ReadOnlySpan<byte> data, int declaredPulseCount, ICollection<uint> destination)
    {
        var offset = 0;
        var decoded = 0;
        while (decoded < declaredPulseCount && offset < data.Length)
        {
            var first = data[offset++];
            uint value;
            if ((first & 0x80) == 0) value = first;
            else if ((first & 0xC0) == 0x80) value = ReadBigEndianTail(data, ref offset, first & 0x3F, 1);
            else if ((first & 0xE0) == 0xC0) value = ReadBigEndianTail(data, ref offset, first & 0x1F, 2);
            else if ((first & 0xF0) == 0xE0) value = ReadBigEndianTail(data, ref offset, first & 0x0F, 3);
            else throw new InvalidDataException("The HxC Stream pulse uses an unknown variable-length code.");
            if (value != 0) destination.Add(value);
            decoded++;
        }

        if (decoded != declaredPulseCount) throw new InvalidDataException("The HxC Stream pulse block ended before its declared pulse count.");
    }

    private static uint ReadBigEndianTail(ReadOnlySpan<byte> data, ref int offset, int prefix, int byteCount)
    {
        if (offset > data.Length - byteCount) throw new InvalidDataException("The HxC Stream pulse is truncated.");
        uint value = checked((uint)prefix);
        for (var index = 0; index < byteCount; index++) value = (value << 8) | data[offset++];
        return value;
    }

    private static IReadOnlyList<TrackFluxRevolution> CreateRevolutions(IReadOnlyList<uint> flux, IReadOnlyList<ushort> io, int resolutionNanoseconds)
    {
        var indexes = new List<int>();
        var previous = false;
        long ticks = 0;
        var fluxIndex = 0;
        for (var ioIndex = 0; ioIndex < io.Count; ioIndex++)
        {
            var current = (io[ioIndex] & 1) != 0;
            if (current && !previous)
            {
                var targetTicks = checked((long)ioIndex * HxcStreamFormat.IoSampleFluxTicks);
                while (fluxIndex < flux.Count && ticks < targetTicks) ticks += flux[fluxIndex++];
                indexes.Add(fluxIndex);
            }
            previous = current;
        }

        var revolutions = new List<TrackFluxRevolution>();
        if (indexes.Count >= 2)
        {
            for (var index = 1; index < indexes.Count; index++) AddRevolution(flux, indexes[index - 1], indexes[index], resolutionNanoseconds, revolutions);
        }
        else
        {
            AddRevolution(flux, 0, flux.Count, resolutionNanoseconds, revolutions);
        }

        return revolutions;
    }

    private static void AddRevolution(IReadOnlyList<uint> flux, int start, int end, int resolutionNanoseconds, ICollection<TrackFluxRevolution> destination)
    {
        if (end <= start) return;
        var intervals = flux.Skip(start).Take(end - start).ToArray();
        var indexTime = intervals.Aggregate<uint, ulong>(0, (sum, value) => sum + value);
        if (indexTime > uint.MaxValue) throw new InvalidDataException("The HxC Stream revolution duration exceeds the supported range.");
        destination.Add(new TrackFluxRevolution(resolutionNanoseconds, new FluxRevolution((uint)indexTime, intervals)));
    }

    private static void ReadMetadata(ReadOnlySpan<byte> bytes, IDictionary<string, string> destination, ref int resolutionNanoseconds)
    {
        var text = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
        foreach (var line in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separator = line.IndexOf(' ');
            if (separator <= 0) continue;
            var key = line[..separator];
            var value = line[(separator + 1)..].Trim();
            destination.TryAdd(key, value);
            if (key.Equals("sample_rate_hz", StringComparison.Ordinal) && int.TryParse(value, out var sampleRate) && sampleRate is 25_000_000 or 50_000_000)
                resolutionNanoseconds = 1_000_000_000 / sampleRate;
        }
    }

    private static int ReadBoundedSize(byte[] data, int offset, int maximum = HxcStreamFormat.MaximumUnpackedBlockSize)
    {
        var value = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset, 4));
        if (value > maximum) throw new InvalidDataException($"HxC Stream block size {value} exceeds the supported limit.");
        return checked((int)value);
    }

    private static void Require(byte[] data, int offset, int count, int? end = null)
    {
        var limit = end ?? data.Length;
        if (offset < 0 || count < 0 || offset > limit - count || limit > data.Length) throw new InvalidDataException("The HxC Stream structure is truncated.");
    }

    private static uint ComputeCrc32(ReadOnlySpan<byte> data, uint seed)
    {
        var crc = seed ^ uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }
        return crc ^ uint.MaxValue;
    }

    [GeneratedRegex(@"^(.*?)(\d{2})\.([01])\.hxcstream$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TrackFileName();

    private sealed record TrackFile(string Path, int Cylinder, int Head);
    private sealed record DecodedTrack(IReadOnlyList<TrackFluxRevolution> Revolutions, bool WriteProtected, IReadOnlyDictionary<string, string> Metadata);
}
