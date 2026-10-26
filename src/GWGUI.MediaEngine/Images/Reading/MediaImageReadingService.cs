using MediaSourceDescriptor = GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Services;

namespace GWGUI.MediaEngine.Images.Reading;

/// <summary>Reads a media source through the registered recognition and format readers.</summary>
public sealed class MediaImageReadingService
{
    private readonly MediaRecognitionRegistry recognitionRegistry;

    /// <summary>Mécaniques des lecteurs utilisés par ce service de lecture.</summary>
    public MediaDriveService Drives { get; } = new();

    public MediaImageReadingService(MediaRecognitionRegistry recognitionRegistry)
    {
        ArgumentNullException.ThrowIfNull(recognitionRegistry);
        this.recognitionRegistry = recognitionRegistry;
    }

    public async Task<MediaImageDocument> ReadAsync(MediaSourceDescriptor source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var context = new MediaRecognitionContext(source) { Drives = Drives };
        try
        {
            var result = await recognitionRegistry.RecognizeAsync(context, cancellationToken).ConfigureAwait(false);
            return Drives.Attach(result.Document);
        }
        catch (Exception error) when (source.RequestedFormatId is null && context.Extension == ".raw" && (error is NotSupportedException || error is AggregateException aggregate && aggregate.InnerExceptions.All(inner => inner is InvalidDataException or NotSupportedException)))
        {
            // Un disque RAW reconnu reste un disque ; l'audio sans en-tête est proposé seulement en dernier recours.
            return await new GWGUI.MediaEngine.Images.Formats.Tape.Raw.RawTapeReader().ReadAsync(new MediaRecognitionContext(source with { RequestedFormatId = GWGUI.MediaEngine.Constants.TapeImageFormatIds.Raw }), cancellationToken).ConfigureAwait(false);
        }
    }
}
