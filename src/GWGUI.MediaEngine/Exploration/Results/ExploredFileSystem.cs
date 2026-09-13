using GWGUI.MediaEngine.FileSystems;

using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.MediaEngine.Exploration.Results;

/// <summary>Décrit un système de fichiers reconnu dans une interprétation sectorielle.</summary>
/// <param name="ReaderId">Identifiant réel du lecteur de système de fichiers.</param>
/// <param name="Image">Image sectorielle exacte remise au lecteur.</param>
/// <param name="Volume">Volume produit par le lecteur.</param>
public sealed record ExploredFileSystem(
    string ReaderId,
    SectorImage Image,
    FileSystemVolume Volume)
{
    /// <summary>Identifiant du format ayant réellement produit le volume.</summary>
    public string FormatId => Image.FormatId;
}
