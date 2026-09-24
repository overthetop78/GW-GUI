using GWGUI.MediaEngine.Interfaces.Reading;

namespace GWGUI.MediaEngine.Images.Reading.Recognition;

/// <summary>Associates a reader candidate with its preselection confidence and reason.</summary>
public sealed record MediaRecognitionCandidate
{
    public MediaRecognitionCandidate(IMediaImageReader reader, double confidence, string reason)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (double.IsNaN(confidence) || confidence < 0 || confidence > 1) throw new ArgumentOutOfRangeException(nameof(confidence));
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        Reader = reader;
        Confidence = confidence;
        Reason = reason;
    }

    public IMediaImageReader Reader { get; }

    public double Confidence { get; }

    public string Reason { get; }
}
