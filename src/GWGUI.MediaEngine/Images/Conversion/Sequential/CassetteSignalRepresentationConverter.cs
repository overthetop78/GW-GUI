using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sequential;
using GWGUI.MediaEngine.Interfaces.Conversion;

namespace GWGUI.MediaEngine.Images.Conversion.Sequential;

/// <summary>Convertit les signaux cassette sans exiger un système de fichiers ou un protocole reconnus.</summary>
/// <remarks>Le contenu d'une bande protégée reste un signal. L'analyse des fichiers est une opération séparée.
/// Les documents et leurs sources sont empruntés ; aucun fichier de sortie n'est créé ici.</remarks>
public sealed class CassetteSignalRepresentationConverter : IMediaRepresentationConverter
{
    public string Id => "cassette-signal";
    public IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };
    public IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };
    public bool CanConvert(MediaImageDocument source, string targetFormatId, MediaRepresentationKind targetRepresentationKind) => source.MediaKind == MediaKind.Tape && source.Representation is SequentialMediaImageRepresentation && targetRepresentationKind == MediaRepresentationKind.Sequential
        && (source.FormatId == TapeImageFormatIds.Uef && targetFormatId is TapeImageFormatIds.Wav or TapeImageFormatIds.Csw or TapeImageFormatIds.Raw
            || targetFormatId == TapeImageFormatIds.Uef && PcmSampleFunctions.TryGetProfile(source.Metadata, out _, out _, out _, out _));

    public async Task<MediaRepresentationConversionResult> ConvertAsync(MediaImageDocument source, string targetFormatId, MediaRepresentationKind targetRepresentationKind, IReadOnlyDictionary<string, string> options, Action<MediaExplorationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!CanConvert(source, targetFormatId, targetRepresentationKind)) throw new NotSupportedException("This cassette signal conversion is not supported.");
        var descriptor = source.Source with { Parameters = options.Count > 0 ? options : source.Source.Parameters };
        MediaImageDocument document;
        IReadOnlyList<string> losses = [];
        if (targetFormatId == TapeImageFormatIds.Uef)
        {
            var native = await CassettePcmFunctions.ChannelsAsync(source, cancellationToken).ConfigureAwait(false);
            var samples = native.Channels[0].Select(value => (short)(Math.Sign(value) * 16000)).ToArray();
            document = CassetteSignalFunctions.UefDocument(descriptor, new(samples, native.Rate), cancellationToken);
            losses = ["UEF retains signal timing and polarity rather than the original PCM amplitude."];
        }
        else
        {
            var signal = await CassetteSignalFunctions.UefAsync((SequentialMediaImageRepresentation)source.Representation, new Dictionary<long, int>(), cancellationToken).ConfigureAwait(false);
            document = CassettePcmFunctions.Document(descriptor, targetFormatId, signal);
        }
        return new(document, source.Diagnostics, losses);
    }
}
