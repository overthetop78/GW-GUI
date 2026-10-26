using System.Globalization;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Formats.Tape;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Repères de blocs sur les bandes PCM, obtenus par les décodeurs communs et conservés pour la présentation.</summary>
internal static class CassetteBlockFunctions
{
    /// <summary>Replace le repère avant la porteuse BBC précédant le start, pour ne pas sauter la détection matérielle.</summary>
    internal static int CarrierStart(MediaCassetteSignal signal, int position)
    {
        var samples = signal.Samples.Span;
        position = Math.Clamp(position, 0, samples.Length);
        var previous = position; var earliest = position; var edges = 0;
        for (var sample = position - 1; sample > 0; sample--)
        {
            if (samples[sample] == 0) break;
            if ((samples[sample] > 0) == (samples[sample - 1] > 0)) continue;
            var width = previous - sample;
            if (edges > 0 && (width < signal.SampleRate / 6500.0 || width > signal.SampleRate / 3500.0)) break;
            earliest = previous = sample; edges++;
        }
        return edges > 10 ? earliest : position;
    }

    /// <summary>Découpe la présentation PCM aux starts validés sans modifier ni réencoder les données du fichier.</summary>
    internal static async Task<MediaImageDocument> IndexAsync(MediaImageDocument document, CancellationToken token)
    {
        if (document.FormatId == TapeImageFormatIds.Uef && document.Representation is SequentialMediaImageRepresentation sourceImage && (sourceImage.Segments ?? []).Any(segment => segment.Metadata.GetValueOrDefault(UefConstants.ChunkIdMetadataKey) == UefConstants.SecurityCycles.ToString("x4", CultureInfo.InvariantCulture)))
        {
            var signal = await CassetteSignalFunctions.UefAsync(sourceImage, new Dictionary<long, int>(), token).ConfigureAwait(false);
            document = CassettePcmFunctions.Document(document.Source, TapeImageFormatIds.Uef, signal);
        }
        if (document.FormatId is not (TapeImageFormatIds.Wav or TapeImageFormatIds.Csw or TapeImageFormatIds.Raw or TapeImageFormatIds.Uef) || document.Representation is not SequentialMediaImageRepresentation { Segments: { Count: > 0 } segments } image || !PcmSampleFunctions.TryGetProfile(document.Metadata, out _, out var rate, out _, out var align)) return document;
        // Le découpage conserve les canaux et les données originales ; les décodeurs apportent seulement des repères.
        var registry = SequentialMediaComposition.CreateDefault().Decoders;
        IReadOnlyList<SequentialDecodedBlock> blocks = [];
        foreach (var decoder in registry.FindCompatible(document))
        {
            var decoded = await decoder.DecodeAsync(document, cancellationToken: token).ConfigureAwait(false);
            if (decoded.Blocks.Count(block => block.IntegrityValid == true) > blocks.Count(block => block.IntegrityValid == true)) blocks = decoded.Blocks;
        }
        var indexed = blocks.Where(block => block.IntegrityValid == true && block.Metadata.ContainsKey("samplePosition")).OrderBy(block => int.Parse(block.Metadata["samplePosition"], CultureInfo.InvariantCulture)).ToArray();
        if (indexed.Length == 0) return document;
        var selected = new List<SequentialMediaSegment>();
        foreach (var segment in segments)
        {
            if (segment.Kind != SequentialSegmentKind.Samples || segment.DataRange is not { Source: { } source } range) { selected.Add(segment); continue; }
            var start = checked((int)Math.Round((segment.Start ?? TimeSpan.Zero).TotalSeconds * rate));
            var length = checked((int)(range.Length / align));
            var points = indexed.Select(block => int.Parse(block.Metadata["samplePosition"], CultureInfo.InvariantCulture)).Where(point => point > start && point < start + length).Prepend(start).Append(start + length).Distinct().Order().ToArray();
            for (var index = 0; index < points.Length - 1; index++)
            {
                var metadata = new Dictionary<string, string>(segment.Metadata);
                var block = indexed.FirstOrDefault(block => block.Metadata["samplePosition"] == points[index].ToString(CultureInfo.InvariantCulture));
                if (block is not null) foreach (var pair in block.Metadata) metadata[pair.Key] = pair.Value;
                selected.Add(new(points[index], SequentialSegmentKind.Samples, points[index + 1] - points[index], TimeSpan.FromSeconds((double)points[index] / rate), TimeSpan.FromSeconds((double)(points[index + 1] - points[index]) / rate), channelNumber: segment.ChannelNumber, dataRange: new MediaDataRange(0, (long)(points[index + 1] - points[index]) * align, MediaDataRangeKind.Stored, source, range.SourceOffset + (long)(points[index] - start) * align), metadata: metadata));
            }
        }
        return new(document.Source, document.FormatId, document.MediaKind, new SequentialMediaImageRepresentation(image.LogicalLength, image.Duration, selected), document.FileSystemVolumes, document.Diagnostics, document.Metadata);
    }
}
