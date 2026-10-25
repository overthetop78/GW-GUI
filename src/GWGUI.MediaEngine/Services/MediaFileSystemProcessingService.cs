using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaFileSystems.Functions;
using FileSystemsMediaExplorer = GWGUI.MediaFileSystems.Exploration.MediaExplorer;

namespace GWGUI.MediaEngine.Services;

/// <summary>Relaie les opérations existantes sans exposer MediaFileSystems à l'application.</summary>
internal sealed class MediaFileSystemProcessingService : IMediaFileSystemProcessing
{
    /// <inheritdoc/>
    public int ReadIntOrDefault(IReadOnlyDictionary<string, string> metadata, string key) => MediaMetadataFunctions.ReadIntOrDefault(metadata, key);

    /// <inheritdoc/>
    public IEnumerable<TEntry> EnumerateFiles<TEntry>(TEntry entry, Func<TEntry, bool> isFile, Func<TEntry, IReadOnlyList<TEntry>> children) => FileSystemsMediaExplorer.EnumerateFiles(entry, isFile, children);
}
