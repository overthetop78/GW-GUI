namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Exigences, emplacements et préparation des firmwares proposés par le backend.</summary>
/// <remarks>Common consulte ces règles pour construire les réglages puis préparer la session. Les fichiers
/// sources restent empruntés ; les dossiers préparés appartiennent au propriétaire de la session.</remarks>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Décrit les emplacements de firmware proposés par cet adaptateur.</summary>
    /// <param name="configuration">Configuration du modèle pour lequel les emplacements sont demandés.</param>
    /// <returns>Descriptions des champs de sélection, fichiers attendus et contraintes associées.</returns>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);

    /// <summary>Indique si le cœur exige au moins un fichier de firmware externe avant l’enregistrement de sa configuration.</summary>
    /// <remarks>Cette obligation peut être satisfaite par plusieurs alternatives facultatives prises séparément.
    /// IsRequired dans FirmwareSlot exprime, lui, l’obligation de fournir précisément cet emplacement.
    /// Un cœur disposant uniquement de firmwares embarqués annonce false.</remarks>
    bool RequiresExternalFirmware { get; }

    /// <summary>Recherche les candidats de firmware suivant les règles du modèle et du cœur.</summary>
    /// <param name="configuration">Configuration indiquant les emplacements et compatibilités à rechercher.</param>
    /// <param name="directory">Répertoire de recherche fourni par l’hôte, dont les fichiers restent empruntés.</param>
    /// <param name="cancellationToken">Annulation de la recherche et des lectures nécessaires à l’identification.</param>
    /// <returns>Candidats avec leurs identités et informations de compatibilité.</returns>
    IReadOnlyList<EmulationFirmwareCandidate> ScanFirmware(MachineConfiguration configuration, string directory, CancellationToken cancellationToken);

    /// <summary>Prépare les fichiers de firmware dans le dossier système fourni au cœur.</summary>
    /// <param name="configuration">Configuration empruntée contenant les firmwares sélectionnés.</param>
    /// <param name="systemDirectory">Dossier de préparation dont Common possède le cycle de vie.</param>
    /// <remarks>Les fichiers sources de l’utilisateur restent empruntés et ne sont pas supprimés par cette opération.</remarks>
    void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);

    /// <summary>Décrit les rôles de firmware proposés pour un modèle.</summary>
    /// <param name="model">Modèle matériel dont les catégories doivent être présentées.</param>
    /// <returns>Rôles pris en charge, dans leur ordre de présentation par le catalogue du backend.</returns>
    IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);

    /// <summary>Indique si un rôle de firmware doit être fourni pour le modèle demandé.</summary>
    /// <param name="model">Modèle dont les obligations de firmware sont consultées.</param>
    /// <param name="category">Rôle précis à contrôler, distinct de l’exigence globale RequiresExternalFirmware.</param>
    /// <returns>True si ce rôle est obligatoire selon les règles du backend.</returns>
    bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
}
