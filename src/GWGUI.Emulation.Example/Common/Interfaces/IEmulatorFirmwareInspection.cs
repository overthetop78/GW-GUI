using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Identification d’un firmware depuis les métadonnées déjà observées par Common.</summary>
/// <remarks>Ce contrat reprend IdentifyFirmware en utilisant le résultat d’analyse unique ScannedFirmware.
/// L’adaptateur consulte ses règles et données de reconnaissance sans recevoir de flux ni posséder le fichier.
/// Il peut fournir l’identité, la version, les modèles compatibles et les autres informations du catalogue.</remarks>
internal interface IEmulatorFirmwareInspection
{
    /// <summary>Recherche une identité connue pour les données observées du candidat.</summary>
    /// <param name="firmware">Chemin, taille et empreintes observés ; Detection peut encore indiquer Unknown et la compatibilité reste à établir.</param>
    /// <returns>Résultat complété avec son identité reconnue, ou null lorsqu’aucune règle ne reconnaît le candidat.</returns>
    /// <remarks>Le résultat conserve les données observées. Les informations reconnues appartiennent à
    /// Definition ; aucun second objet Firmware parallèle ne transporte une autre copie de ces mêmes données.</remarks>
    ScannedFirmware? IdentifyFirmware(ScannedFirmware firmware);

    /// <summary>Inspecte un fichier candidat suivant les règles du modèle et de ses options.</summary>
    /// <param name="configuration">Configuration empruntée déterminant les contraintes de firmware.</param>
    /// <param name="path">Fichier utilisateur à lire sans le modifier.</param>
    /// <param name="cancellationToken">Annulation de la lecture et du calcul des empreintes.</param>
    /// <returns>Candidat identifié et informations de compatibilité ; les flux ouverts sont libérés avant le retour.</returns>
    /// <remarks>Ce nom unique réunit les opérations InspectFirmwareAsync, ScanFirmwareAsync et ScanFirmwareFileAsync des variantes existantes.</remarks>
    ValueTask<EmulationFirmwareCandidate> ScanFirmwareFileAsync(MachineConfiguration configuration, string path, CancellationToken cancellationToken);

    /// <summary>Contrôle la compatibilité d’un fichier avec un rôle de firmware et la configuration.</summary>
    /// <param name="configuration">Configuration du modèle et du cœur retenus.</param>
    /// <param name="role">Rôle du fichier dans le système émulé.</param>
    /// <param name="path">Chemin du candidat, conservé sous la responsabilité de l’appelant.</param>
    /// <returns>True si le candidat répond aux contraintes de ce rôle.</returns>
    bool IsFirmwareCompatible(MachineConfiguration configuration, FirmwareCategory role, string path);

    /// <summary>Décrit les exigences de firmware nécessaires à la configuration du cœur.</summary>
    /// <param name="configuration">Configuration empruntée déterminant les rôles et contraintes attendus.</param>
    /// <returns>Exigences de contenu et de compatibilité, distinctes des champs de sélection de fichiers présentés à l’utilisateur.</returns>
    IReadOnlyList<FirmwareRequirement> DescribeFirmware(MachineConfiguration configuration);
}
