using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Description des exigences de firmware et contrôle de leur compatibilité par l’adaptateur.</summary>
/// <remarks>Common consulte ce contrat avec la configuration courante pour présenter les champs de sélection et valider les candidats.
/// Le catalogue et les contrôles du cœur restent dans son adaptation ; ce contrat n’installe aucun fichier.</remarks>
internal interface IEmulatorFirmwareCapabilities
{
    /// <summary>Décrit les rôles et fichiers attendus pour la configuration courante.</summary>
    /// <param name="configuration">Modèle, cœur et options déterminant les exigences.</param>
    /// <returns>Rôles, champs, libellés, noms de fichiers et caractère obligatoire de chaque exigence.</returns>
    IReadOnlyList<FirmwareRequirement> DescribeFirmware(MachineConfiguration configuration);
    /// <summary>Contrôle un candidat pour un rôle précis de cette configuration.</summary>
    /// <param name="configuration">Configuration empruntée pour laquelle le candidat est évalué.</param>
    /// <param name="role">Catégorie de firmware attendue par le cœur.</param>
    /// <param name="path">Chemin du candidat laissé sous la responsabilité de son appelant.</param>
    /// <returns>True lorsque les règles du cœur acceptent ce candidat pour le rôle demandé.</returns>
    /// <remarks>Le contrôle conserve les règles existantes de nom, de taille ou de contenu propres à l’adaptateur.</remarks>
    bool IsFirmwareCompatible(MachineConfiguration configuration, FirmwareCategory role, string path);
}
