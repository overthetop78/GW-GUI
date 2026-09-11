using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Recognition;

/// <summary>Contains the selected reader, its document, and the failures reported by the other attempted candidates.</summary>
public sealed class MediaRecognitionResult
{
    public MediaRecognitionResult(
        IMediaImageReader reader,
        MediaImageDocument document,
        IReadOnlyDictionary<IMediaImageReader, Exception> candidateFailures)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(candidateFailures);

        Reader = reader;
        Document = document;
        CandidateFailures = new ReadOnlyDictionary<IMediaImageReader, Exception>(new Dictionary<IMediaImageReader, Exception>(candidateFailures));
    }

    public IMediaImageReader Reader { get; }

    public MediaImageDocument Document { get; }

    public IReadOnlyDictionary<IMediaImageReader, Exception> CandidateFailures { get; }
}
