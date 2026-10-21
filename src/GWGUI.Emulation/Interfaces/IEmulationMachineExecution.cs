using System.Collections.Concurrent;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Exécution commune des sessions de machine par un contrat indépendant de l’identité du module.</summary>
/// <remarks>Le service est sans état de session. La machine possède ses threads, cœurs et files ; elle demande
/// leur arrêt puis attend leur destruction effective dans son finally avant de libérer ses autres ressources.</remarks>
public interface IEmulationMachineExecution
{
    /// <summary>Démarre le thread de session avec ses signaux de démarrage et de fin.</summary>
    /// <param name="run">Boucle ou action empruntée au propriétaire de la session.</param>
    /// <param name="name">Nom technique du thread de cœur.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’opération ou de la session.</param>
    /// <param name="stop">Source d’arrêt possédée par la machine et remplacée lors du nouveau démarrage.</param>
    /// <param name="started">Signal de démarrage fourni à la machine, que la boucle doit terminer.</param>
    /// <param name="loop">Tâche signalant la fin de la boucle de session.</param>
    /// <param name="thread">Thread possédé, dont la fin effective doit être attendue avant libération du cœur.</param>
    void StartThread(Action<CancellationToken> run, string name, CancellationToken cancellationToken, ref CancellationTokenSource? stop, out TaskCompletionSource started, out Task? loop, ref Thread? thread);

    /// <summary>Exécute l’initialisation et la boucle, puis le nettoyage dans finally même après un échec.</summary>
    /// <param name="initialize">Initialisation du cœur empruntée pendant le démarrage.</param>
    /// <param name="run">Boucle ou action empruntée au propriétaire de la session.</param>
    /// <param name="selectFailureHandler">Sélection du traitement d’échec propre à la machine.</param>
    /// <param name="cleanup">Nettoyage du propriétaire, toujours exécuté dans le parcours finally.</param>
    void RunCore(Action initialize, Action run, Func<Exception, Action> selectFailureHandler, Action<bool> cleanup);

    /// <summary>Attend l’arrêt puis exécute toutes les libérations dans leur ordre, chacune protégée par finally.</summary>
    /// <param name="stop">Source d’arrêt possédée par la machine et remplacée lors du nouveau démarrage.</param>
    /// <param name="disposeFirst">Première libération de ressource possédée.</param>
    /// <param name="disposeSecond">Deuxième libération, exécutée même si la première échoue.</param>
    /// <param name="disposeThird">Troisième libération, exécutée même après un échec précédent.</param>
    /// <param name="cleanup">Nettoyage du propriétaire, toujours exécuté dans le parcours finally.</param>
    ValueTask DisposeMachineAsync(Func<ValueTask> stop, Action disposeFirst, Action disposeSecond, Action disposeThird, Action cleanup);

    /// <summary>Attend le démarrage et demande l’arrêt si cette attente échoue ou est annulée.</summary>
    /// <param name="started">Signal de démarrage fourni à la machine, que la boucle doit terminer.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’opération ou de la session.</param>
    /// <param name="stop">Source d’arrêt possédée par la machine et remplacée lors du nouveau démarrage.</param>
    ValueTask WaitForStartOrStopAsync(Task started, CancellationToken cancellationToken, Func<ValueTask> stop);

    /// <summary>Attend la fin de la boucle puis la destruction effective de son thread dans finally.</summary>
    /// <param name="loop">Tâche signalant la fin de la boucle de session.</param>
    /// <param name="thread">Thread possédé, dont la fin effective doit être attendue avant libération du cœur.</param>
    ValueTask WaitForThreadAsync(Task? loop, Thread? thread);

    /// <summary>Tente la destruction d’un cœur et transmet son éventuel échec au propriétaire.</summary>
    /// <param name="core">Cœur possédé que l’appelant demande explicitement de libérer.</param>
    /// <param name="reportFailure">Récepteur de l’erreur de destruction, emprunté au propriétaire.</param>
    bool TryDisposeCore(IDisposable core, Action<Exception> reportFailure);

    /// <summary>Exécute les commandes et les frames dans la boucle commune avec attente par échéance.</summary>
    /// <param name="nextFrame">Première échéance, en ticks de Stopwatch.</param>
    /// <param name="gate">Verrou emprunté protégeant l’état et les commandes de la machine.</param>
    /// <param name="commands">File de commandes possédée par la machine.</param>
    /// <param name="isPaused">Consultation de l’état de pause sous les règles de verrouillage de la machine.</param>
    /// <param name="runFrame">Calcul d’une frame et traitement de ses sorties.</param>
    /// <param name="framesPerSecond">Consultation de la cadence courante du cœur, en images par seconde.</param>
    /// <param name="nextFrameTimestamp">Calcul de l’échéance suivante dans la même base de temps que nextFrame.</param>
    /// <param name="waitUntil">Attente de l’échéance par le timer possédé de la session.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’opération ou de la session.</param>
    /// <param name="pauseWaitMilliseconds">Intervalle de vérification des commandes pendant une pause, en millisecondes.</param>
    void RunLoop(long nextFrame, object gate, ConcurrentQueue<EmulationMachineCommand> commands, Func<bool> isPaused, Action runFrame, Func<double> framesPerSecond, Func<long, double, long> nextFrameTimestamp, Action<long, CancellationToken> waitUntil, CancellationToken cancellationToken, int pauseWaitMilliseconds);

    /// <summary>Publie chaque nouvelle image puis consomme l’audio dans son ordre de production.</summary>
    /// <typeparam name="TMachine">Type de la machine empruntée pendant la publication.</typeparam>
    /// <param name="machine">Machine empruntée portant les événements de sorties.</param>
    /// <param name="videoSequence">Dernière séquence vidéo publiée, actualisée après publication.</param>
    /// <param name="latestVideo">Accès à la dernière image du cœur, sans transfert de propriété.</param>
    /// <param name="dequeueAudio">Consommation du prochain bloc audio, ou null lorsque la file est vide.</param>
    /// <param name="publishVideo">Notification de l’image au propriétaire de la présentation.</param>
    /// <param name="publishAudio">Notification du bloc audio à son consommateur.</param>
    void PublishOutputs<TMachine>(TMachine machine, ref long videoSequence, Func<TMachine, VideoFrame?> latestVideo, Func<TMachine, AudioChunk?> dequeueAudio, Action<TMachine, VideoFrame> publishVideo, Action<TMachine, AudioChunk> publishAudio);

    /// <summary>Place une commande sous le contrôle de la machine et réveille sa boucle.</summary>
    /// <param name="commands">File de commandes possédée par la machine.</param>
    /// <param name="gate">Verrou emprunté protégeant l’état et les commandes de la machine.</param>
    /// <param name="action">Commande empruntée, exécutée sur le thread propriétaire du cœur.</param>
    /// <param name="cancellationToken">Signal d’annulation de l’opération ou de la session.</param>
    /// <param name="validateInsideGate">Contrôle facultatif effectué sous le verrou avant l’ajout de la commande.</param>
    ValueTask EnqueueCommand(ConcurrentQueue<EmulationMachineCommand> commands, object gate, Action action, CancellationToken cancellationToken, Action? validateInsideGate = null);

    /// <summary>Termine en échec les commandes encore en attente lors de l’arrêt.</summary>
    /// <param name="commands">File de commandes possédée par la machine.</param>
    /// <param name="error">Cause transmise aux attentes des commandes non exécutées.</param>
    void FailPendingCommands(ConcurrentQueue<EmulationMachineCommand> commands, Exception error);
}
