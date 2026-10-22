namespace GWGUI.Emulation.Commodore.Common.Interfaces;

/// <summary>Emplacements et métadonnées de firmware proposés par le backend.</summary>
/// <remarks>Common consulte les règles du modèle et du cœur pour présenter les firmwares ; les fichiers sources restent empruntés.</remarks>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Décrit les emplacements de firmware proposés par cet adaptateur.</summary>
    /// <param name="configuration">Configuration du modèle pour lequel les emplacements sont demandés.</param>
    /// <returns>Descriptions des champs de sélection, fichiers attendus et contraintes associées.</returns>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);

    /// <summary>Recherche les candidats de firmware suivant les règles du modèle et du cœur.</summary>
    /// <param name="configuration">Configuration indiquant les emplacements et compatibilités à rechercher.</param>
    /// <param name="directory">Répertoire de recherche fourni par l’hôte, dont les fichiers restent empruntés.</param>
    /// <param name="cancellationToken">Annulation de la recherche et des lectures nécessaires à l’identification.</param>
    /// <returns>Candidats avec leurs identités et informations de compatibilité.</returns>
    IReadOnlyList<EmulationFirmwareCandidate> ScanFirmware(MachineConfiguration configuration, string directory, CancellationToken cancellationToken);
}
