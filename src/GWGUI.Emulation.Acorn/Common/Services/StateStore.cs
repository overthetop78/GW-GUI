using System.IO;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Acorn.Common.Services;

/// <summary>Propriétaire Common de la lecture et de la publication des fichiers d’état.</summary>
/// <remarks>La machine fournit l’en-tête et contrôle sa compatibilité ; le service public du SDK
/// conserve le format, les vérifications d’intégrité et la propriété de ses flux de fichiers.</remarks>
internal static class StateStore
{
    /// <summary>Publie les octets empruntés au cœur avec leur en-tête complet.</summary>
    /// <param name="path">Fichier d’état à écrire par le service commun.</param>
    /// <param name="header">Identités et empreintes déjà préparées par la machine.</param>
    /// <param name="state">Octets empruntés pendant la publication, sans transfert du cœur.</param>
    internal static void Write(string path, EmulationSavedStateHeader<MachineModel, Emulator> header, ReadOnlySpan<byte> state) => EmulationSdk.StateFiles.WriteState(path, header, state);

    /// <summary>Relit le fichier sous le même contrat typé que la publication.</summary>
    /// <param name="path">Fichier d’état à consulter.</param>
    /// <returns>En-tête contrôlé et octets ; la machine vérifie leur compatibilité avant chargement.</returns>
    internal static (EmulationSavedStateHeader<MachineModel, Emulator> Header, byte[] State) Read(string path) => EmulationSdk.StateFiles.ReadState<MachineModel, Emulator>(path, static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateInvalid));

    /// <summary>Calcule l’empreinte du contenu emprunté pour identifier les fichiers d’une sauvegarde d’état.</summary>
    /// <param name="path">Chemin du contenu à lire, dont la propriété reste à l’appelant.</param>
    /// <returns>Empreinte obtenue avec les conventions de lecture du module.</returns>
    /// <remarks>Le lecteur commun libère les flux ouverts, y compris en cas d’erreur. La configuration
    /// et les fichiers utilisateur restent inchangés ; les dépendances de média sont résolues par l’adaptation appelante.</remarks>
    internal static string HashPath(string path) => EmulationSdk.StateFiles.HashPath(path, "*", '/', 64 * 1024,
        () => new FileNotFoundException(ErrorMessages.StateInvalid, path));

    /// <summary>Calcule le SHA-256 des octets empruntés au cœur pour l’en-tête d’un état.</summary>
    /// <param name="bytes">Octets exacts de l’état, valides pendant cet appel ; le service ne les conserve pas.</param>
    /// <returns>Empreinte hexadécimale produite par le contrat commun de sauvegarde.</returns>
    /// <remarks>Ce calcul reste en mémoire : il ne crée aucun fichier et ne transfère pas la propriété du tampon.
    /// La même opération identifie aussi les descriptions de contenus déjà encodées en octets par l’appelant.</remarks>
    internal static string HashBytes(ReadOnlySpan<byte> bytes) => EmulationSdk.StateFiles.HashBytes(bytes);
}
