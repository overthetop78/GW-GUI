using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential;

using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Images.Writing;

namespace GWGUI.MediaEngine.Images.Conversion.Sequential;

/// <summary>Chains sequential reading, protocol decoding, target encoding, and writing while declaring losses first.</summary>
public sealed class SequentialMediaConversionService
{
    private readonly MediaImageReadingService reading;
    private readonly SequentialDecoderRegistry decoders;
    private readonly SequentialEncoderRegistry encoders;
    private readonly MediaImageWriterRegistry writers;
    private readonly MediaImageWritingService writing;

    public SequentialMediaConversionService(
        MediaImageReadingService reading,
        SequentialDecoderRegistry decoders,
        SequentialEncoderRegistry encoders,
        MediaImageWriterRegistry writers,
        MediaImageWritingService writing)
    {
        ArgumentNullException.ThrowIfNull(reading);
        ArgumentNullException.ThrowIfNull(decoders);
        ArgumentNullException.ThrowIfNull(encoders);
        ArgumentNullException.ThrowIfNull(writers);
        ArgumentNullException.ThrowIfNull(writing);
        this.reading = reading;
        this.decoders = decoders;
        this.encoders = encoders;
        this.writers = writers;
        this.writing = writing;
    }

    public async Task<IReadOnlyList<MediaConversionDestination>> GetAvailableDestinationsAsync(
        MediaImageDocument source,
        string? machineId = null,
        IReadOnlyDictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.MediaKind != MediaKind.Tape || source.Representation.RepresentationKind != MediaRepresentationKind.Sequential)
            return [];
        var resolvedMachineId = ResolveMachineId(source, machineId);
        if (resolvedMachineId is null) return [];
        var decoded = await decoders.DecodeAsync(source, resolvedMachineId, cancellationToken).ConfigureAwait(false);
        var selectedParameters = parameters ?? new Dictionary<string, string>(StringComparer.Ordinal);
        var destinations = new List<MediaConversionDestination>();
        foreach (var writer in writers.Writers)
        foreach (var formatId in writer.FormatIds)
        {
            var request = SelectEncodableRequest(decoded, formatId, resolvedMachineId, selectedParameters);
            if (request is null) continue;
            foreach (var extension in writer.ProducedFileExtensions)
                destinations.Add(new MediaConversionDestination(formatId, extension, writer.Id, writer.ProducesMultipleFiles));
        }
        return destinations
            .Distinct()
            .OrderBy(destination => destination.FormatId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(destination => destination.Extension, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<SequentialMediaConversionPlan> PlanAsync(
        MediaImageDocument source,
        string targetFormatId,
        string? machineId = null,
        IReadOnlyDictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetFormatId);
        if (source.MediaKind != MediaKind.Tape || source.Representation is not SequentialMediaImageRepresentation)
            throw new NotSupportedException("The source is not a sequential tape image.");

        var resolvedMachineId = ResolveMachineId(source, machineId)
            ?? throw new InvalidOperationException("The sequential source requires an explicit machine selection.");
        var decoded = await decoders.DecodeAsync(source, resolvedMachineId, cancellationToken).ConfigureAwait(false);
        var selectedParameters = parameters ?? new Dictionary<string, string>(StringComparer.Ordinal);
        var request = SelectEncodableRequest(decoded, targetFormatId, resolvedMachineId, selectedParameters)
            ?? throw new NotSupportedException(
                $"No sequential encoder can convert '{source.FormatId}' to '{targetFormatId}' for '{resolvedMachineId}'.");
        var encoded = await encoders.EncodeAsync(request, cancellationToken).ConfigureAwait(false);
        var losses = new List<string>();
        if (decoded.UndecodedSegments.Count > 0 && request.RetainedSegments.Count == 0)
            losses.Add($"{decoded.UndecodedSegments.Count} source segment(s) cannot be represented by the selected target format.");
        if (encoded.Segments?.Any(segment =>
                segment.Metadata.TryGetValue("reconstructedSignal", out var reconstructed)
                && bool.TryParse(reconstructed, out var isReconstructed)
                && isReconstructed) == true)
            losses.Add("The target signal timing is a standard reconstruction rather than the original recording timing.");

        var metadata = BuildTargetMetadata(selectedParameters, encoded);
        var target = new MediaImageDocument(
            source.Source,
            targetFormatId,
            MediaKind.Tape,
            encoded,
            [],
            decoded.Diagnostics,
            metadata);
        return new SequentialMediaConversionPlan(
            target,
            decoded.Diagnostics.Distinct(StringComparer.Ordinal).ToArray(),
            losses.Distinct(StringComparer.Ordinal).ToArray());
    }

    public async Task<MediaConversionResult> ConvertAsync(
        string sourcePath,
        string outputPath,
        string targetFormatId,
        string? machineId = null,
        IReadOnlyDictionary<string, string>? parameters = null,
        bool acceptLosses = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var source = await reading.ReadAsync(new MediaSourceDescriptor(sourcePath, []), cancellationToken).ConfigureAwait(false);
        var plan = await PlanAsync(source, targetFormatId, machineId, parameters, cancellationToken).ConfigureAwait(false);
        if (plan.Losses.Count > 0 && !acceptLosses)
            throw new InvalidOperationException("The sequential conversion has declared information losses that were not accepted.");
        var extension = Path.GetExtension(outputPath);
        if (writers.Resolve(plan.TargetDocument, targetFormatId, extension) is null)
            throw new NotSupportedException(
                $"No writer accepts sequential target '{targetFormatId}' with extension '{extension}'.");
        var produced = await writing.WriteAsync(plan.TargetDocument, outputPath, targetFormatId, cancellationToken).ConfigureAwait(false);
        return new MediaConversionResult(produced, plan.Diagnostics, plan.Losses);
    }

    private SequentialEncodeRequest? SelectEncodableRequest(
        SequentialDecodeResult decoded,
        string targetFormatId,
        string machineId,
        IReadOnlyDictionary<string, string> parameters)
    {
        var preserving = new SequentialEncodeRequest(
            targetFormatId,
            machineId,
            decoded.Blocks,
            decoded.UndecodedSegments,
            parameters);
        if (encoders.FindCompatible(preserving).Count == 1) return preserving;
        var lossy = new SequentialEncodeRequest(targetFormatId, machineId, decoded.Blocks, [], parameters);
        return encoders.FindCompatible(lossy).Count == 1 ? lossy : null;
    }

    private string? ResolveMachineId(MediaImageDocument source, string? requestedMachineId)
    {
        if (!string.IsNullOrWhiteSpace(requestedMachineId)) return requestedMachineId;
        var machineIds = decoders.FindCompatible(source)
            .SelectMany(decoder => decoder.MachineIds)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return machineIds.Length == 1 ? machineIds[0] : null;
    }

    private static IReadOnlyDictionary<string, string> BuildTargetMetadata(
        IReadOnlyDictionary<string, string> parameters,
        SequentialMediaImageRepresentation representation)
    {
        var metadata = new Dictionary<string, string>(parameters, StringComparer.Ordinal);
        var samples = representation.Segments?.FirstOrDefault(segment => segment.Kind == SequentialSegmentKind.Samples);
        if (samples is not null)
        {
            metadata["channels"] = "1";
            CopyMetadata(samples.Metadata, metadata, "sampleRate");
            CopyMetadata(samples.Metadata, metadata, "bitsPerSample");
            CopyMetadata(samples.Metadata, metadata, "blockAlign");
        }
        return metadata;
    }

    private static void CopyMetadata(
        IReadOnlyDictionary<string, string> source,
        IDictionary<string, string> destination,
        string key)
    {
        if (source.TryGetValue(key, out var value)) destination[key] = value;
    }
}
