namespace GWGUI.MediaEngine.Decoding.Apple;

/// <summary>Définit les règles de sélection des secteurs Apple II et RWTS18.</summary>
internal static class AppleTrackSelectionRules
{
    /// <summary>Premier numéro de secteur accepté pour une piste Apple II standard.</summary>
    public const int StandardMinimumSectorNumber = 0;
    /// <summary>Dernier numéro de secteur accepté pour une piste Apple II standard.</summary>
    public const int StandardMaximumSectorNumber = 15;
    /// <summary>Taille attendue d'un secteur Apple II standard.</summary>
    public const int StandardSectorSize = 256;
    /// <summary>Premier numéro de secteur accepté pour une piste RWTS18.</summary>
    public const int Rwts18MinimumSectorNumber = 0;
    /// <summary>Dernier numéro de secteur accepté pour une piste RWTS18.</summary>
    public const int Rwts18MaximumSectorNumber = 5;
    /// <summary>Taille attendue d'un secteur RWTS18.</summary>
    public const int Rwts18SectorSize = 768;
    /// <summary>Poids accordé à chaque numéro de secteur distinct dans le score.</summary>
    public const int DistinctSectorScoreWeight = 100;
    /// <summary>Poids accordé à chaque secteur dont l'intégrité est valide dans le score.</summary>
    public const int IntegrityScoreWeight = 10;
    /// <summary>Score précédant tout candidat décodé.</summary>
    public const int InitialScore = -1;
    /// <summary>Nombre minimal de pistes RWTS18 valides nécessaire pour retenir ce format.</summary>
    public const int MinimumCredibleRwts18TrackCount = 2;
}
