using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Exploration;

/// <summary>Explores the volumes of an already recognized media image without recognizing its format again.</summary>
public sealed class MediaExplorer
{
    private readonly FileSystemRegistry fileSystems;

    public MediaExplorer(FileSystemRegistry fileSystems)
    {
        ArgumentNullException.ThrowIfNull(fileSystems);
        this.fileSystems = fileSystems;
    }

    public ExploredMediaImage Explore(MediaImageDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var volumes = ResolveVolumes(document, out var volumeDiagnostic);
        var explored = volumes.Select(volume => fileSystems.Explore(document, volume)).ToArray();
        var diagnostics = volumeDiagnostic is null
            ? document.Diagnostics
            : document.Diagnostics.Append(volumeDiagnostic).ToArray();
        return new(document, explored, diagnostics);
    }

    private static IReadOnlyList<MediaVolumeDescriptor> ResolveVolumes(
        MediaImageDocument document,
        out string? diagnostic)
    {
        if (document.Volumes.Count > 0)
        {
            diagnostic = null;
            return document.Volumes;
        }

        var length = document.Representation.LogicalLength ??
                     document.Source.KnownLength ??
                     SourceLength(document.Source.PrimaryPath);
        if (length is null or <= 0)
        {
            diagnostic = "The media representation does not expose an addressable volume length.";
            return [];
        }

        diagnostic = null;
        return [new MediaVolumeDescriptor(0, length.Value, MediaVolumeOrigins.WholeMedia)];
    }

    private static long? SourceLength(string path) => File.Exists(path) ? new FileInfo(path).Length : null;
}
