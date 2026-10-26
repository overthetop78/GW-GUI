using System.IO;
using System.Buffers.Binary;
using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Formats.Tape.Uef;
using GWGUI.MediaEngine.Images.Formats.Tape.Wav;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Reading.Blocks;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Images.Formats.Tape.Csw;
using GWGUI.MediaEngine.Images.Formats.Tape.Raw;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Prépare le signal lu par un magnétophone virtuel en réutilisant les lecteurs de conteneurs existants.</summary>
/// <remarks>Aucune commande MOS ni adresse mémoire de machine n'est interprétée ici. Le fichier reste intact.
/// Les données UEF deviennent du FSK ; un WAV conserve sa cadence et son premier canal sans rééchantillonnage.</remarks>
internal static class CassetteSignalFunctions
{
    /// <summary>Formats capables de conserver un signal enregistré arbitraire sans interpréter le protocole d'une machine.</summary>
    internal static IReadOnlyList<string> WritableExtensions { get; } = Array.AsReadOnly<string>([".wav", ".uef", ".csw", ".raw"]);

    /// <summary>Publie atomiquement tout le signal, y compris les zones avant et après l'enregistrement.</summary>
    /// <remarks>Le magnétophone fournit sa bande complète. Le conteneur et ses fichiers temporaires sont possédés
    /// par l'écrivain WAV existant ; un échec conserve la destination précédente. Aucun protocole BBC n'est imposé.</remarks>
    internal static async Task WriteAsync(string path, MediaCassetteSignal signal, CancellationToken token, IReadOnlyDictionary<string, string>? parameters = null)
    {
        if (!WritableExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)) throw new NotSupportedException("This format cannot retain a recorded cassette signal.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(signal.SampleRate);
        if (Path.GetExtension(path).Equals(".uef", StringComparison.OrdinalIgnoreCase)) { await new UefWriter().WriteAsync(UefDocument(new(path, []), signal, token), path, TapeImageFormatIds.Uef, token).ConfigureAwait(false); return; }
        if (Path.GetExtension(path).Equals(".csw", StringComparison.OrdinalIgnoreCase)) { await new CswWriter().WriteAsync(CassettePcmFunctions.Document(new MediaSourceDescriptor(path, []), TapeImageFormatIds.Csw, signal), path, TapeImageFormatIds.Csw, token).ConfigureAwait(false); return; }
        if (Path.GetExtension(path).Equals(".raw", StringComparison.OrdinalIgnoreCase)) { await new RawTapeWriter().WriteAsync(CassettePcmFunctions.Document(new MediaSourceDescriptor(path, []) { Parameters = parameters }, TapeImageFormatIds.Raw, signal), path, TapeImageFormatIds.Raw, token).ConfigureAwait(false); return; }
        var pcm = new byte[checked(signal.Samples.Length * sizeof(short))];
        for (var index = 0; index < signal.Samples.Length; index++) BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(index * sizeof(short)), signal.Samples.Span[index]);
        var metadata = new Dictionary<string, string> { ["channels"] = "1", ["sampleRate"] = signal.SampleRate.ToString(CultureInfo.InvariantCulture), ["bitsPerSample"] = "16", ["blockAlign"] = "2" };
        IReadOnlyList<SequentialMediaSegment> segments = pcm.Length == 0 ? [] : [new(0, SequentialSegmentKind.Samples, channelNumber: 0, dataRange: new MediaDataRange(0, pcm.Length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(pcm)))];
        var document = MediaImageDocument.CreateUnexplored(new MediaSourceDescriptor(path, []), TapeImageFormatIds.Wav, MediaKind.Tape, new SequentialMediaImageRepresentation(pcm.Length, TimeSpan.FromSeconds((double)signal.Samples.Length / signal.SampleRate), segments), [], metadata);
        await new WavTapeWriter().WriteAsync(document, path, TapeImageFormatIds.Wav, token).ConfigureAwait(false);
    }

    /// <summary>Transcrit les passages par zéro en pulses UEF 0114, sans supposer 8N1 ni un débit série BBC.</summary>
    /// <remarks>La bande est numérique : l'amplitude analogique n'est pas une propriété UEF. Les durées et polarités
    /// des pulses et des silences sont conservées ; l'écrivain UEF existant possède la compression et la publication atomique.</remarks>
    internal static MediaImageDocument UefDocument(MediaSourceDescriptor source, MediaCassetteSignal signal, CancellationToken token)
    {
        var segments = new List<SequentialMediaSegment>();
        void Chunk(ushort id, byte[] bytes)
        {
            segments.Add(new(segments.Count, SequentialSegmentKind.Pulse, dataRange: new MediaDataRange(0, bytes.Length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(bytes)), metadata: new Dictionary<string, string> { [UefConstants.ChunkIdMetadataKey] = id.ToString("x4", CultureInfo.InvariantCulture) }));
        }
        Chunk(UefConstants.PhaseChange, [0, 0]);
        var offset = 0;
        while (offset < signal.Samples.Length)
        {
            token.ThrowIfCancellationRequested();
            var polarity = Math.Sign(signal.Samples.Span[offset]);
            var end = offset + 1;
            while (end < signal.Samples.Length && Math.Sign(signal.Samples.Span[end]) == polarity) end++;
            var duration = end - offset;
            // Les cycles BBC natifs peuvent être stockés ensemble, même lorsque les bits ou le débit changent.
            // Les pulses irréguliers passent ensuite par le chemin exact ci-dessous.
            if (polarity > 0 && signal.SampleRate % 4800 == 0)
            {
                var bits = new List<bool>(); var cursor = offset;
                while (cursor < signal.Samples.Length && bits.Count < 0xffffff)
                {
                    var positiveEnd = cursor;
                    while (positiveEnd < signal.Samples.Length && signal.Samples.Span[positiveEnd] > 0) positiveEnd++;
                    var width = positiveEnd - cursor;
                    if (width != signal.SampleRate / 2400 && width != signal.SampleRate / 4800) break;
                    var negativeEnd = positiveEnd;
                    while (negativeEnd < signal.Samples.Length && signal.Samples.Span[negativeEnd] < 0) negativeEnd++;
                    if (negativeEnd - positiveEnd != width) break;
                    bits.Add(width == signal.SampleRate / 4800); cursor = negativeEnd;
                }
                if (bits.Count > 0)
                {
                    var frequency = new byte[4]; BinaryPrimitives.WriteSingleLittleEndian(frequency, 1200); Chunk(UefConstants.BaseFrequency, frequency);
                    var cycles = new byte[5 + (bits.Count + 7) / 8]; cycles[0] = (byte)bits.Count; cycles[1] = (byte)(bits.Count >> 8); cycles[2] = (byte)(bits.Count >> 16); cycles[3] = cycles[4] = (byte)'W';
                    for (var bit = 0; bit < bits.Count; bit++) if (bits[bit]) cycles[5 + bit / 8] |= (byte)(128 >> (bit & 7));
                    Chunk(UefConstants.SecurityCycles, cycles); offset = cursor; continue;
                }
            }
            if (polarity == 0)
            {
                var gap = new byte[4]; BinaryPrimitives.WriteSingleLittleEndian(gap, (float)duration / signal.SampleRate); Chunk(UefConstants.FloatingGap, gap);
            }
            else
            {
                var frequency = new byte[4]; BinaryPrimitives.WriteSingleLittleEndian(frequency, (float)signal.SampleRate / (2 * duration)); Chunk(UefConstants.BaseFrequency, frequency);
                var pairs = 0;
                if (polarity > 0)
                {
                    var cursor = offset;
                    while (pairs < 0xffffff && cursor + 2 * duration <= signal.Samples.Length)
                    {
                        var complete = true;
                        for (var sample = 0; sample < 2 * duration; sample++)
                            if (Math.Sign(signal.Samples.Span[cursor + sample]) != (sample < duration ? 1 : -1)) { complete = false; break; }
                        if (!complete) break;
                        pairs++; cursor += 2 * duration;
                    }
                    if (pairs > 0) end = cursor;
                }
                if (pairs == 0) Chunk(UefConstants.SecurityCycles, [1, 0, 0, (byte)(polarity > 0 ? 'P' : 'W'), (byte)(polarity < 0 ? 'P' : 'W'), 0]);
                else
                {
                    var cycles = new byte[5 + (pairs + 7) / 8]; cycles[0] = (byte)pairs; cycles[1] = (byte)(pairs >> 8); cycles[2] = (byte)(pairs >> 16); cycles[3] = cycles[4] = (byte)'W';
                    Chunk(UefConstants.SecurityCycles, cycles);
                }
            }
            offset = end;
        }
        return MediaImageDocument.CreateUnexplored(source, TapeImageFormatIds.Uef, MediaKind.Tape, new SequentialMediaImageRepresentation(null, segments: segments), [], new Dictionary<string, string> { [UefConstants.GzipMetadataKey] = bool.TrueString });
    }

    internal static async Task<MediaCassetteSignal> ReadAsync(string path, CancellationToken cancellationToken, string? machineId = null, IReadOnlyDictionary<string, string>? parameters = null)
    {
        IMediaImageReader reader = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".uef" => new UefReader(), ".wav" => new WavTapeReader(),
            ".csw" => new CswReader(), ".raw" => new RawTapeReader(),
            _ => throw new NotSupportedException("This cassette signal format is not supported.")
        };
        var document = await reader.ReadAsync(new MediaRecognitionContext(new MediaSourceDescriptor(path, []) { Parameters = parameters }), cancellationToken).ConfigureAwait(false);
        var representation = (SequentialMediaImageRepresentation)document.Representation;
        var positions = new Dictionary<long, int>();
        var signal = document.FormatId == TapeImageFormatIds.Uef ? await UefAsync(representation, positions, cancellationToken).ConfigureAwait(false) : await WaveAsync(document, representation, cancellationToken).ConfigureAwait(false);
        if (machineId is null || signal.Samples.IsEmpty) return signal;
        var decoders = SequentialMediaComposition.CreateDefault().Decoders;
        var decoded = await decoders.DecodeAsync(document, machineId, cancellationToken).ConfigureAwait(false);
        // Un UEF de cycles ne contient pas d'octets implicites : le même décodeur existant reçoit alors son signal PCM.
        if (decoded.Blocks.Count == 0 && document.FormatId == TapeImageFormatIds.Uef)
        {
            var pcm = new byte[signal.Samples.Length * 2];
            for (var sample = 0; sample < signal.Samples.Length; sample++) BinaryPrimitives.WriteInt16LittleEndian(pcm.AsSpan(sample * 2), signal.Samples.Span[sample]);
            var segment = new SequentialMediaSegment(0, SequentialSegmentKind.Samples, channelNumber: 0, dataRange: new MediaDataRange(0, pcm.Length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(pcm)));
            var wave = MediaImageDocument.CreateUnexplored(document.Source, TapeImageFormatIds.Wav, MediaKind.Tape, new SequentialMediaImageRepresentation(pcm.Length, segments: [segment]), [], new Dictionary<string, string> { ["channels"] = "1", ["sampleRate"] = signal.SampleRate.ToString(CultureInfo.InvariantCulture), ["bitsPerSample"] = "16", ["blockAlign"] = "2" });
            decoded = await decoders.DecodeAsync(wave, machineId, cancellationToken).ConfigureAwait(false);
        }
        var blocks = decoded.Blocks.Select(block =>
        {
            var position = block.Metadata.TryGetValue("samplePosition", out var sample) ? int.Parse(sample, CultureInfo.InvariantCulture) : block.SourceSegmentPositions.Select(sourcePosition => positions.GetValueOrDefault(sourcePosition)).FirstOrDefault();
            if (!block.Metadata.ContainsKey("samplePosition") && block.Metadata.TryGetValue("byteOffset", out var byteOffset))
            {
                var segment = representation.Segments?.FirstOrDefault(segment => block.SourceSegmentPositions.Contains(segment.Position));
                var baud = segment?.Metadata.GetValueOrDefault(UefConstants.BaudRateMetadataKey) ?? "1200";
                position += checked((int)Math.Round(int.Parse(byteOffset, CultureInfo.InvariantCulture) * 10.0 * signal.SampleRate / int.Parse(baud, CultureInfo.InvariantCulture)));
            }
            return new MediaCassetteBlock(position, block.Data, block.IntegrityValid, block.Metadata);
        }).ToArray();
        return signal with { Blocks = blocks, BlockPositions = blocks.Length == 0 ? signal.BlockPositions : blocks.Select(block => CassetteBlockFunctions.CarrierStart(signal, block.SamplePosition)).Distinct().Order().ToArray() };
    }

    /// <summary>Conserve l'ordre, les porteuses et les silences des chunks ; aucun fichier BASIC n'est injecté en RAM.</summary>
    internal static async Task<MediaCassetteSignal> UefAsync(SequentialMediaImageRepresentation image, Dictionary<long, int> positions, CancellationToken token)
    {
        // 48 kHz est divisible par 1 200 et 2 400 : les signaux BBC standard n'ont aucun arrondi de demi-onde.
        const int rate = 48000;
        var samples = new List<short>();
        short level = -UefConstants.PcmAmplitude;
        double remainder = 0;
        foreach (var segment in image.Segments ?? [])
        {
            token.ThrowIfCancellationRequested();
            var id = ushort.Parse(segment.Metadata[UefConstants.ChunkIdMetadataKey], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (id < 0x0100 || id is UefConstants.PositionMarker or UefConstants.TapeSetInformation or UefConstants.TapeSide) continue;
            var data = await BlockMediaDataReader.ReadRangeAsync(segment.DataRange, token).ConfigureAwait(false) ?? [];
            var baseFrequency = double.Parse(segment.Metadata[UefConstants.BaseFrequencyMetadataKey], CultureInfo.InvariantCulture);
            var baud = int.Parse(segment.Metadata[UefConstants.BaudRateMetadataKey], CultureInfo.InvariantCulture);
            switch (id)
            {
                case UefConstants.ImplicitData:
                    positions[segment.Position] = samples.Count;
                    foreach (var value in data) Frame(value);
                    break;
                case UefConstants.ExplicitData:
                    if (data.Length == 0 || data[0] is < 8 or > 15) throw new InvalidDataException("Invalid UEF explicit bit count.");
                    var bitCount = data.Length * 8 - data[0];
                    for (var bit = 0; bit < bitCount; bit++) Bit((data[1 + bit / 8] & (1 << (bit & 7))) != 0);
                    break;
                case UefConstants.DefinedData:
                    if (data.Length < 3 || data[0] is < 1 or > 8 || data[1] is not ((byte)'N' or (byte)'E' or (byte)'O')) throw new InvalidDataException("Invalid UEF serial format.");
                    var stops = (sbyte)data[2];
                    for (var index = 3; index < data.Length; index++)
                    {
                        Bit(false); var parity = false;
                        for (var bit = 0; bit < data[0]; bit++) { var value = (data[index] & (1 << bit)) != 0; Bit(value); parity ^= value; }
                        if (data[1] != 'N') Bit(parity != (data[1] == 'O'));
                        for (var stop = 0; stop < Math.Abs((int)stops); stop++) Bit(true);
                        if (stops < 0) Carrier(1);
                    }
                    break;
                case UefConstants.ImplicitAtomData:
                    for (var index = 0; index < data.Length; index++)
                    {
                        Frame(data[index]);
                        if (index + 1 < data.Length) Carrier(1);
                    }
                    break;
                case UefConstants.CarrierTone:
                    if (data.Length != 2) throw new InvalidDataException("Invalid UEF carrier.");
                    Carrier(BinaryPrimitives.ReadUInt16LittleEndian(data));
                    break;
                case UefConstants.CarrierToneWithDummy:
                    if (data.Length != 4) throw new InvalidDataException("Invalid UEF dummy carrier.");
                    Carrier(BinaryPrimitives.ReadUInt16LittleEndian(data));
                    Frame(0xaa);
                    Carrier(BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(2)));
                    break;
                case UefConstants.IntegerGap:
                    if (data.Length != 2) throw new InvalidDataException("Invalid UEF gap.");
                    var count = BinaryPrimitives.ReadUInt16LittleEndian(data);
                    // Unité définie par la spécification UEF historique et le lecteur de référence jsbeeb.
                    if (count != 0) Pulse(0, rate / (2 * count * baseFrequency));
                    break;
                case UefConstants.FloatingGap:
                    if (data.Length != 4) throw new InvalidDataException("Invalid UEF floating gap.");
                    Pulse(0, rate * (double)BinaryPrimitives.ReadSingleLittleEndian(data));
                    break;
                case UefConstants.SecurityCycles:
                    if (data.Length < 5) throw new InvalidDataException("Invalid UEF security cycles.");
                    var cycles = data[0] | data[1] << 8 | data[2] << 16;
                    if (data.Length < 5 + (cycles + 7) / 8) throw new InvalidDataException("Truncated UEF security cycles.");
                    for (var cycle = 0; cycle < cycles; cycle++)
                    {
                        var fast = (data[5 + cycle / 8] & (128 >> (cycle & 7))) != 0;
                        var half = rate / (2 * baseFrequency * (fast ? 2 : 1));
                        var highOnly = cycle == 0 && data[3] == 'P';
                        var lowOnly = cycle == cycles - 1 && data[4] == 'P';
                        var phase = int.Parse(segment.Metadata[UefConstants.PhaseMetadataKey], CultureInfo.InvariantCulture);
                        var positive = phase < 180 ? UefConstants.PcmAmplitude : (short)-UefConstants.PcmAmplitude;
                        if (!lowOnly) Pulse(positive, half);
                        if (!highOnly) Pulse((short)-positive, half);
                    }
                    break;
                case UefConstants.BaseFrequency: case UefConstants.BaudRate: break;
                case UefConstants.PhaseChange:
                    if (data.Length != 2) throw new InvalidDataException("Invalid UEF phase.");
                    level = BinaryPrimitives.ReadUInt16LittleEndian(data) % 360 < 180 ? UefConstants.PcmAmplitude : (short)-UefConstants.PcmAmplitude;
                    break;
                default: throw new NotSupportedException($"UEF signal chunk 0x{id:x4} is not supported.");
            }
            void Frame(byte value) { Bit(false); for (var bit = 0; bit < 8; bit++) Bit((value & (1 << bit)) != 0); Bit(true); }
            void Bit(bool value)
            {
                if (baud is not (300 or 1200)) throw new NotSupportedException("Unsupported UEF data encoding format.");
                var halves = (baud == 300 ? 4 : 1) * (value ? 4 : 2);
                for (var half = 0; half < halves; half++) { Pulse(level, rate / (2 * baseFrequency * (value ? 2 : 1))); level = (short)-level; }
            }
            void Carrier(int cycles)
            {
                for (var half = 0; half < cycles * 2; half++) { Pulse(level, rate / (4 * baseFrequency)); level = (short)-level; }
            }
            void Pulse(short value, double duration)
            {
                if (!double.IsFinite(duration) || duration < 0) throw new InvalidDataException("Invalid UEF pulse duration.");
                remainder += duration;
                var length = checked((int)Math.Round(remainder)); remainder -= length;
                if (length > 128 * 1024 * 1024 - samples.Count) throw new InvalidDataException("The UEF signal exceeds the supported memory limit.");
                samples.AddRange(Enumerable.Repeat(value, length));
            }
        }
        return new MediaCassetteSignal(samples.ToArray(), rate, positions.Values.ToArray());
    }

    /// <summary>Extrait les niveaux du premier canal PCM avec les validations et la lecture bornée existantes.</summary>
    private static async Task<MediaCassetteSignal> WaveAsync(MediaImageDocument document, SequentialMediaImageRepresentation image, CancellationToken token)
    {
        if (!PcmSampleFunctions.TryGetProfile(document.Metadata, out var channels, out var rate, out var bits, out var align)) throw new InvalidDataException("Invalid cassette PCM profile.");
        var samples = new List<short>();
        foreach (var segment in (image.Segments ?? []).Where(segment => segment.Kind == SequentialSegmentKind.Samples && segment.ChannelNumber == 0))
        {
            var bytes = await BlockMediaDataReader.ReadRangeAsync(segment.DataRange, token).ConfigureAwait(false) ?? [];
            foreach (var value in PcmSampleFunctions.ExtractChannel(bytes, channels, bits, align, 0)) samples.Add((short)(bits < 16 ? value << (16 - bits) : value >> (bits - 16)));
        }
        return new MediaCassetteSignal(samples.ToArray(), rate);
    }
}
