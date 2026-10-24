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
}
