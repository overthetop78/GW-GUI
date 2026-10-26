using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Interfaces.Writing;

namespace GWGUI.MediaEngine.Images.Formats.Tape.Raw;

/// <summary>Écrit un signal sans en-tête avec un profil numérique explicite, par la publication atomique commune.</summary>
public sealed class RawTapeWriter(IAtomicImageFileWriter? files = null) : IMediaImageWriter
{
    private readonly IAtomicImageFileWriter _files = files ?? new AtomicImageFileWriter();
    public string Id => MediaImageWriterIds.TapeRaw;
    public IReadOnlySet<string> FormatIds { get; } = new HashSet<string> { TapeImageFormatIds.Raw };
    public IReadOnlySet<MediaRepresentationKind> RepresentationKinds { get; } = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Sequential };
    public IReadOnlySet<string> ProducedFileExtensions { get; } = new HashSet<string> { ".raw" };
    public bool ProducesMultipleFiles => false;
    public bool CanWrite(MediaImageDocument document, string format, string extension) => document.MediaKind == MediaKind.Tape && document.Representation.RepresentationKind == MediaRepresentationKind.Sequential && FormatIds.Contains(format) && ProducedFileExtensions.Contains(extension.ToLowerInvariant()) && PcmSampleFunctions.TryGetProfile(document.Metadata, out _, out _, out _, out _);
    public async Task<IReadOnlyList<string>> WriteAsync(MediaImageDocument document, string path, string format, CancellationToken token = default)
    {
        if (!CanWrite(document, format, Path.GetExtension(path))) throw new InvalidDataException("A PCM cassette document is required.");
        var bytes = await CassettePcmFunctions.EncodeDocumentAsync(document, CassettePcmProfile.Read(document.Source.Parameters ?? document.Metadata), token).ConfigureAwait(false);
        await _files.WriteAsync(path, (stream, cancellation) => stream.WriteAsync(bytes, cancellation).AsTask(), token).ConfigureAwait(false);
        return [path];
    }
}
