using GWGUI.Emulation;
using System.IO;
using System.Text.Json;
using GWGUI.Emulation.Example.Common.Constants;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Propriétaire Common de l’enveloppe de sauvegarde et des empreintes, comme les StateStore existants.</summary>
/// <remarks>Le cœur possède les octets de son état ; Machine fournit l’identité complète.
/// Le SDK conserve les algorithmes de format, de contrôle et d’écriture atomique.</remarks>
internal static class StateStore
{
    // Interface sans état empruntée au SDK ; les fichiers et flux ouverts par ses opérations sont libérés par lui.
    private static readonly IEmulationStateFiles Files = EmulationSdk.StateFiles;
    /// <summary>Écrit l’état avec le même format complet que les autres modules.</summary>
    internal static void Write(string path, EmulationSavedStateHeader<string, string> header, ReadOnlySpan<byte> state) => Files.WriteState(path, header, state);
    /// <summary>Lit et valide le format unique ; aucun second schéma n’est sélectionné.</summary>
    internal static (EmulationSavedStateHeader<string, string> Header, byte[] State) Read(string path) => Files.ReadState<string, string>(path, () => new InvalidDataException(path));
    /// <summary>Calcule les empreintes de fichiers ou dossiers selon la représentation stable du module.</summary>
    internal static string HashPath(string path) => Files.HashPath(path, "*", ConfigurationStoreConstants.StoredDirectorySeparator, BufferConstants.FileTransferBufferSize, () => new FileNotFoundException(path));
    /// <summary>Calcule l’empreinte des octets empruntés.</summary>
    internal static string HashBytes(ReadOnlySpan<byte> bytes) => Files.HashBytes(bytes);
    /// <summary>Calcule l’empreinte canonique des projections de configuration et de contenu du Common.</summary>
    internal static string HashDocument<T>(T value, JsonSerializerOptions? options = null) => Files.HashDocument(value, options);
}
