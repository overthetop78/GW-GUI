using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Décrit un système de fichiers reconnu dans une interprétation sectorielle.</summary>
/// <param name="FormatId">Identifiant du format de l'image sectorielle.</param>
/// <param name="ReaderId">Identifiant réel du lecteur de système de fichiers.</param>
/// <param name="Volume">Volume produit par le lecteur.</param>
public sealed record ExploredFileSystem(string FormatId, string ReaderId, FileSystemVolume Volume);
