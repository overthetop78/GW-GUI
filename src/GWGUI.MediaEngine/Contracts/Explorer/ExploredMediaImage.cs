using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Contracts;

namespace GWGUI.MediaEngine.Contracts.Explorer;

/// <summary>Contains one recognized media document and the file-system exploration of its volumes.</summary>
public sealed class ExploredMediaImage
{
    public ExploredMediaImage(
        MediaImageDocument document,
        IReadOnlyList<ExploredMediaVolume> volumes,
        IReadOnlyList<string> diagnostics,
        string? firstFileName = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volumes);
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (volumes.Any(volume => volume is null)) throw new ArgumentException("An explored volume cannot be null.", nameof(volumes));

        Document = document;
        Volumes = new ReadOnlyCollection<ExploredMediaVolume>(volumes.ToArray());
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
        FirstFileName = firstFileName;
    }

    public MediaImageDocument Document { get; }

    public IReadOnlyList<ExploredMediaVolume> Volumes { get; }

    public IReadOnlyList<string> Diagnostics { get; }

    public string? FirstFileName { get; }
}
