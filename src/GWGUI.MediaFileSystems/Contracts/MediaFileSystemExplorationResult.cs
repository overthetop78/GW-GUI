namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Volumes détectés et fichiers retrouvés dans un média déjà décodé.</summary>
public sealed record MediaFileSystemExplorationResult(
    IReadOnlyList<MediaVolumeDescriptor> Volumes,
    IReadOnlyList<ExploredFileSystemVolume> ExploredVolumes,
    IReadOnlyList<string> Diagnostics);
