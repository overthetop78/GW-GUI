using CommonCoreHostFunctions = GWGUI.Emulation.Example.Common.Functions.CoreHostFunctions;
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
    /// <summary>Interface sans état empruntée au SDK ; ses opérations libèrent les fichiers et flux qu’elles ouvrent.</summary>
    private static readonly IEmulationStateFiles Files = CommonCoreHostFunctions.StateFiles;

    /// <summary>Écrit l’état avec le même format complet que les autres modules.</summary>
    /// <param name="path">Chemin du fichier d’état à créer ou remplacer atomiquement.</param>
    /// <param name="header">Identités du modèle et du cœur, empreintes de configuration, de contenu et des octets d’état.</param>
    /// <param name="state">Octets d’état empruntés au cœur pendant l’écriture.</param>
    /// <remarks>Le SDK vérifie l’en-tête et l’intégrité avant publication du fichier. Ce magasin
    /// ne prend possession ni du cœur ni des médias montés et ne modifie pas leur configuration.</remarks>
    internal static void Write(string path, EmulationSavedStateHeader<string, string> header, ReadOnlySpan<byte> state) => Files.WriteState(path, header, state);

    /// <summary>Lit et valide le format unique ; aucun second schéma n’est sélectionné.</summary>
    /// <param name="path">Chemin du fichier d’état à lire.</param>
    /// <returns>En-tête contrôlé et nouveau tableau contenant les octets à transmettre au cœur.</returns>
    /// <remarks>La validation du fichier vérifie sa structure et son intégrité. La compatibilité de son
    /// identité avec la session courante doit être contrôlée avant que le cœur reçoive les octets.</remarks>
    internal static (EmulationSavedStateHeader<string, string> Header, byte[] State) Read(string path) => Files.ReadState<string, string>(path, () => new InvalidDataException(path));

    /// <summary>Calcule les empreintes de fichiers ou dossiers selon la représentation stable du module.</summary>
    /// <param name="path">Chemin du fichier ou du dossier dont le contenu doit être identifié.</param>
    /// <returns>Empreinte SHA-256 du fichier, ou de la représentation ordonnée du dossier.</returns>
    /// <remarks>Pour un dossier, les chemins relatifs normalisés et leurs longueurs participent à
    /// l’empreinte avec le contenu des fichiers. Le service libère chaque flux après son calcul.
    /// Le même traitement couvre donc aussi le cas d’un fichier seul.</remarks>
    internal static string HashPath(string path) => Files.HashPath(path, "*", ConfigurationStoreConstants.StoredDirectorySeparator, BufferConstants.FileTransferBufferSize, () => new FileNotFoundException(path));

    /// <summary>Calcule l’empreinte des octets empruntés.</summary>
    /// <param name="bytes">Octets lus uniquement pendant le calcul, sans transfert de propriété.</param>
    /// <returns>Représentation textuelle du SHA-256 calculé par le service commun.</returns>
    internal static string HashBytes(ReadOnlySpan<byte> bytes) => Files.HashBytes(bytes);

    /// <summary>Calcule l’empreinte canonique des projections de configuration et de contenu du Common.</summary>
    /// <typeparam name="T">Type de la projection de données choisie par le module.</typeparam>
    /// <param name="value">Données à sérialiser pour calculer leur identité.</param>
    /// <param name="options">Conventions JSON partagées avec le calcul effectué lors du rechargement.</param>
    /// <returns>Empreinte SHA-256 de la représentation JSON canonique.</returns>
    /// <remarks>Le service ordonne les clés des objets et conserve l’ordre des tableaux. La projection
    /// détermine les réglages et contenus qui participent à l’identité ; ce magasin n’en ajoute pas.</remarks>
    internal static string HashDocument<T>(T value, JsonSerializerOptions? options = null) => Files.HashDocument(value, options);
}
