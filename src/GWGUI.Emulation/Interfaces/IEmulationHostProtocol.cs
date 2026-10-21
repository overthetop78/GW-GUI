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
    /// <summary>Convertit une valeur de protocole vers un standard vidéo connu.</summary>
    /// <param name="value">Valeur reçue dans le bloc de statut du cœur.</param>
    /// <param name="allowAllDefinedStandards">True admet tous les standards déclarés ; false conserve la restriction PAL et NTSC.</param>
    /// <returns>Standard correspondant, ou null lorsque la valeur n’est pas admise.</returns>
    EmulationVideoStandard? ReadRegion(int value, bool allowAllDefinedStandards);

    /// <summary>Projette l’état du processus emprunté vers l’énumération de statut du module.</summary>
    /// <typeparam name="TState">Énumération de statut définie par le module.</typeparam>
    /// <param name="process">Processus de session, ou null avant son lancement.</param>
    /// <param name="connectionFailed">Indique une rupture de transport prioritaire sur les autres états.</param>
    /// <param name="disposed">Indique que le propriétaire a déjà libéré la session.</param>
    /// <param name="faulted">Valeur à retourner après une rupture de transport.</param>
    /// <param name="notStarted">Valeur à retourner en l’absence de processus.</param>
    /// <param name="exited">Valeur à retourner lorsque la sortie du processus est observée.</param>
    /// <param name="running">Valeur à retourner lorsque le processus est actif.</param>
    /// <returns>État calculé selon la politique commune existante ; le processus reste emprunté.</returns>
    TState GetProcessState<TState>(Process? process, bool connectionFailed, bool disposed, TState faulted, TState notStarted, TState exited, TState running) where TState : struct, Enum;

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
    /// <param name="writeAudioStatus">Écriture facultative des compteurs et capacités audio.</param>
    void WriteNativeCoreFrameResponse<TCore, TConfiguration, TOption>( BinaryWriter writer, TCore core, Action<BinaryWriter, VideoFrame?> writeFrame, Action<BinaryWriter, IReadOnlyDictionary<int, bool>> writeLedStates, Func<JsonSerializerOptions> jsonOptions, ref long lastVideoSequence, ref int lastDiagnosticCount, Action<BinaryWriter, TCore>? writeStateSupport = null, Action<BinaryWriter, TCore>? writeAudioStatus = null) where TCore : IEmulationCoreRuntime<TConfiguration, TOption>, IEmulationCoreRuntimeStatus;

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
    /// <typeparam name="TDiskStatus">Type du statut de disque exposé par le cœur.</typeparam>
    /// <param name="command">Code binaire de la commande à reconnaître.</param>
    /// <param name="reader">Lecteur binaire emprunté, positionné au début des données attendues.</param>
    /// <param name="writer">Rédacteur binaire emprunté, positionné au début des données à écrire.</param>
    /// <param name="ensureCore">Accès au cœur initialisé sous son interface de commandes.</param>
    /// <param name="readMedia">Codec des médias propres au module.</param>
    /// <param name="readBytes">Codec des blocs d’état binaire.</param>
    /// <param name="writeDiskStatus">Codec du statut de disque propre au module.</param>
    /// <param name="decodePeripheral">Conversion de l’identifiant binaire vers le périphérique du module.</param>
    bool TryExecuteCoreCommand<TMedia, TPeripheral, TDiskStatus>( byte command, BinaryReader reader, BinaryWriter writer, Func<IEmulationCoreCommands<TMedia, TPeripheral, TDiskStatus>> ensureCore, Func<BinaryReader, TMedia> readMedia, Func<BinaryReader, byte[]> readBytes, Action<BinaryWriter, TDiskStatus> writeDiskStatus, Func<int, TPeripheral> decodePeripheral);

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
}
