using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Scoring;

/// <summary>Calcule la proportion de blocs décodés d'une image sectorielle.</summary>
internal static class DiskImageDecodeScore
{
    /// <summary>Dénominateur minimal évitant une division par zéro.</summary>
    public const int MinimumBlockCount = 1;
    /// <summary>Retourne la proportion de blocs disponibles parmi les blocs logiques annoncés.</summary>
    /// <param name="image">Image sectorielle à évaluer.</param>
    /// <returns>Proportion comprise entre zéro et un pour une image cohérente.</returns>
    public static double Calculate(SectorImage image) => image.AvailableBlocks.Count / (double)Math.Max(MinimumBlockCount, image.BlockCount);
}
