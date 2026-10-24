using System.IO;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text.Json;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Contrat de transport commun entre un module et son hôte de cœur.</summary>
/// <remarks>Les codecs, configurations et identités propres aux modules restent fournis par leurs interfaces.
/// Les flux et cœurs reçus sont empruntés, sauf les opérations explicitement destinées à leur libération.
/// Le propriétaire de session ferme ses ressources dans son bloc finally après la fin effective des échanges.</remarks>
public interface IEmulationHostProtocol
{
    /// <summary>Sérialise le diagnostic complet en JSON puis écrit sa chaîne binaire.</summary>
    /// <typeparam name="TCategory">Type des catégories utilisées par Common.</typeparam>
    /// <typeparam name="TCode">Type des codes associés aux catégories.</typeparam>
    /// <param name="writer">Rédacteur emprunté, après le statut d’échec ; reste ouvert.</param>
    /// <param name="error">Diagnostic projeté par Common, conservant contexte et localisation.</param>
    /// <param name="jsonOptions">Conventions JSON partagées par les deux extrémités.</param>
    /// <param name="diagnostics">Messages disponibles du cœur, ajoutés dans leur ordre sans remplacer les codes ni le contexte.</param>
    void WriteError<TCategory, TCode>(BinaryWriter writer, HostError<TCategory, TCode> error, JsonSerializerOptions jsonOptions, IReadOnlyList<string> diagnostics) where TCategory : struct where TCode : struct;

    /// <summary>Lit le diagnostic complet ; la restitution d’une exception reste à la charge de Common.</summary>
    /// <typeparam name="TCategory">Type des catégories utilisées par Common.</typeparam>
    /// <typeparam name="TCode">Type des codes associés aux catégories.</typeparam>
    /// <param name="reader">Lecteur emprunté, après le statut d’échec ; reste ouvert.</param>
    /// <param name="jsonOptions">Conventions JSON identiques à celles du rédacteur.</param>
    /// <param name="invalidError">Diagnostic Common à lever lorsque le JSON représente une valeur nulle.</param>
    /// <returns>Diagnostic complet désérialisé, sans tentative de recréer son type d’exception.</returns>
    HostError<TCategory, TCode> ReadError<TCategory, TCode>(BinaryReader reader, JsonSerializerOptions jsonOptions, Func<Exception> invalidError) where TCategory : struct where TCode : struct;

    /// <summary>Écrit la version entière du protocole puis le code de commande ou de statut sur un octet.</summary>
    /// <param name="writer">Rédacteur emprunté, laissé ouvert.</param>
    /// <param name="version">Version attendue également par le destinataire.</param>
    /// <param name="code">Code interprété par le contrat Common du module.</param>
    void WriteHeader(BinaryWriter writer, int version, byte code);

    /// <summary>Lit et valide la version avant de consommer le code de commande ou de statut.</summary>
    /// <param name="reader">Lecteur emprunté, laissé ouvert.</param>
    /// <param name="validateVersion">Validation Common ; une exception laisse le code non consommé.</param>
    /// <returns>Code octet reçu après validation de la version.</returns>
    byte ReadHeader(BinaryReader reader, Action<int> validateVersion);

    /// <summary>Écrit un instantané de retours de contrôleur après validation complète des états.</summary>
    /// <param name="writer">Rédacteur binaire emprunté ; reste ouvert après cet appel.</param>
    /// <param name="states">Effets forts et faibles des ports, avec des intensités finies entre zéro et un.</param>
    /// <remarks>Chaque couple port/commande est unique. Une liste vide arrête les effets précédents.
    /// Les limites sont celles des ports du protocole commun. Le service ne conserve pas la collection.</remarks>
    void WriteControllerFeedback(BinaryWriter writer, IReadOnlyList<ControllerFeedbackState> states);

    /// <summary>Lit et valide un instantané binaire de retours sans consommer le champ suivant.</summary>
    /// <param name="reader">Lecteur emprunté positionné avant le nombre des effets ; reste ouvert.</param>
    /// <returns>Copie des états validés ; une liste vide signale la fin des effets précédents.</returns>
    IReadOnlyList<ControllerFeedbackState> ReadControllerFeedback(BinaryReader reader);


    /// <summary>Capacité maximale en octets d’un emplacement du transport vidéo fixe.</summary>
    int VideoSlotCapacity { get; }

    /// <summary>Capacité totale en octets du segment vidéo fixe partagé entre les deux extrémités.</summary>
    long VideoMapCapacity { get; }

    /// <summary>Nombre maximal de voyants admis par le codec de statut ; leurs indices commencent à zéro.</summary>
    int MaximumLedStateCount { get; }

    /// <summary>Écrit le booléen de présence puis la chaîne facultative selon le codec existant.</summary>
    /// <param name="writer">Rédacteur emprunté ; sa fermeture appartient au propriétaire du transport.</param>
    /// <param name="value">Chaîne à transmettre ; null écrit uniquement le booléen de présence.</param>
    void WriteString(BinaryWriter writer, string? value);

    /// <summary>Libère une ressource de transport suivant la politique existante qui ignore les erreurs de fermeture.</summary>
    /// <param name="value">Ressource possédée par l’appelant et remise à cette opération pour sa libération ; null est accepté.</param>
    /// <remarks>Cette opération complète le finally du propriétaire ; les objets simplement empruntés ne lui sont pas transmis.</remarks>
    void DisposeTransportIgnoringErrors(IDisposable? value);

    /// <summary>Écrit une image dans le transport vidéo à deux emplacements fixes et publie ses métadonnées.</summary>
    /// <param name="writer">Rédacteur binaire emprunté pour les métadonnées de réponse.</param>
    /// <param name="frame">Image empruntée, ou null pour signaler l’absence de nouvelle image.</param>
    /// <param name="videoMap">Vue de mémoire partagée empruntée ; le propriétaire de session la ferme dans son finally.</param>
    /// <param name="hostName">Nom employé dans les erreurs de capacité du protocole.</param>
    void WriteSharedFrame(BinaryWriter writer, VideoFrame? frame, MemoryMappedViewAccessor videoMap, string hostName);

    /// <summary>Lit les métadonnées et copie les pixels de l’emplacement partagé indiqué.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné sur la présence d’une image.</param>
    /// <param name="videoMap">Vue empruntée qui reste sous la responsabilité de son propriétaire.</param>
    /// <param name="hostName">Nom employé dans les erreurs de métadonnées et de lecture.</param>
    /// <returns>Image dont les pixels sont copiés, ou null si aucune image n’est annoncée.</returns>
    VideoFrame? ReadSharedFrame(BinaryReader reader, MemoryMappedViewAccessor videoMap, string hostName);

    /// <summary>Remplace le cœur, initialise sa session et écrit sa description selon les codecs du module.</summary>
    /// <typeparam name="TCore">Cœur implémentant le contrat commun de runtime.</typeparam>
    /// <typeparam name="TEmulator">Identité d’adaptateur définie par le module.</typeparam>
    /// <typeparam name="TConfiguration">Configuration opaque reçue par le cœur.</typeparam>
    /// <typeparam name="TOption">Description des options annoncées par ce cœur.</typeparam>
    /// <param name="reader">Lecteur de requête emprunté.</param>
    /// <param name="writer">Rédacteur de réponse emprunté.</param>
    /// <param name="createCore">Fabrique du cœur adaptée à son chemin et à son identité.</param>
    /// <param name="core">Emplacement possédé par la session ; l’ancien cœur est libéré et le nouveau y est publié avant Initialize.</param>
    /// <param name="lastDiagnosticCount">Compteur de diagnostics mis à jour après leur transmission.</param>
    /// <param name="readEmulator">Codec de l’identité d’adaptateur.</param>
    /// <param name="readSaves">Codec du chemin de sauvegarde facultatif.</param>
    /// <param name="readConfiguration">Codec de la configuration du module.</param>
    /// <param name="jsonOptions">Options JSON empruntées pour la description des options et diagnostics.</param>
    /// <param name="extensionSeparator">Séparateur des extensions prises en charge.</param>
    /// <param name="writeStatus">Écriture des statuts propres au module.</param>
    /// <param name="writeLedStates">Écriture des états des voyants.</param>
    /// <remarks>Le service libère et détache immédiatement le cœur dont l'initialisation échoue ; après réussite, la session en reste propriétaire.
    /// Le service ne conserve aucun cœur ni flux après l’appel.</remarks>
    void InitializeCore<TCore, TEmulator, TConfiguration, TOption>(BinaryReader reader, BinaryWriter writer, Func<string, TEmulator, TCore> createCore,
        ref TCore? core, ref int lastDiagnosticCount, Func<BinaryReader, TEmulator> readEmulator, Func<BinaryReader, string?> readSaves,
        Func<BinaryReader, TConfiguration> readConfiguration, Func<JsonSerializerOptions> jsonOptions, char extensionSeparator,
        Action<BinaryWriter, TCore> writeStatus, Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates)
        where TCore : class, IEmulationCoreRuntime<TConfiguration, TOption>;

    /// <summary>Initialise le cœur publié dans l'emplacement de session et le libère si Initialize échoue.</summary>
    /// <typeparam name="TCore">Cœur possédé par la session, implémentant le contrat de runtime.</typeparam>
    /// <typeparam name="TConfiguration">Configuration opaque transmise au cœur.</typeparam>
    /// <typeparam name="TOption">Type des options annoncées par le cœur.</typeparam>
    /// <param name="core">Emplacement contenant le cœur créé ; remis à null avant sa libération en cas d'échec.</param>
    /// <param name="configuration">Configuration empruntée, transmise sans modification.</param>
    /// <param name="sessionDirectory">Répertoire de session fourni par son propriétaire.</param>
    /// <param name="saveDirectory">Répertoire facultatif des sauvegardes persistantes.</param>
    /// <returns>Cœur initialisé, toujours possédé par la session qui doit le libérer dans son finally.</returns>
    /// <remarks>Les répertoires restent empruntés. Un échec ne laisse aucun cœur partiellement initialisé dans la session.</remarks>
    TCore InitializeCore<TCore, TConfiguration, TOption>(ref TCore? core, TConfiguration configuration, string sessionDirectory, string? saveDirectory) where TCore : class, IEmulationCoreRuntime<TConfiguration, TOption>;

    /// <summary>Applique les entrées, exécute une frame et publie vidéo, audio et statuts dans leur ordre existant.</summary>
    /// <typeparam name="TCore">Cœur emprunté implémentant le contrat de runtime.</typeparam>
    /// <typeparam name="TConfiguration">Configuration du runtime.</typeparam>
    /// <typeparam name="TOption">Description de ses options.</typeparam>
    /// <param name="reader">Lecteur emprunté des entrées de la frame.</param>
    /// <param name="writer">Rédacteur emprunté de la réponse.</param>
    /// <param name="core">Cœur possédé et libéré par la session, jamais par cette opération.</param>
    /// <param name="readInput">Codec des entrées.</param>
    /// <param name="writeFrame">Publication de la vidéo propre au transport choisi.</param>
    /// <param name="writeMediaStatus">Écriture des statuts des médias.</param>
    /// <param name="writeLedStates">Écriture des voyants.</param>
    /// <param name="jsonOptions">Options JSON de sérialisation des diagnostics.</param>
    /// <param name="lastVideoSequence">Dernière séquence publiée, actualisée après écriture.</param>
    /// <param name="lastDiagnosticCount">Nombre de diagnostics déjà transmis, actualisé après écriture.</param>
    /// <param name="writeSuccess">Publication de l’en-tête de succès après exécution, avant les données de frame.</param>
    void RunCoreFrame<TCore, TConfiguration, TOption>(BinaryReader reader, BinaryWriter writer, TCore core,
        Func<BinaryReader, EmulationInputSnapshot> readInput, Action<BinaryWriter, VideoFrame?> writeFrame, Action<BinaryWriter, TCore> writeMediaStatus,
        Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates, Func<JsonSerializerOptions> jsonOptions,
        ref long lastVideoSequence, ref int lastDiagnosticCount, Action<BinaryWriter> writeSuccess) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>;

    /// <summary>Écrit le disque sélectionné et le nombre de disques selon le protocole des médias par chemin.</summary>
    /// <param name="writer">Rédacteur binaire emprunté.</param>
    /// <param name="diskCount">Nombre des images annoncé par le cœur de la session.</param>
    /// <param name="currentDiskIndex">Index sélectionné, ou moins un sans sélection.</param>
    void WriteDiskSelection(BinaryWriter writer, int diskCount, int currentDiskIndex);

    /// <summary>Convertit le code de région du cœur natif en standard vidéo.</summary>
    /// <param name="nativeRegion">Code natif : zéro pour NTSC et un pour PAL.</param>
    /// <returns>Standard correspondant, ou null pour une autre valeur.</returns>
    EmulationVideoStandard? Region(uint nativeRegion);

    /// <summary>Ouvre la mémoire vidéo fixe existante et possède sa vue pendant le callback fourni.</summary>
    /// <param name="videoMapName">Nom du segment créé par l’autre extrémité de la session.</param>
    /// <param name="viewOffset">Début de la vue dans le segment, en octets.</param>
    /// <param name="run">Callback qui emprunte la vue uniquement pendant son exécution.</param>
    /// <remarks>La vue puis le segment sont libérés après le callback, y compris si celui-ci échoue.
    /// Le callback ne détruit pas les objets empruntés et ne les conserve pas après son retour.</remarks>
    [SupportedOSPlatform("windows")]
    void WithSharedVideoMap(string videoMapName, long viewOffset, Action<MemoryMappedViewAccessor> run);

    /// <summary>Exécute le callback avec le mode existant de suppression des boîtes d’erreur Windows.</summary>
    /// <param name="run">Action de l’hôte à exécuter sans conserver sa référence.</param>
    /// <remarks>Le mode d’erreur précédent est restauré dans finally, même après une exception du callback.</remarks>
    [SupportedOSPlatform("windows")]
    void WithSuppressedErrorDialogs(Action run);
    /// <summary>Convertit une valeur de protocole vers un standard vidéo connu.</summary>
    /// <param name="value">Valeur reçue dans le bloc de statut du cœur.</param>
    /// <param name="allowAllDefinedStandards">True admet tous les standards déclarés ; false conserve la restriction PAL et NTSC.</param>
    /// <returns>Standard correspondant, ou null lorsque la valeur n’est pas admise.</returns>
    EmulationVideoStandard? ReadRegion(int value, bool allowAllDefinedStandards);

    /// <summary>Consulte l’état du processus emprunté avec le même type de statut pour tous les modules.</summary>
    /// <param name="process">Processus de session, ou null avant son lancement.</param>
    /// <param name="connectionFailed">Indique une rupture de transport prioritaire sur les autres états.</param>
    /// <param name="disposed">Indique que le propriétaire a déjà libéré la session.</param>
    /// <param name="hostProcessId">Identifiant conservé après libération ; null avant tout démarrage.</param>
    /// <returns>État calculé selon la politique commune existante ; le processus reste emprunté.</returns>
    HostProcessState GetProcessState(Process? process, bool connectionFailed, bool disposed, int? hostProcessId = null);

    /// <summary>Démarre le processus d’hôte et le rattache à la durée de vie de l’application.</summary>
    /// <param name="startInfo">Paramètres de lancement préparés par le module, empruntés pendant l’appel.</param>
    /// <param name="startFailed">Construction de l’erreur lorsque le lancement ne fournit pas de processus.</param>
    /// <returns>Processus démarré dont la propriété est transférée à la session appelante.</returns>
    /// <remarks>Si le rattachement échoue, le service termine le processus, attend sa sortie et libère son handle.
    /// Après un lancement réussi, le propriétaire attend sa sortie dans son finally puis libère le processus.</remarks>
    Process StartHostProcess(ProcessStartInfo startInfo, Func<Exception> startFailed);

    /// <summary>Attend la sortie effective du processus d’hôte, en le terminant si nécessaire.</summary>
    /// <param name="process">Processus emprunté ; son propriétaire libère ensuite son handle.</param>
    /// <param name="graceful">True accorde d’abord un délai à la fermeture normale déjà demandée par la session.</param>
    /// <param name="gracefulTimeoutMilliseconds">Délai de fermeture normale en millisecondes.</param>
    /// <remarks>L’attente finale a lieu dans le finally du traitement existant, y compris après un échec de terminaison.
    /// Cette opération conserve le processus sous la propriété de l’appelant et ne libère pas son handle.</remarks>
    void WaitForHostExit(Process process, bool graceful, int gracefulTimeoutMilliseconds);

    /// <summary>Sérialise une requête sous le verrou de session, reçoit sa réponse puis applique son lecteur.</summary>
    /// <param name="gate">Verrou emprunté qui protège une requête et sa réponse contre les échanges concurrents.</param>
    /// <param name="getWriter">Accès au rédacteur courant de la session, conservé par son propriétaire.</param>
    /// <param name="getResponse">Réception de la réponse binaire correspondant à la requête.</param>
    /// <param name="write">Écriture des paramètres propres à cette requête.</param>
    /// <param name="read">Lecture des résultats propres à cette réponse.</param>
    /// <param name="failConnection">Notification de défaillance chargée de fermer la connexion selon la politique du propriétaire.</param>
    void ExecuteRequest(SemaphoreSlim gate, Func<BinaryWriter> getWriter, Func<byte[]> getResponse, Action<BinaryWriter> write, Action<BinaryReader> read, Action<Exception> failConnection);

    /// <summary>Lit une réponse bornée par son préfixe de longueur, le délai de réception et l’annulation.</summary>
    /// <param name="pipe">Tube emprunté ; son propriétaire assure sa fermeture à la fin de la session.</param>
    /// <param name="responseTimeout">Durée maximale accordée à la réception de la réponse.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’opération en cours.</param>
    /// <param name="invalidLength">Construction du diagnostic associé à une longueur de réponse invalide.</param>
    Task<byte[]> ReadResponseAsync(Stream pipe, TimeSpan responseTimeout, CancellationToken cancellationToken, Func<int, string> invalidLength);

    /// <summary>Lit une chaîne facultative selon le marqueur de présence du protocole.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    string? ReadString(BinaryReader reader);

    /// <summary>Écrit la longueur puis les octets empruntés dans le message courant.</summary>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="bytes">Octets empruntés ; leur contenu est copié dans le message.</param>
    void WriteBytes(BinaryWriter writer, ReadOnlySpan<byte> bytes);

    /// <summary>Lit un bloc d’octets en vérifiant sa longueur et la présence de toutes ses données.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="hostName">Nom de l’hôte employé pour contextualiser les diagnostics de protocole.</param>
    byte[] ReadBytes(BinaryReader reader, string hostName);

    /// <summary>Écrit un instantané d’entrées selon le format binaire commun.</summary>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="input">Instantané d’entrées emprunté à sérialiser.</param>
    void WriteInput(BinaryWriter writer, EmulationInputSnapshot input);

    /// <summary>Reconstruit un instantané d’entrées depuis le format binaire commun.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="hostName">Nom de l’hôte employé pour contextualiser les diagnostics de protocole.</param>
    EmulationInputSnapshot ReadInput(BinaryReader reader, string hostName);

    /// <summary>Reconstruit les blocs PCM d’une réponse avec leurs caractéristiques audio.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="hostName">Nom de l’hôte employé pour contextualiser les diagnostics de protocole.</param>
    IReadOnlyList<AudioChunk> ReadAudio(BinaryReader reader, string hostName);

    /// <summary>Écrit les identifiants de voyants et leur état logique.</summary>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="states">États des voyants indexés par leur identifiant de cœur.</param>
    void WriteLedStates(BinaryWriter writer, IReadOnlyDictionary<int, bool> states);

    /// <summary>Lit les identifiants de voyants et leur état logique.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="hostName">Nom de l’hôte employé pour contextualiser les diagnostics de protocole.</param>
    IReadOnlyDictionary<int, bool> ReadLedStates(BinaryReader reader, string hostName);

    /// <summary>Prépare les arguments de lancement de l’hôte sans démarrer le processus.</summary>
    /// <param name="executablePath">Chemin de l’exécutable qui héberge le cœur.</param>
    /// <param name="workingDirectory">Répertoire de travail du futur processus.</param>
    /// <param name="hostCommand">Argument qui sélectionne le mode hôte de l’exécutable.</param>
    /// <param name="pipeName">Nom du tube de commande propre à cette session.</param>
    /// <param name="videoMapName">Nom du segment de mémoire vidéo propre à cette session.</param>
    ProcessStartInfo CreateHostStartInfo(string executablePath, string workingDirectory, string hostCommand, string pipeName, string videoMapName);

    /// <summary>Attend la connexion du client sur le tube emprunté, avec le délai fourni.</summary>
    /// <param name="pipe">Tube emprunté ; son propriétaire assure sa fermeture à la fin de la session.</param>
    /// <param name="connectionTimeout">Délai maximal d’attente de la connexion.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’opération en cours.</param>
    void WaitForConnection(NamedPipeServerStream pipe, TimeSpan connectionTimeout, CancellationToken cancellationToken);

    /// <summary>Attend la connexion du client sur le tube emprunté, avec le délai fourni.</summary>
    /// <param name="pipe">Tube emprunté ; son propriétaire assure sa fermeture à la fin de la session.</param>
    /// <param name="connectionTimeout">Délai maximal d’attente de la connexion.</param>
    void WaitForConnection(NamedPipeServerStream pipe, TimeSpan connectionTimeout);

    /// <summary>Libère un transport possédé en ignorant uniquement les erreurs d’E/S et de ressource déjà libérée.</summary>
    /// <param name="resource">Transport possédé que l’appelant souhaite libérer ; null est accepté.</param>
    void DisposeTransport(IDisposable? resource);

    /// <summary>Retire la référence au cœur précédent puis le libère, même si sa destruction échoue.</summary>
    /// <typeparam name="TCore">Type de cœur fourni par le module et contraint par les capacités nécessaires à cette opération.</typeparam>
    /// <param name="core">Cœur concerné ; sa propriété reste à l’appelant sauf lors de sa destruction explicitement demandée.</param>
    void DisposePreviousCore<TCore>(ref TCore? core) where TCore : class, IDisposable;

    /// <summary>Lit les paramètres de création du cœur avec les codecs de configuration fournis.</summary>
    /// <typeparam name="TConfiguration">Type de configuration du module, conservé par les codecs de celui-ci.</typeparam>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="readSaves">Codec du chemin facultatif des sauvegardes, appelé dans l’ordre de lecture du protocole.</param>
    /// <param name="readConfiguration">Codec de la configuration opaque au transport.</param>
    (string CorePath, string Session, string? Saves, TConfiguration Configuration) ReadInitializationRequest<TConfiguration>(BinaryReader reader, Func<BinaryReader, string?> readSaves, Func<BinaryReader, TConfiguration> readConfiguration);

    /// <summary>Lit les paramètres de création du cœur avec les codecs de configuration fournis.</summary>
    /// <typeparam name="TEmulator">Type d’identité d’émulateur propre au module.</typeparam>
    /// <typeparam name="TConfiguration">Type de configuration du module, conservé par les codecs de celui-ci.</typeparam>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="readEmulator">Codec de l’identité de l’émulateur propre au module.</param>
    /// <param name="readSaves">Codec du chemin facultatif des sauvegardes, appelé dans l’ordre de lecture du protocole.</param>
    /// <param name="readConfiguration">Codec de la configuration opaque au transport.</param>
    (string CorePath, TEmulator Emulator, string Session, string? Saves, TConfiguration Configuration) ReadInitializationRequest<TEmulator, TConfiguration>(BinaryReader reader, Func<BinaryReader, TEmulator> readEmulator, Func<BinaryReader, string?> readSaves, Func<BinaryReader, TConfiguration> readConfiguration);

    /// <summary>Écrit les paramètres de création du cœur dans l’ordre du protocole.</summary>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="corePath">Chemin du cœur choisi pour la session.</param>
    /// <param name="session">Accès au répertoire de la session courante.</param>
    /// <param name="saves">Accès au répertoire facultatif des sauvegardes persistantes.</param>
    /// <param name="writeSaves">Codec du chemin facultatif des sauvegardes.</param>
    /// <param name="serializeConfiguration">Sérialisation de la configuration par son propriétaire.</param>
    void WriteInitializationRequest(ref BinaryWriter? writer, string corePath, Func<string?> session, Func<string?> saves, Action<BinaryWriter, string?> writeSaves, Func<string> serializeConfiguration);

    /// <summary>Écrit les paramètres de création du cœur dans l’ordre du protocole.</summary>
    /// <typeparam name="TConfiguration">Type de configuration du module, conservé par les codecs de celui-ci.</typeparam>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="corePath">Chemin du cœur choisi pour la session.</param>
    /// <param name="emulator">Identifiant numérique de l’émulateur sélectionné par le module.</param>
    /// <param name="sessionDirectory">Répertoire de la session courante.</param>
    /// <param name="saveDirectory">Répertoire des sauvegardes persistantes, ou null lorsqu’il est absent.</param>
    /// <param name="configuration">Configuration du module transmise sans interprétation de son modèle.</param>
    /// <param name="jsonOptions">Conventions JSON partagées par les deux extrémités du protocole.</param>
    void WriteInitializationRequest<TConfiguration>(BinaryWriter writer, string corePath, int emulator, string sessionDirectory, string? saveDirectory, TConfiguration configuration, JsonSerializerOptions jsonOptions);

    /// <summary>Écrit les capacités initiales du cœur et les diagnostics non encore transmis.</summary>
    /// <typeparam name="TCore">Type de cœur fourni par le module et contraint par les capacités nécessaires à cette opération.</typeparam>
    /// <typeparam name="TConfiguration">Type de configuration du module, conservé par les codecs de celui-ci.</typeparam>
    /// <typeparam name="TOption">Type des options exposées par le cœur.</typeparam>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="core">Cœur concerné ; sa propriété reste à l’appelant sauf lors de sa destruction explicitement demandée.</param>
    /// <param name="jsonOptions">Conventions JSON partagées par les deux extrémités du protocole.</param>
    /// <param name="lastDiagnosticCount">Nombre de diagnostics déjà transmis, actualisé après écriture.</param>
    /// <param name="extensionSeparator">Séparateur des extensions acceptées annoncé au client.</param>
    /// <param name="writeStatus">Écriture facultative du statut complémentaire du cœur.</param>
    void WriteCoreInitialization<TCore, TConfiguration, TOption>(BinaryWriter writer, TCore core, JsonSerializerOptions jsonOptions, ref int lastDiagnosticCount, char extensionSeparator, Action<BinaryWriter, TCore>? writeStatus = null) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>;

    /// <summary>Écrit l’image, l’audio, les voyants et les diagnostics produits par le cœur.</summary>
    /// <typeparam name="TCore">Type de cœur fourni par le module et contraint par les capacités nécessaires à cette opération.</typeparam>
    /// <typeparam name="TConfiguration">Type de configuration du module, conservé par les codecs de celui-ci.</typeparam>
    /// <typeparam name="TOption">Type des options exposées par le cœur.</typeparam>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="core">Cœur concerné ; sa propriété reste à l’appelant sauf lors de sa destruction explicitement demandée.</param>
    /// <param name="writeFrame">Publication de l’image selon le transport vidéo de la session.</param>
    /// <param name="writeLedStates">Écriture des états de voyants selon le codec commun.</param>
    /// <param name="jsonOptions">Conventions JSON partagées par les deux extrémités du protocole.</param>
    /// <param name="lastVideoSequence">Dernière séquence vidéo transmise, actualisée après publication.</param>
    /// <param name="lastDiagnosticCount">Nombre de diagnostics déjà transmis, actualisé après écriture.</param>
    /// <param name="writeStateSupport">Écriture facultative des capacités de sauvegarde d’état.</param>
    /// <param name="writeMediaStatus">Codec emprunté des champs de médias ajoutés après le statut partagé, ou null lorsque ces champs sont absents.</param>
    void WriteNativeCoreFrameResponse<TCore, TConfiguration, TOption>( BinaryWriter writer, TCore core, Action<BinaryWriter, VideoFrame?> writeFrame, Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates, Func<JsonSerializerOptions> jsonOptions, ref long lastVideoSequence, ref int lastDiagnosticCount, Action<BinaryWriter, TCore>? writeStateSupport = null, Action<BinaryWriter, TCore>? writeMediaStatus = null) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>;

    /// <summary>Écrit le standard vidéo et les compteurs du tampon audio.</summary>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="region">Standard vidéo annoncé par le cœur, ou null s’il est inconnu.</param>
    /// <param name="bufferedAudioFrames">Nombre de trames audio actuellement en attente.</param>
    /// <param name="audioOverrunCount">Nombre cumulé de dépassements de capacité audio.</param>
    /// <param name="audioUnderrunCount">Nombre cumulé de manques de données audio.</param>
    void WriteRuntimeStatus(BinaryWriter writer, EmulationVideoStandard? region, int bufferedAudioFrames, long audioOverrunCount, long audioUnderrunCount);

    /// <summary>Lit le standard vidéo et transmet les compteurs audio aux récepteurs fournis.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="setRegion">Réception de la valeur numérique du standard vidéo.</param>
    /// <param name="setBufferedAudioFrames">Réception du nombre de trames audio en attente.</param>
    /// <param name="setAudioOverrunCount">Réception du compteur de dépassements audio.</param>
    /// <param name="setAudioUnderrunCount">Réception du compteur de manques audio.</param>
    void ReadRuntimeStatus(BinaryReader reader, Action<int> setRegion, Action<int> setBufferedAudioFrames, Action<long> setAudioOverrunCount, Action<long> setAudioUnderrunCount);

    /// <summary>Exécute une commande commune reconnue et laisse les commandes spécifiques à l’appelant.</summary>
    /// <typeparam name="TMedia">Type de média accepté par le cœur.</typeparam>
    /// <typeparam name="TPeripheral">Type de périphérique reconnu par le cœur.</typeparam>
    /// <param name="command">Code binaire de la commande à reconnaître.</param>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="ensureCore">Accès au cœur initialisé sous son interface de commandes.</param>
    /// <param name="readMedia">Codec des médias propres au module.</param>
    /// <param name="readBytes">Codec des blocs d’état binaire.</param>
    /// <param name="writeDiskStatus">Codec du statut de disque propre au module.</param>
    /// <param name="decodePeripheral">Conversion de l’identifiant binaire vers le périphérique du module.</param>
    /// <param name="writeSuccess">En-tête de succès du transport ; null lorsque le succès précède déjà la charge utile.</param>
    bool TryExecuteCoreCommand<TMedia, TPeripheral>( byte command, BinaryReader reader, BinaryWriter writer, Func<IEmulationCoreCommands<TMedia, TPeripheral>> ensureCore, Func<BinaryReader, TMedia> readMedia, Func<BinaryReader, byte[]> readBytes, Action<BinaryWriter, DiskStatus> writeDiskStatus, Func<int, TPeripheral> decodePeripheral, Action<BinaryWriter>? writeSuccess = null);

    /// <summary>Possède la connexion cliente et ses codecs pendant l’exécution du traitement de session.</summary>
    /// <param name="pipeName">Nom du tube de commande propre à cette session.</param>
    /// <param name="serverName">Nom du serveur auquel se connecter.</param>
    /// <param name="connectionTimeoutMilliseconds">Délai maximal de connexion, en millisecondes.</param>
    /// <param name="run">Traitement qui emprunte les codecs pendant la session ; ceux-ci sont libérés à son retour.</param>
    void RunConnectedSession(string pipeName, string serverName, int connectionTimeoutMilliseconds, Action<BinaryReader, BinaryWriter> run);

    /// <summary>Traite les commandes successives jusqu’à la commande de destruction ou la fin du transport.</summary>
    /// <typeparam name="TCommand">Énumération des commandes du protocole de module.</typeparam>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="transportWriter">Rédacteur emprunté du transport de réponse.</param>
    /// <param name="readCommand">Lecture et conversion du code de commande.</param>
    /// <param name="disposeCommand">Commande qui termine la boucle et provoque le retour au propriétaire de session.</param>
    /// <param name="execute">Exécution d’une commande et écriture de sa réponse.</param>
    /// <param name="writeFailure">Sérialisation d’une erreur dans la réponse de commande.</param>
    void RunCommandLoop<TCommand>(BinaryReader reader, BinaryWriter transportWriter, Func<BinaryReader, TCommand> readCommand, TCommand disposeCommand, Action<TCommand, BinaryWriter> execute, Action<BinaryWriter, Exception> writeFailure) where TCommand : struct, Enum;

    /// <summary>Calcule une capacité vidéo bornée, arrondie à une puissance de deux.</summary>
    /// <param name="frameLength">Longueur du contenu d’image en octets.</param>
    /// <param name="minimumSlotCapacity">Capacité minimale d’un emplacement vidéo, en octets.</param>
    /// <param name="hostName">Nom de l’hôte employé pour contextualiser les diagnostics de protocole.</param>
    int CalculateVideoSlotCapacity(int frameLength, int minimumSlotCapacity, string hostName);

    /// <summary>Publie une image dans la mémoire partagée et écrit sa description dans le transport.</summary>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="frame">Image à publier, ou null lorsque le cœur n’a pas produit de nouvelle image.</param>
    /// <param name="video">Rédacteur de mémoire vidéo emprunté, possédé et libéré par la session.</param>
    [SupportedOSPlatform("windows")]
    void WriteResizableSharedFrame(BinaryWriter writer, VideoFrame? frame, IEmulationSharedVideoWriter video);

    /// <summary>Lit une description d’image et adapte la vue de mémoire partagée lorsque sa génération change.</summary>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="memory">Segment possédé par l’appelant ; l’ancien segment est libéré lors de son remplacement.</param>
    /// <param name="view">Vue possédée par l’appelant ; l’ancienne vue est libérée lors de son remplacement.</param>
    /// <param name="activeName">Nom de la génération actuellement ouverte, actualisé avec le segment.</param>
    /// <param name="minimumSlotCapacity">Capacité minimale d’un emplacement vidéo, en octets.</param>
    [SupportedOSPlatform("windows")]
    VideoFrame? ReadResizableSharedFrame(BinaryReader reader, ref MemoryMappedFile? memory, ref MemoryMappedViewAccessor? view, ref string? activeName, int minimumSlotCapacity);
    /// <summary>Lit l’instantané audio après les entrées d’une requête de frame.</summary>
    /// <param name="reader">Lecteur emprunté, positionné sur la présence du statut.</param>
    /// <returns>Valeurs de fréquence, remplissage, capacité et activité, ou null si aucun statut n’est disponible.</returns>
    AudioOutputBufferStatus? ReadAudioBufferStatus(BinaryReader reader);
    /// <summary>Écrit le même instantané dans une requête de frame destinée au cœur hébergé.</summary>
    /// <param name="writer">Rédacteur emprunté ; sa durée de vie reste possédée par le transport.</param>
    /// <param name="status">Valeurs copiées du tampon de sortie ; null représente une sortie indisponible.</param>
    void WriteAudioBufferStatus(BinaryWriter writer, AudioOutputBufferStatus? status);
    /// <summary>Écrit la catégorie et l’indice complets d’un lecteur dans une requête empruntée.</summary>
    /// <param name="writer">Rédacteur du transport ; son appelant conserve sa propriété.</param>
    /// <param name="slot">Identité du lecteur, y compris les cartes mémoire et lecteurs d’indice supérieur à zéro.</param>
    /// <remarks>Le codec vérifie la catégorie et un indice positif ou nul avant toute écriture.</remarks>
    void WriteMediaSlot(BinaryWriter writer, EmulationMediaSlot slot);
    /// <summary>Lit puis valide la catégorie et l’indice d’un lecteur dans la requête empruntée.</summary>
    /// <param name="reader">Lecteur positionné sur les deux entiers représentant le lecteur.</param>
    /// <returns>Identité du lecteur sans limitation aux identifiants de périphériques natifs historiques.</returns>
    EmulationMediaSlot ReadMediaSlot(BinaryReader reader);
}
