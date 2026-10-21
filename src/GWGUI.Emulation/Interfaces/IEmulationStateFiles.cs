using System.Text.Json;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Format de sauvegarde unique et empreintes partagées par les modules.</summary>
public interface IEmulationStateFiles
{
    /// <summary>Écrit atomiquement un état dont l’en-tête complet et l’empreinte sont valides.</summary>
    void WriteState<TModel, TCore>(string path, EmulationSavedStateHeader<TModel, TCore> header, ReadOnlySpan<byte> state);
    /// <summary>Lit le seul format commun et vérifie l’intégrité ; le module fournit son erreur de format.</summary>
    (EmulationSavedStateHeader<TModel, TCore> Header, byte[] State) ReadState<TModel, TCore>(string path, Func<Exception> invalid);
    /// <summary>Vérifie qu’un en-tête correspond au modèle, au cœur et aux contenus de la session courante.</summary>
    /// <param name="header">En-tête lu et contrôlé par ReadState.</param>
    /// <param name="model">Identité du modèle attendu.</param>
    /// <param name="emulator">Identité du backend attendu.</param>
    /// <param name="core">Cœur emprunté fournissant son nom, sa version et son empreinte.</param>
    /// <param name="configurationHash">Calcul de l’empreinte de configuration, appelé lorsque les identités préalables correspondent.</param>
    /// <param name="contentHash">Calcul de l’empreinte des contenus courants.</param>
    /// <returns>True si le format, les identités et les empreintes correspondent selon les règles du format unique.</returns>
    bool IsCompatible<TModel, TCore, TConfiguration, TOption>(EmulationSavedStateHeader<TModel, TCore> header,
        TModel model, TCore emulator, IEmulationCoreRuntime<TConfiguration, TOption> core, Func<string> configurationHash, Func<string> contentHash);
    /// <summary>Calcule une empreinte JSON avec clés triées et ordre des tableaux conservé.</summary>
    string HashDocument<T>(T value, JsonSerializerOptions? options = null);
    /// <summary>Calcule le SHA-256 des octets empruntés.</summary>
    string HashBytes(ReadOnlySpan<byte> bytes);
    /// <summary>Calcule le SHA-256 d’un fichier ou dossier selon la représentation du module.</summary>
    string HashPath(string path, string pattern, char separator, int bufferSize, Func<Exception> missing);
}
