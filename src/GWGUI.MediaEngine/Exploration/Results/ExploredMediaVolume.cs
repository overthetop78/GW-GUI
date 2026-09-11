using System.Collections.ObjectModel;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Associates one media volume with its recognized file system and candidate diagnostics.</summary>
public sealed class ExploredMediaVolume
{
    public ExploredMediaVolume(
        MediaVolumeDescriptor descriptor,
        string? readerId,
        FileSystemVolume? fileSystem,
        IReadOnlyList<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (fileSystem is not null && string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("A recognized file system requires its reader identifier.", nameof(readerId));

        Descriptor = descriptor;
        ReaderId = readerId;
        FileSystem = fileSystem;
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
    }

    public MediaVolumeDescriptor Descriptor { get; }

    public string? ReaderId { get; }

    public FileSystemVolume? FileSystem { get; }

    public IReadOnlyList<string> Diagnostics { get; }
}
