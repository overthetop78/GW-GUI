namespace GWGUI.MediaEngine.SectorImages.Builders.Apple;

/// <summary>Construit les erreurs des builders sectoriels Apple.</summary>
internal static class AppleSectorImageBuilderExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucun secteur RWTS18 valide n'a été trouvé dans les pistes reçues.</summary>
    public static InvalidDataException NoRwts18Sector(int trackCount, int decodedSectorCount) => new($"No valid Apple II RWTS18 sector was found among {decodedSectorCount} decoded sectors from {trackCount} tracks.");
}
