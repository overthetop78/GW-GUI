using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains the validated CloneCD descriptor structure required to expose its raw image.</summary>
public sealed class CloneCdDescriptor
{
    public CloneCdDescriptor(
        int version,
        int sessionCount,
        string? catalogNumber,
        IReadOnlyList<CloneCdTocEntry> tocEntries,
        IReadOnlyList<CueTrackDeclaration> tracks,
        IReadOnlyDictionary<int, IReadOnlyDictionary<string, int>> sessions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sessionCount);
        ArgumentNullException.ThrowIfNull(tocEntries);
        ArgumentNullException.ThrowIfNull(tracks);
        ArgumentNullException.ThrowIfNull(sessions);
        if (tocEntries.Count == 0) throw new ArgumentException("A CloneCD descriptor requires TOC entries.", nameof(tocEntries));
        if (tracks.Count == 0) throw new ArgumentException("A CloneCD descriptor requires tracks.", nameof(tracks));
        if (sessions.Count != sessionCount) throw new ArgumentException("The declared CloneCD session count does not match its sections.", nameof(sessions));

        Version = version;
        SessionCount = sessionCount;
        CatalogNumber = catalogNumber;
        TocEntries = new ReadOnlyCollection<CloneCdTocEntry>(tocEntries.ToArray());
        Tracks = new ReadOnlyCollection<CueTrackDeclaration>(tracks.ToArray());
        Sessions = new ReadOnlyDictionary<int, IReadOnlyDictionary<string, int>>(
            sessions.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyDictionary<string, int>)new ReadOnlyDictionary<string, int>(
                    new Dictionary<string, int>(pair.Value, StringComparer.OrdinalIgnoreCase))));
    }

    public int Version { get; }
    public int SessionCount { get; }
    public string? CatalogNumber { get; }
    public IReadOnlyList<CloneCdTocEntry> TocEntries { get; }
    public IReadOnlyList<CueTrackDeclaration> Tracks { get; }
    public IReadOnlyDictionary<int, IReadOnlyDictionary<string, int>> Sessions { get; }
}
