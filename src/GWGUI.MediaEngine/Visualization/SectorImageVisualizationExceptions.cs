namespace GWGUI.MediaEngine.Visualization;

/// <summary>Construit les erreurs de visualisation d'une image sectorielle.</summary>
internal static class SectorImageVisualizationExceptions
{
    /// <summary>Crée l'erreur signalant l'absence de politique ou d'encodeur.</summary>
    public static NotSupportedException MissingPolicy(string formatId) => new($"Aucune politique de visualisation n'est disponible pour le format '{formatId}'.");
    /// <summary>Crée l'erreur signalant qu'aucune piste n'a pu être produite.</summary>
    public static InvalidDataException NoTrack(string formatId) => new($"L'image sectorielle '{formatId}' ne produit aucune piste visualisable.");
}
