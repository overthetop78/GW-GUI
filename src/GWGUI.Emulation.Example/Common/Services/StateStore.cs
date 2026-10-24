using System.IO;
using CommonCoreHostFunctions = GWGUI.Emulation.Example.Common.Functions.CoreHostFunctions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Propriétaire Common de la lecture et de la publication des états de la machine.</summary>
/// <remarks>Machine prépare les identités du modèle, du cœur et des contenus. StateStore échange ensuite
/// ces données par le contrat public du SDK. Le service commun garde le format unique, contrôle son
/// intégrité et libère les fichiers et flux qu’il ouvre ; aucun cœur ni média ne lui est transféré.</remarks>
internal static class StateStore
{
    /// <summary>Interface empruntée au SDK, sans ressource de session possédée par ce magasin.</summary>
    private static readonly IEmulationStateFiles Files = CommonCoreHostFunctions.StateFiles;

    /// <summary>Publie les octets d’état du cœur avec leur en-tête complet.</summary>
    /// <param name="path">Fichier d’état à créer ou remplacer par le service commun.</param>
    /// <param name="header">Version de format, modèle, cœur et empreintes préparés par la machine.</param>
    /// <param name="state">Octets empruntés pendant l’écriture, sans transfert du cœur ni de ses ressources.</param>
    /// <remarks>Le service vérifie l’en-tête et les octets avant publication. Le magasin ne transforme
    /// ni les identifiants typés ni les données et ne modifie pas la configuration de la session.</remarks>
    internal static void Write(string path, EmulationSavedStateHeader<MachineModel, Emulator> header, ReadOnlySpan<byte> state) => Files.WriteState(path, header, state);

    /// <summary>Relit un état sous le même contrat de modèle et de cœur que lors de sa publication.</summary>
    /// <param name="path">Fichier d’état à consulter, ouvert puis fermé par le service commun.</param>
    /// <returns>En-tête contrôlé et nouveau tableau d’octets destiné au cœur.</returns>
    /// <remarks>Le contrôle du fichier vérifie sa structure et son intégrité. Machine vérifie ensuite
    /// la compatibilité avec la session courante avant de remettre ces octets au cœur. Une lecture
    /// invalide conserve la même exception de domaine que le parcours de la machine.</remarks>
    internal static (EmulationSavedStateHeader<MachineModel, Emulator> Header, byte[] State) Read(string path) => Files.ReadState<MachineModel, Emulator>(path, static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateInvalid));

    /// <summary>Calcule l’empreinte du contenu emprunté pour identifier les fichiers d’une sauvegarde d’état.</summary>
    /// <param name="path">Chemin du contenu à lire, dont la propriété reste à l’appelant.</param>
    /// <returns>Empreinte obtenue avec les conventions de lecture du module.</returns>
    /// <remarks>Le lecteur commun libère les flux ouverts, y compris en cas d’erreur. La configuration
    /// et les fichiers utilisateur restent inchangés ; les dépendances de média sont résolues par l’adaptation appelante.</remarks>
    internal static string HashPath(string path) => Files.HashPath(path, "*", ConfigurationStoreConstants.StoredDirectorySeparator, BufferConstants.FileTransferBufferSize,
        () => new FileNotFoundException(ErrorMessages.StateInvalid, path));
}
