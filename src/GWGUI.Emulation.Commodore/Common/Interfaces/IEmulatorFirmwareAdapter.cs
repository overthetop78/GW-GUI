namespace GWGUI.Emulation.Commodore.Common.Interfaces;

/// <summary>Inspection en lecture seule d’un fichier candidat par les règles de l’adaptateur de cœur.</summary>
/// <remarks>Common fournit la configuration courante et transmet le résultat au gestionnaire de firmwares.
/// Cette opération reste distincte de la préparation des fichiers de session et de la description des emplacements.
/// L’adaptateur réutilise son identification existante et laisse le fichier d’origine sous la responsabilité de son appelant.</remarks>
internal interface IEmulatorFirmwareAdapter
{
    /// <summary>Analyse un fichier suivant le modèle, le cœur et les options de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée de la machine qui utilisera le firmware.</param>
    /// <param name="path">Chemin du fichier candidat à lire.</param>
    /// <param name="cancellationToken">Annulation des lectures et calculs d’empreintes.</param>
    /// <returns>Identité observée, compatibilité et champ de destination déterminés par les règles de l’adaptateur.</returns>
    /// <remarks>Les flux ouverts pour l’analyse sont libérés avant le retour ; le fichier n’est ni installé ni modifié.</remarks>
    ValueTask<EmulationFirmwareCandidate> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);
}
