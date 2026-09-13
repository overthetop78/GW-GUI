namespace GWGUI.MediaEngine.Reconstruction.Iso;

/// <summary>Définit les paramètres de sélection des reconstructions SCP ISO FM et MFM.</summary>
internal static class IsoScpReconstructionDefinitions
{
    /// <summary>Poids prioritaire d'un secteur dont le contrôle d'intégrité est valide.</summary>
    public const int ValidSectorScoreWeight = 1000;
    /// <summary>Poids d'un secteur contenant des données dans le score d'un décodage.</summary>
    public const int DataSectorScoreWeight = 10;
    /// <summary>Pénalité appliquée à un secteur dont le contrôle d'intégrité est invalide.</summary>
    public const int InvalidSectorScorePenalty = 1000;
}
