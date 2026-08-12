namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit les erreurs propres aux reconstructions SCP ISO FM et MFM.</summary>
internal static class IsoScpReconstructionExceptions
{
    /// <summary>Crée l'erreur signalant l'absence de candidat adressé à mesurer.</summary>
    /// <returns>L'exception signalant l'absence de candidat adressé.</returns>
    public static InvalidDataException NoAddressedCandidates() => new("No consistently addressed ISO FM/MFM sectors could be measured.");
    /// <summary>Crée l'erreur signalant qu'aucun candidat cohérent n'a été décodé.</summary>
    /// <param name="formatId">Identifiant demandé, ou <see langword="null"/> pour la détection automatique.</param>
    /// <param name="addressedCandidateCount">Nombre de candidats dont l'adresse décodée correspond à la piste physique.</param>
    /// <param name="physicalCandidateCount">Nombre de candidats conservés à leur adresse physique source.</param>
    /// <returns>L'exception décrivant les deux collections vides.</returns>
    public static InvalidDataException NoCandidates(string? formatId, int addressedCandidateCount, int physicalCandidateCount) => new($"ISO FM/MFM reconstruction for '{formatId ?? "automatic"}' produced {addressedCandidateCount} addressed candidate(s) and {physicalCandidateCount} physical candidate(s).");
    /// <summary>Crée l'erreur signalant qu'un BPB non DOS ne doit pas être attribué automatiquement à IBM PC.</summary>
    public static InvalidDataException NotIbmDos() => new("The ISO MFM boot sector contains a FAT BPB but no DOS/IBM identity.");
}
