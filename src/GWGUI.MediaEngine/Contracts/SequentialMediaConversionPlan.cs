using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains an encoded sequential target and all diagnostics or losses known before writing it.</summary>
public sealed class SequentialMediaConversionPlan
{
    public SequentialMediaConversionPlan(
        MediaImageDocument targetDocument,
        IReadOnlyList<string> diagnostics,
        IReadOnlyList<string> losses)
    {
        ArgumentNullException.ThrowIfNull(targetDocument);
        ArgumentNullException.ThrowIfNull(diagnostics);
        ArgumentNullException.ThrowIfNull(losses);
        if (diagnostics.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A conversion diagnostic cannot be empty.", nameof(diagnostics));
        if (losses.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A declared conversion loss cannot be empty.", nameof(losses));
        TargetDocument = targetDocument;
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
        Losses = new ReadOnlyCollection<string>(losses.ToArray());
    }

    public MediaImageDocument TargetDocument { get; }
    public IReadOnlyList<string> Diagnostics { get; }
    public IReadOnlyList<string> Losses { get; }
}
