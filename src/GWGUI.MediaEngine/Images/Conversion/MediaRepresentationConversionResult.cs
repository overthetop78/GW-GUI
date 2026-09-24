using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Images.Conversion;

/// <summary>Contains an in-memory conversion result before the target image is written.</summary>
public sealed class MediaRepresentationConversionResult
{
    public MediaRepresentationConversionResult(
        MediaImageDocument document,
        IReadOnlyList<string> diagnostics,
        IReadOnlyList<string> losses)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(losses);
        if (diagnostics.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A conversion diagnostic cannot be empty.", nameof(diagnostics));
        if (losses.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A declared conversion loss cannot be empty.", nameof(losses));

        Document = document;
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
        Losses = new ReadOnlyCollection<string>(losses.ToArray());
    }

    public MediaImageDocument Document { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public IReadOnlyList<string> Losses { get; }
}
