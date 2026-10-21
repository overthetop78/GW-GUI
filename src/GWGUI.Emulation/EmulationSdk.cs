namespace GWGUI.Emulation;

/// <summary>Accès aux services du SDK par leurs contrats publics, sans connaissance de l’identité des modules.</summary>
public static class EmulationSdk
{
    /// <summary>Découverte des adaptateurs et fabriques d’un module par leurs contrats, sans liste de modules connus.</summary>
    public static IEmulationAdapters Adapters { get; } = new EmulationAdapterService();

    /// <summary>Service d’inspection sans état partagé mutable et sans bibliothèque native chargée.</summary>
    /// <remarks>L’implémentation reste interne au SDK. Le module utilise le contrat et n’a aucun service à libérer.</remarks>
    public static IEmulationCoreDiagnostics CoreDiagnostics { get; } = new EmulationCoreDiagnosticFunctions();

    /// <summary>Sélection et installation des versions par le contrat commun, sans accès à la classe interne.</summary>
    /// <remarks>Ce service sans état ne possède pas le client HTTP ni les callbacks fournis par le module.</remarks>
    public static IEmulationCoreReleases CoreReleases { get; } = new EmulationCoreReleaseFunctions();

    /// <summary>Traitements audio sans état ; sorties et pointeurs restent possédés par les appelants.</summary>
    public static IEmulationAudioProcessing AudioProcessing { get; } = new EmulationAudioFunctions();

    /// <summary>Nettoyage des seuls dossiers de session possédés, après destruction des cœurs.</summary>
    public static IEmulationSessionDirectories SessionDirectories { get; } = new EmulationSessionDirectoryFunctions();

    /// <summary>Format de sauvegarde commun, sans accès aux lecteurs binaires internes.</summary>
    public static IEmulationStateFiles StateFiles { get; } = new EmulationStateFileService();

    /// <summary>Persistance par contrat, avec les propriétaires d’écriture et de verrouillage existants.</summary>
    public static IEmulationConfigurationFiles ConfigurationFiles { get; } = new EmulationConfigurationFileService();

    /// <summary>Crée un stockage indépendant des entrées sous son contrat public.</summary>
    public static IEmulationInputFrames CreateInputFrames() => new EmulationInputFrameStore();

    /// <summary>Fournit le contrôleur existant sous son contrat ; la sortie reçue reste empruntée.</summary>
    public static IEmulationAudioOutputController CreateAudioOutputController(IAudioOutput? output = null,
        Func<IAudioOutput?>? factory = null) => new EmulationAudioOutputController(output, factory);
}
