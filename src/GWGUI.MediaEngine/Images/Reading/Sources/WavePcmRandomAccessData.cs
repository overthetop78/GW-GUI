using System.Buffers.Binary;
using System.IO;
using System.Text;
using IMediaRandomAccessData = global::GWGUI.MediaFileSystems.Interfaces.IMediaRandomAccessData;
using GWGUI.MediaEngine.Images.Formats.Optical.BinCue;

namespace GWGUI.MediaEngine.Images.Reading.Sources;

/// <summary>Expose le bloc PCM d’un fichier RIFF/WAVE stéréo 16 bits 44,1 kHz.</summary>
public sealed class WavePcmRandomAccessData : IMediaRandomAccessData
{
    private readonly string path;
    private readonly long dataOffset;

    private WavePcmRandomAccessData(string path, long dataOffset, long length)
    {
        this.path = path;
        this.dataOffset = dataOffset;
        Length = length;
    }

    public long Length { get; }

    public static WavePcmRandomAccessData Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        if (stream.Length < WavePcmConstants.RiffHeaderLength)
            throw new InvalidDataException("The WAVE file header is incomplete.");
        var riff = Encoding.ASCII.GetString(reader.ReadBytes(sizeof(uint)));
        _ = reader.ReadUInt32();
        var form = Encoding.ASCII.GetString(reader.ReadBytes(sizeof(uint)));
        if (!string.Equals(riff, WavePcmConstants.RiffChunk, StringComparison.Ordinal)
            || !string.Equals(form, WavePcmConstants.WaveForm, StringComparison.Ordinal))
            throw new InvalidDataException("The file is not a RIFF/WAVE stream.");

        var hasPcmFormat = false;
        long dataOffset = 0;
        long dataLength = 0;
        while (stream.Position <= stream.Length - WavePcmConstants.ChunkHeaderLength)
        {
            var chunkId = Encoding.ASCII.GetString(reader.ReadBytes(sizeof(uint)));
            var chunkLength = reader.ReadUInt32();
            var chunkStart = stream.Position;
            if (chunkLength > stream.Length - chunkStart)
                throw new InvalidDataException("A WAVE chunk exceeds the file.");
            if (string.Equals(chunkId, WavePcmConstants.FormatChunk, StringComparison.Ordinal))
            {
                if (chunkLength < WavePcmConstants.MinimumFormatChunkLength)
                    throw new InvalidDataException("The WAVE format chunk is incomplete.");
                var format = reader.ReadUInt16();
                var channels = reader.ReadUInt16();
                var sampleRate = reader.ReadInt32();
                var byteRate = reader.ReadInt32();
                var blockAlign = reader.ReadUInt16();
                var bitsPerSample = reader.ReadUInt16();
                if (format != WavePcmConstants.PcmFormat
                    || channels != WavePcmConstants.StereoChannels
                    || sampleRate != WavePcmConstants.SampleRate
                    || byteRate != WavePcmConstants.ByteRate
                    || blockAlign != WavePcmConstants.BlockAlign
                    || bitsPerSample != WavePcmConstants.BitsPerSample)
                    throw new NotSupportedException("Only stereo 16-bit 44.1 kHz PCM WAVE tracks are supported.");
                hasPcmFormat = true;
            }
            else if (string.Equals(chunkId, WavePcmConstants.DataChunk, StringComparison.Ordinal))
            {
                dataOffset = chunkStart;
                dataLength = chunkLength;
            }

            var nextChunk = checked(chunkStart + chunkLength + (chunkLength & 1));
            if (nextChunk > stream.Length)
                throw new InvalidDataException("A WAVE chunk padding byte is missing.");
            stream.Position = nextChunk;
        }

        if (!hasPcmFormat || dataLength <= 0)
            throw new InvalidDataException("The WAVE file has no supported PCM data chunk.");
        if (dataLength % WavePcmConstants.AudioSectorSize != 0)
            throw new InvalidDataException("The WAVE PCM data is not aligned to complete CD audio sectors.");
        return new WavePcmRandomAccessData(fullPath, dataOffset, dataLength);
    }

    public async ValueTask ReadExactlyAsync(
        long offset,
        Memory<byte> destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        if (offset > Length || destination.Length > Length - offset)
            throw new ArgumentOutOfRangeException(nameof(destination), "The requested range exceeds the WAVE PCM data.");
        if (destination.IsEmpty) return;
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.RandomAccess);
        stream.Seek(checked(dataOffset + offset), SeekOrigin.Begin);
        var totalRead = 0;
        while (totalRead < destination.Length)
        {
            var read = await stream.ReadAsync(destination[totalRead..], cancellationToken).ConfigureAwait(false);
            if (read == 0) throw new EndOfStreamException("The WAVE PCM data ended before the requested range.");
            totalRead += read;
        }
    }
}
