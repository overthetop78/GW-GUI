namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Construit les erreurs propres aux reconstructions SCP ISO FM et MFM.</summary>
internal static class IsoScpReconstructionExceptions
{
    /// <summary>Crée l'erreur signalant qu'aucun candidat cohérent n'a été décodé.</summary>
    public static InvalidDataException NoCandidates(string? formatId, int candidateCount) => new($"ISO FM/MFM reconstruction for '{formatId ?? "automatic"}' produced {candidateCount} coherent candidates.");
}
