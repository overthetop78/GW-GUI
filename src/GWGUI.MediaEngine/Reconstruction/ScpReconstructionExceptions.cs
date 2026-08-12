namespace GWGUI.MediaEngine.Reconstruction;

/// <summary>Construit les erreurs communes à la reconstruction sectorielle des captures SCP.</summary>
internal static class ScpReconstructionExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucun secteur d'une famille n'a été décodé.</summary>
    public static InvalidDataException NoDecodedSectors(string family) => new($"No {family} sectors could be decoded from the SCP image.");
    /// <summary>Crée l'erreur signalant qu'aucun secteur décodé n'a pu être reconstruit.</summary>
    public static InvalidDataException NoUsableSectors(string family) => new($"No usable {family} sectors could be reconstructed.");
    /// <summary>Crée l'erreur signalant que tous les secteurs décodés ont été rejetés lors de la reconstruction.</summary>
    /// <param name="family">Nom de la famille ou de la structure sectorielle reconstruite.</param>
    /// <param name="decodedSectorCount">Nombre de candidats décodés avant validation de la géométrie.</param>
    /// <param name="usableSectorCount">Nombre de secteurs conservés après validation.</param>
    /// <returns>L'exception décrivant le rejet de tous les candidats.</returns>
    public static InvalidDataException NoUsableSectors(string family, int decodedSectorCount, int usableSectorCount) => new($"No usable {family} sectors could be reconstructed: {decodedSectorCount} candidate(s) decoded, {usableSectorCount} usable.");
    /// <summary>Crée l'erreur signalant qu'un format demandé n'appartient pas à la famille attendue.</summary>
    public static InvalidDataException InvalidRequestedFormat(string family, string formatId) => new($"Requested format '{formatId}' is not supported by the {family} SCP reconstructor.");
    /// <summary>Crée l'erreur signalant qu'une piste ne contient aucun secteur attendu.</summary>
    public static InvalidDataException MissingTrackSectors(string family, int track, int decodedSectorCount) => new($"{family} track {track} contains {decodedSectorCount} decodable sectors; no usable sector was reconstructed.");
}
