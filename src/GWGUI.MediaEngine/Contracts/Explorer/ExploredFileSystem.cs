using GWGUI.MediaEngine.Contracts.Explorer;

using GWGUI.MediaEngine.Images.Models.Sectors;

namespace GWGUI.MediaEngine.Contracts.Explorer;

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
