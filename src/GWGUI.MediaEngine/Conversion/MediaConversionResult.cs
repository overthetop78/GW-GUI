using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Conversion;

/// <summary>Reports the files, diagnostics, and declared information losses produced by a conversion.</summary>
public sealed class MediaConversionResult
{
    public MediaConversionResult(
        IReadOnlyList<string> producedFiles,
        IReadOnlyList<string> diagnostics,
        IReadOnlyList<string> losses)
    {
        ArgumentNullException.ThrowIfNull(producedFiles);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(losses);
        if (producedFiles.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("A produced file path cannot be empty.", nameof(producedFiles));
        if (losses.Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("A declared conversion loss cannot be empty.", nameof(losses));

        ProducedFiles = new ReadOnlyCollection<string>(producedFiles.ToArray());
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
        Losses = new ReadOnlyCollection<string>(losses.ToArray());
    }

    public IReadOnlyList<string> ProducedFiles { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public IReadOnlyList<string> Losses { get; }
}
