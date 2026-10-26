using System.IO;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.MediaEngine.Images.Reading.Recognition;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Raw;

/// <summary>Lecture RAW explicitement choisie : l'extension ne permet pas de distinguer un disque brut d'un audio brut.</summary>
public sealed class RawTapeReader : IMediaImageReader
{
    public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { TapeImageFormatIds.Raw };
    public IReadOnlySet<string> Extensions { get; } = new HashSet<string> { ".raw" };
    public IReadOnlyList<ReadOnlyMemory<byte>> Signatures => [];
    public IReadOnlySet<string> AssociatedFileExtensions { get; } = new HashSet<string>();
    public IReadOnlySet<MediaKind> MediaKinds { get; } = new HashSet<MediaKind> { MediaKind.Tape };
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };
    public bool SupportsFormatId(string formatId) => FormatIds.Contains(formatId);
    public ValueTask<bool> CanReadAsync(MediaRecognitionContext context, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return ValueTask.FromResult(context.RequestedFormatId == TapeImageFormatIds.Raw);
    }
    public async Task<MediaImageDocument> ReadAsync(MediaRecognitionContext context, CancellationToken token)
    {
        var profile = CassettePcmProfile.Read(context.Source.Parameters);
        var bytes = await File.ReadAllBytesAsync(context.Source.PrimaryPath, token).ConfigureAwait(false);
        return CassettePcmFunctions.RawDocument(context.Source, bytes, profile);
    }
}
