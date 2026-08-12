namespace GWGUI.MediaEngine.Containers.Commodore;

/// <summary>Identifie les diagnostics sectoriels stockés dans les cartes d'erreurs D64 et D71.</summary>
public enum CommodoreDiskErrorCode : byte
{
    /// <summary>Aucune erreur.</summary>
    None = 1,
    /// <summary>Bloc d'en-tête introuvable.</summary>
    HeaderNotFound = 2,
    /// <summary>Marque de synchronisation introuvable.</summary>
    SyncNotFound = 3,
    /// <summary>Bloc de données introuvable.</summary>
    DataNotFound = 4,
    /// <summary>Checksum des données invalide.</summary>
    DataChecksumError = 5,
    /// <summary>Échec de vérification après écriture.</summary>
    WriteVerifyError = 6,
    /// <summary>Disque protégé contre l'écriture.</summary>
    WriteProtected = 7,
    /// <summary>Checksum d'en-tête invalide.</summary>
    HeaderChecksumError = 8,
    /// <summary>Erreur d'écriture.</summary>
    WriteError = 9,
    /// <summary>Identifiant de disque différent.</summary>
    DiskIdMismatch = 10,
    /// <summary>Erreur générale du lecteur.</summary>
    DriveError = 11,
    /// <summary>Lecteur non prêt.</summary>
    DriveNotReady = 15
}
