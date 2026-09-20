using System.Collections.ObjectModel;

namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Contains the volumes and diagnostics produced by one matching volume detector.</summary>
public sealed class MediaVolumeDetectionResult
{
    public MediaVolumeDetectionResult(
        IReadOnlyList<MediaVolumeDescriptor> volumes,
        IReadOnlyList<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(volumes);
        ArgumentNullException.ThrowIfNull(diagnostics);
        if (volumes.Any(volume => volume is null))
            throw new ArgumentException("A detected volume cannot be null.", nameof(volumes));
        if (diagnostics.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A volume diagnostic cannot be empty.", nameof(diagnostics));

        Volumes = new ReadOnlyCollection<MediaVolumeDescriptor>(volumes.ToArray());
        Diagnostics = new ReadOnlyCollection<string>(diagnostics.ToArray());
    }

    public IReadOnlyList<MediaVolumeDescriptor> Volumes { get; }

    public IReadOnlyList<string> Diagnostics { get; }
}
