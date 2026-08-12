namespace GWGUI.MediaEngine.Geometries.Commodore;

/// <summary>Construit les erreurs de validation des géométries Commodore.</summary>
internal static class CommodoreGeometryExceptions
{
    /// <summary>Crée l'erreur signalant une piste hors limites.</summary>
    public static ArgumentOutOfRangeException InvalidTrack(int track, int minimum, int maximum) => new(nameof(track), track, $"La piste doit être comprise entre {minimum} et {maximum}.");
    /// <summary>Crée l'erreur signalant un nombre de pistes non pris en charge.</summary>
    public static ArgumentOutOfRangeException InvalidTrackCount(int trackCount, IEnumerable<int> accepted) => new(nameof(trackCount), trackCount, $"Le nombre de pistes doit appartenir à : {string.Join(", ", accepted)}.");
    /// <summary>Crée l'erreur signalant une face hors limites.</summary>
    public static ArgumentOutOfRangeException InvalidSide(int side, int sideCount) => new(nameof(side), side, $"La face doit être comprise entre 0 et {sideCount - 1}.");
    /// <summary>Crée l'erreur signalant un secteur hors limites.</summary>
    public static ArgumentOutOfRangeException InvalidSector(int sector, int minimum, int maximum) => new(nameof(sector), sector, $"Le secteur doit être compris entre {minimum} et {maximum}.");
    /// <summary>Crée l'erreur signalant un bloc logique hors limites.</summary>
    public static ArgumentOutOfRangeException InvalidLogicalBlock(int logicalBlock, int blockCount) => new(nameof(logicalBlock), logicalBlock, $"Le bloc logique doit être compris entre 0 et {blockCount - 1}.");
}
