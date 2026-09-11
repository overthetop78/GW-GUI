using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Composition;

/// <summary>Provides the registered file-system readers and the common media explorer.</summary>
public sealed class MediaExplorationComposition
{
    private MediaExplorationComposition(FileSystemRegistry fileSystems)
    {
        FileSystems = fileSystems;
        Explorer = new MediaExplorer(fileSystems);
    }

    public FileSystemRegistry FileSystems { get; }

    public MediaExplorer Explorer { get; }

    public static MediaExplorationComposition CreateDefault() => new(new FileSystemRegistry());
}
