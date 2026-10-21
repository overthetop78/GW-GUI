using GWGUI.Emulation;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Contrat de fonctionnement d’un cœur, réutilisable par tout module indépendant.</summary>
/// <remarks>La machine possède le cœur : elle l’initialise, l’exécute et le libère sur son thread après arrêt de la boucle. Les sorties ne doivent plus être lues après sa destruction.</remarks>
public interface IEmulationCoreRuntime<in TConfiguration, out TOption> : IDisposable
{
    /// <summary>Dernière image produite ; null tant qu’aucune image n’est disponible.</summary>
    VideoFrame? LatestVideoFrame { get; }
    /// <summary>Dernier bloc audio produit, indépendamment de sa consommation dans la file.</summary>
    AudioChunk? LatestAudioChunk { get; }
    /// <summary>Retire le prochain bloc audio de la file ; renvoie false lorsque la file est vide.</summary>
    bool TryDequeueAudio(out AudioChunk? chunk);
    /// <summary>Options exposées par ce cœur, avec leurs valeurs courantes.</summary>
    IReadOnlyList<TOption> Options { get; }
    /// <summary>Messages de diagnostic recueillis par le cœur.</summary>
    IReadOnlyList<string> Diagnostics { get; }
    /// <summary>État d’activité des lecteurs, indexés selon le contrat du module.</summary>
    IReadOnlyDictionary<int, bool> LedStates { get; }
    /// <summary>Nom du cœur utilisé aussi pour vérifier l’identité des sauvegardes.</summary>
    string CoreName { get; }
    /// <summary>Version du cœur réellement chargé.</summary>
    string CoreVersion { get; }
    /// <summary>Extensions de contenu acceptées par ce cœur.</summary>
    IReadOnlySet<string> SupportedContentExtensions { get; }
    /// <summary>Empreinte SHA-256 de l’implémentation utilisée pour produire les états.</summary>
    string CoreSha256 { get; }
    /// <summary>Cadence courante utilisée par la boucle de frames commune.</summary>
    double FramesPerSecond { get; }
    /// <summary>Fréquence des échantillons audio produits, en hertz.</summary>
    int SampleRate { get; }
    /// <summary>Initialise le cœur avec la configuration et les dossiers préparés par la machine.</summary>
    /// <remarks>Le dossier de session appartient à la machine ; le dossier de sauvegarde peut persister après son arrêt.</remarks>
    void Initialize(TConfiguration configuration, string sessionDirectory, string? saveDirectory = null);
    /// <summary>Exécute une frame sur le thread propriétaire et met à jour les sorties disponibles.</summary>
    void RunFrame();
    /// <summary>Applique les entrées de la frame ; leur traduction vers l’API native appartient au cœur.</summary>
    void SetInput(EmulationInputSnapshot snapshot);
}
