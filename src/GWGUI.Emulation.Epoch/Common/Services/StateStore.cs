using System.IO;
namespace GWGUI.Emulation.Epoch.Common.Services;

internal static class StateStore
{
    internal static void Write(string path, SavedStateHeader header, ReadOnlySpan<byte> state)
    {
        SavedStateFunctions.ValidatePayloadSize(state.Length);
        EmulationSdk.StateFiles.WriteState(path, header, state);
    }

    internal static StateFile Read(string path)
    {
        var result = EmulationSdk.StateFiles.ReadState<MachineModel, Emulator>(path,
            static () => SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateInvalid));
        SavedStateFunctions.ValidatePayloadSize(result.State.Length);
        return new StateFile(result.Header, result.State);
    }

    /// <summary>Calcule l’empreinte du contenu emprunté pour identifier les fichiers d’une sauvegarde d’état.</summary>
    /// <param name="path">Chemin du contenu à lire, dont la propriété reste à l’appelant.</param>
    /// <returns>Empreinte obtenue avec les conventions de lecture du module.</returns>
    /// <remarks>Le lecteur commun libère les flux ouverts, y compris en cas d’erreur. La configuration
    /// et les fichiers utilisateur restent inchangés ; les dépendances de média sont résolues par l’adaptation appelante.</remarks>
    internal static string HashPath(string path) =>
        EmulationSdk.StateFiles.HashPath(path, StateConstants.AllFilesSearchPattern, StateConstants.CanonicalDirectorySeparator, StateConstants.HashBufferSize, () => new FileNotFoundException(ErrorMessages.ContentFileMissing, path));
}
