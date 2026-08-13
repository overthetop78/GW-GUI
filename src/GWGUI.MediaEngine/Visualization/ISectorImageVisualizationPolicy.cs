using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Visualization;

/// <summary>Décrit les décisions nécessaires pour convertir une image sectorielle en pistes visualisables.</summary>
internal interface ISectorImageVisualizationPolicy
{
    /// <summary>Indique si la politique accepte l'image.</summary>
    /// <param name="image">Image sectorielle à examiner.</param>
    /// <returns><see langword="true"/> lorsque la politique est compatible.</returns>
    bool CanHandle(SectorImage image);
    /// <summary>Retourne l'identifiant de l'encodeur à employer.</summary>
    /// <param name="image">Image sectorielle reconnue.</param>
    /// <returns>Identifiant technique de l'encodeur.</returns>
    string EncoderId(SectorImage image);
    /// <summary>Convertit une adresse logique en adresse de piste visualisée.</summary>
    /// <param name="image">Image sectorielle reconnue.</param>
    /// <param name="address">Adresse logique initiale.</param>
    /// <returns>Adresse de visualisation.</returns>
    SectorAddress VisualAddress(SectorImage image, SectorAddress address);
    /// <summary>Construit les secteurs physiques d'une piste.</summary>
    /// <param name="image">Image sectorielle reconnue.</param>
    /// <param name="items">Blocs et adresses appartenant à la piste.</param>
    /// <returns>Secteurs à encoder dans leur ordre logique.</returns>
    IReadOnlyList<TrackSector> CreateTrackSectors(SectorImage image, IReadOnlyList<(SectorBlock Block, SectorAddress Address)> items);
    /// <summary>Construit les attributs techniques de la piste.</summary>
    /// <param name="image">Image sectorielle reconnue.</param>
    /// <param name="sectorCount">Nombre de secteurs produits.</param>
    /// <returns>Attributs d'encodage, ou <see langword="null"/>.</returns>
    IReadOnlyDictionary<string, int>? TrackAttributes(SectorImage image, int sectorCount);
    /// <summary>Retourne la durée d'une cellule binaire.</summary>
    /// <param name="image">Image sectorielle reconnue.</param>
    /// <param name="cylinder">Cylindre de la piste.</param>
    /// <returns>Durée en ticks.</returns>
    uint BitCellTicks(SectorImage image, int cylinder);
    /// <summary>Retourne la durée nominale d'une révolution.</summary>
    /// <param name="image">Image sectorielle reconnue.</param>
    /// <param name="cylinder">Cylindre de la piste.</param>
    /// <returns>Durée en ticks.</returns>
    uint IndexTimeTicks(SectorImage image, int cylinder);
}
