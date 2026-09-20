namespace GWGUI.MediaFileSystems.Contracts;

/// <summary>Volume reconnu et fichiers réellement lus par un lecteur de systèmes de fichiers.</summary>
public sealed record ExploredFileSystemVolume(
    MediaVolumeDescriptor Descriptor,
    string? ReaderId,
    FileSystemVolume? FileSystem,
    IReadOnlyList<string> Diagnostics);
