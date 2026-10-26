using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Reading.Blocks;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Adaptation PCM commune : documents séquentiels et codage explicite du RAW, sans protocole de machine.</summary>
internal static class CassettePcmFunctions
{
    /// <summary>Normalise seulement l'ordre des octets et le signe ; la précision et les canaux de la source RAW restent intacts.</summary>
    internal static MediaImageDocument RawDocument(MediaSourceDescriptor source, byte[] bytes, CassettePcmProfile profile)
    {
        var width = profile.BitsPerSample / 8; var align = width * profile.Channels;
        if (bytes.Length % align != 0) throw new InvalidDataException("The RAW signal ends inside a sample frame.");
        var canonical = new byte[bytes.Length];
        for (var sample = 0; sample < bytes.Length; sample += width)
        {
            for (var octet = 0; octet < width; octet++) canonical[sample + octet] = bytes[sample + (profile.LittleEndian ? octet : width - 1 - octet)];
            if (profile.Signed != (profile.BitsPerSample != 8)) canonical[sample + width - 1] ^= 128;
        }
        var metadata = new Dictionary<string, string> { ["channels"] = profile.Channels.ToString(CultureInfo.InvariantCulture), ["sampleRate"] = profile.SampleRate.ToString(CultureInfo.InvariantCulture), ["bitsPerSample"] = profile.BitsPerSample.ToString(CultureInfo.InvariantCulture), ["blockAlign"] = align.ToString(CultureInfo.InvariantCulture) };
        var frames = bytes.Length / align; var duration = TimeSpan.FromSeconds((double)frames / profile.SampleRate);
        var data = new MemoryRandomAccessData(canonical);
        IReadOnlyList<SequentialMediaSegment> segments = frames == 0 ? [] : Enumerable.Range(0, profile.Channels).Select(channel => new SequentialMediaSegment(0, SequentialSegmentKind.Samples, frames, TimeSpan.Zero, duration, channelNumber: channel, dataRange: new MediaDataRange(0, canonical.Length, MediaDataRangeKind.Stored, data), metadata: metadata)).ToArray();
        return MediaImageDocument.CreateUnexplored(source, Constants.TapeImageFormatIds.Raw, MediaKind.Tape, new SequentialMediaImageRepresentation(bytes.Length, duration, segments), [], metadata);
    }

    /// <summary>Lit les canaux entiers à leur précision native pour les conversions ; le service d'émulation choisit séparément le PCM 16 bits.</summary>
    internal static async Task<(int[][] Channels, int Rate, int Bits)> ChannelsAsync(MediaImageDocument document, CancellationToken token)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var rate, out var bits, out var align) || document.Representation is not SequentialMediaImageRepresentation image) throw new InvalidDataException("A PCM signal document is required.");
        var values = Enumerable.Range(0, channels).Select(_ => new List<int>()).ToArray();
        foreach (var segment in (image.Segments ?? []).Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber is null or 0))
        {
            var bytes = await BlockMediaDataReader.ReadRangeAsync(segment.DataRange, token).ConfigureAwait(false) ?? [];
            for (var channel = 0; channel < channels; channel++) values[channel].AddRange(PcmSampleFunctions.ExtractChannel(bytes, channels, bits, align, channel));
        }
        return (values.Select(channel => channel.ToArray()).ToArray(), rate, bits);
    }

    /// <summary>Encode le RAW depuis les canaux natifs ; à profil identique, aucun bit de précision n'est perdu.</summary>
    internal static async Task<byte[]> EncodeDocumentAsync(MediaImageDocument document, CassettePcmProfile profile, CancellationToken token)
    {
        var source = await ChannelsAsync(document, token).ConfigureAwait(false);
        var count = checked((int)Math.Round((double)source.Channels[0].Length * profile.SampleRate / source.Rate));
        var width = profile.BitsPerSample / 8;
        var bytes = new byte[checked(count * width * profile.Channels)];
        for (var frame = 0; frame < count; frame++)
        {
            var position = (double)frame * source.Rate / profile.SampleRate;
            var first = Math.Min((int)position, source.Channels[0].Length - 1); var next = Math.Min(first + 1, source.Channels[0].Length - 1);
            for (var channel = 0; channel < profile.Channels; channel++)
            {
                var samples = source.Channels[Math.Min(channel, source.Channels.Length - 1)];
                var left = unchecked(samples[first] << (32 - source.Bits)); var right = unchecked(samples[next] << (32 - source.Bits));
                var value = (int)Math.Clamp(Math.Round(left + ((double)right - left) * (position - first)), int.MinValue, int.MaxValue);
                var encoded = unchecked((uint)value) >> (32 - profile.BitsPerSample);
                if (!profile.Signed) encoded ^= 1u << (profile.BitsPerSample - 1);
                for (var octet = 0; octet < width; octet++) bytes[(frame * profile.Channels + channel) * width + octet] = (byte)(encoded >> (8 * (profile.LittleEndian ? octet : width - 1 - octet)));
            }
        }
        return bytes;
    }

    internal static MediaImageDocument Document(MediaSourceDescriptor source, string format, MediaCassetteSignal signal)
    {
        var bytes = new byte[checked(signal.Samples.Length * 2)];
        for (var index = 0; index < signal.Samples.Length; index++) BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(index * 2), signal.Samples.Span[index]);
        var metadata = new Dictionary<string, string> { ["channels"] = "1", ["sampleRate"] = signal.SampleRate.ToString(CultureInfo.InvariantCulture), ["bitsPerSample"] = "16", ["blockAlign"] = "2" };
        IReadOnlyList<SequentialMediaSegment> segments = bytes.Length == 0 ? [] : [new(0, SequentialSegmentKind.Samples, channelNumber: 0, dataRange: new MediaDataRange(0, bytes.Length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(bytes)), metadata: metadata)];
        return MediaImageDocument.CreateUnexplored(source, format, MediaKind.Tape, new SequentialMediaImageRepresentation(bytes.Length, TimeSpan.FromSeconds((double)signal.Samples.Length / signal.SampleRate), segments), [], metadata);
    }
}
