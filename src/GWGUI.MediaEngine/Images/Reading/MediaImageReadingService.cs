using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>Reads a media source through the registered recognition and format readers.</summary>
public sealed class MediaImageReadingService
{
    private readonly MediaRecognitionRegistry recognitionRegistry;

    public MediaImageReadingService(MediaRecognitionRegistry recognitionRegistry)
    {
        ArgumentNullException.ThrowIfNull(recognitionRegistry);
        this.recognitionRegistry = recognitionRegistry;
    }

    public async Task<MediaImageDocument> ReadAsync(MediaSourceDescriptor source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var context = new MediaRecognitionContext(source);
        var result = await recognitionRegistry.RecognizeAsync(context, cancellationToken).ConfigureAwait(false);
        return result.Document;
    }
}
