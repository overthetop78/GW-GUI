using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Images.Reading.Recognition;

/// <summary>Ranks media reader candidates and delegates complete validation to each specialized reader.</summary>
public sealed class MediaRecognitionRegistry
{
    private readonly IReadOnlyList<IMediaImageReader> readers;

    public MediaRecognitionRegistry(IReadOnlyList<IMediaImageReader> readers)
    {
        ArgumentNullException.ThrowIfNull(readers);
        if (readers.Any(reader => reader is null)) throw new ArgumentException("A media image reader cannot be null.", nameof(readers));
        this.readers = readers.ToArray();
    }

    public async Task<MediaRecognitionResult> RecognizeAsync(MediaRecognitionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var candidates = await SelectCandidatesAsync(context, cancellationToken).ConfigureAwait(false);
        var failures = new Dictionary<IMediaImageReader, Exception>();

        foreach (var candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var document = await candidate.Reader.ReadAsync(context, cancellationToken).ConfigureAwait(false);
                return new MediaRecognitionResult(candidate.Reader, document, failures);
            }
            catch (Exception exception) when (exception is InvalidDataException or NotSupportedException)
            {
                failures[candidate.Reader] = exception;
            }
        }

        if (failures.Count > 0) throw new AggregateException("All selected media image readers rejected the source.", failures.Values);
        throw new NotSupportedException($"No media image reader accepted '{context.Source.PrimaryPath}'.");
    }

    public async Task<IReadOnlyList<MediaRecognitionCandidate>> SelectCandidatesAsync(MediaRecognitionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        var candidates = new List<(MediaRecognitionCandidate Candidate, int EvidenceCount, int RegistrationOrder)>();
        var registrationOrder = 0;

        foreach (var reader in readers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var reasons = new List<string>();
            var confidence = 0d;

            if (!string.IsNullOrWhiteSpace(context.RequestedFormatId))
            {
                if (!reader.SupportsFormatId(context.RequestedFormatId))
                {
                    registrationOrder++;
                    continue;
                }

                confidence = 1d;
                reasons.Add("explicit format");
            }

            if (reader.Extensions.Contains(context.Extension, StringComparer.OrdinalIgnoreCase))
            {
                confidence = Math.Max(confidence, 0.5d);
                reasons.Add("file extension");
            }

            if (context.Source.AssociatedPaths.Any(path => reader.AssociatedFileExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)))
            {
                confidence = Math.Max(confidence, 0.55d);
                reasons.Add("associated file");
            }

            if (await MatchesSignatureAsync(reader, context, cancellationToken).ConfigureAwait(false))
            {
                confidence = Math.Max(confidence, 0.9d);
                reasons.Add("file signature");
            }

            if (await reader.CanReadAsync(context, cancellationToken).ConfigureAwait(false))
            {
                confidence = Math.Max(confidence, 0.8d);
                reasons.Add("reader probe");
            }

            if (reasons.Count > 0) candidates.Add((new MediaRecognitionCandidate(reader, confidence, string.Join(", ", reasons)), reasons.Count, registrationOrder));
            registrationOrder++;
        }

        return candidates
            .OrderByDescending(item => item.Candidate.Confidence)
            .ThenByDescending(item => item.EvidenceCount)
            .ThenBy(item => item.RegistrationOrder)
            .Select(item => item.Candidate)
            .ToArray();
    }

    private static async ValueTask<bool> MatchesSignatureAsync(IMediaImageReader reader, MediaRecognitionContext context, CancellationToken cancellationToken)
    {
        var maximumLength = reader.Signatures.Count == 0 ? 0 : reader.Signatures.Max(signature => signature.Length);
        if (maximumLength == 0 || context.Length < maximumLength) return false;
        var header = await context.ReadHeaderAsync(maximumLength, cancellationToken).ConfigureAwait(false);
        return reader.Signatures.Any(signature => header.Span.StartsWith(signature.Span));
    }
}
