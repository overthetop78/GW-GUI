using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains the understood structure and metadata of one parsed CUE sheet.</summary>
public sealed class CueSheetDocument
{
    public CueSheetDocument(
        IReadOnlyList<CueFileDescriptor> files,
        string? catalogNumber = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(files);
        if (files.Count == 0) throw new ArgumentException("A CUE sheet requires at least one file declaration.", nameof(files));
        var trackNumbers = files.SelectMany(file => file.Tracks).Select(track => track.Number).ToArray();
        if (trackNumbers.Distinct().Count() != trackNumbers.Length)
            throw new ArgumentException("CUE track numbers must be unique across the sheet.", nameof(files));

        Files = new ReadOnlyCollection<CueFileDescriptor>(files.ToArray());
        CatalogNumber = catalogNumber;
        Metadata = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public IReadOnlyList<CueFileDescriptor> Files { get; }
    public string? CatalogNumber { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
