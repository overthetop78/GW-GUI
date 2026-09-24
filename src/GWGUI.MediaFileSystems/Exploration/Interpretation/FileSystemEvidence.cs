using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.Exploration.Interpretation;

/// <summary>Évalue un volume lu, en conservant le score physique fourni par le décodeur.</summary>
public readonly record struct FileSystemEvidence(int EntryCount, int WarningCount, double DecodeScore)
{
    public static FileSystemEvidence From(IFileSystemVolumeView volume, double decodeScore)
    {
        ArgumentNullException.ThrowIfNull(volume);
        return new(CountEntries(volume.Entries), volume.Warnings.Count, decodeScore);
    }

    public bool IsBetterThan(FileSystemEvidence other) =>
        EntryCount > other.EntryCount ||
        (EntryCount == other.EntryCount && WarningCount < other.WarningCount) ||
        (EntryCount == other.EntryCount && WarningCount == other.WarningCount && DecodeScore > other.DecodeScore);

    private static int CountEntries(IEnumerable<IFileSystemEntryView> entries) =>
        entries.Sum(entry => 1 + CountEntries(entry.Children));
}
