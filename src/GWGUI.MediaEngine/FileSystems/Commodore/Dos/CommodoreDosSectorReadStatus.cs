namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Décrit précisément le résultat d'un accès à un secteur Commodore DOS.</summary>
internal enum CommodoreDosSectorReadStatus
{
    /// <summary>Le secteur complet est disponible.</summary>
    Success,
    /// <summary>La coordonnée piste/secteur est invalide.</summary>
    InvalidCoordinate,
    /// <summary>Le bloc logique correspondant est absent.</summary>
    Missing,
    /// <summary>Le bloc existe mais sa taille n'est pas celle d'un secteur Commodore DOS.</summary>
    Truncated
}
