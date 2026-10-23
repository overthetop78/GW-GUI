using CommonCoreHostFunctions = GWGUI.Emulation.Example.Common.Functions.CoreHostFunctions;
using System.Diagnostics;
using GWGUI.Emulation.Exceptions;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Services;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Machines.Common.Constants;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Commandes sérialisées, boucle d’exécution et libération des ressources possédées par la machine.</summary>
/// <remarks>Le thread du cœur exécute les commandes et les frames ; il reste disponible pour les commandes
/// pendant une pause. Le contrôleur audio commun possède ses sorties créées, tandis que le propriétaire de
/// la machine attend DisposeAsync pour libérer effectivement le cœur, son thread et sa session.</remarks>
internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput, IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    /// <summary>Projette les montages et les associations d’entrée actuellement utilisés par la session.</summary>
    /// <returns>Configuration portant les médias et entrées courants, sans modifier la configuration initiale.</returns>
    /// <remarks>Cette consultation a lieu sur le thread du cœur ou sous le verrou de la machine.
    /// Les lecteurs dont le média est éjecté restent décrits dans les montages courants.</remarks>
    private MachineConfiguration CurrentConfiguration() => Configuration with { Media = _mountedMedia.ToArray(), Input = _inputConfiguration };

    /// <summary>Ajoute une opération à la file de commandes du thread du cœur.</summary>
    /// <param name="action">Opération empruntée, exécutée après les commandes déjà présentes.</param>
    /// <param name="cancellationToken">Annulation de la commande avant ou pendant son attente.</param>
    /// <param name="category">Catégorie du diagnostic d’une erreur non déjà contextualisée.</param>
    /// <param name="code">Code public à associer à cette erreur.</param>
    /// <returns>Attente de la commande et de son éventuel diagnostic.</returns>
    /// <exception cref="ObjectDisposedException">La machine a été libérée.</exception>
    /// <exception cref="InvalidOperationException">La machine n’est ni en marche ni en pause.</exception>
    /// <remarks>La validation est effectuée sous le verrou partagé avec l’arrêt. Une annulation reste une
    /// annulation ; les autres erreurs sont contextualisées pour la machine avant leur transmission.</remarks>
    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken, EmulationMessageCategory category = EmulationMessageCategory.Machine, EmulationMessageCode code = EmulationMessageCode.MachineStartFailed)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return CommonCoreHostFunctions.MachineExecution.EnqueueCommand(_commands, _gate, () =>
        {
            try { action(); }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                throw EmulationSdk.Messages.Translate(error, category, code, new EmulationMachineMessageContext(Configuration.MachineId));
            }
        }, cancellationToken, validateInsideGate: () =>
        {
            ThrowIfDisposed();
            if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
        });
    }

    /// <summary>Initialise le cœur, exécute les frames à sa cadence et ferme la session à la sortie de boucle.</summary>
    /// <param name="cancellationToken">Signal d’arrêt de la boucle et des attentes entre frames.</param>
    /// <remarks>Le démarrage est signalé après Initialize et le démarrage audio. Avant chaque frame,
    /// les entrées physiques sont projetées par le mapper Common ; le contrôleur de cassette peut ensuite
    /// ajouter ses impulsions de commande. Le cœur reçoit aussi l’état du tampon physique de sortie.
    /// Une demande de fermeture du cœur arrête la boucle par son signal d’arrêt, sans devenir une erreur.
    /// Le finally commun libère le cœur, arrête l’audio et termine les commandes restantes avant de publier Stopped.</remarks>
    private void Run(CancellationToken cancellationToken)
    {
        CommonCoreHostFunctions.MachineExecution.RunCore(
            () => _core.Initialize(CurrentConfiguration(), _sessionDirectory, _saveDirectory),
            () =>
            {
                StartAudio();
                lock (_gate)
                {
                    State = EmulationMachineState.Running;
                    _started?.TrySetResult();
                }
                var nextFrame = Stopwatch.GetTimestamp();
                using var frameTimer = CommonCoreHostFunctions.MachineExecution.CreateFrameTimer(cancellationToken);
                long videoSequence = default;
                CommonCoreHostFunctions.MachineExecution.RunLoop(nextFrame, _gate, _commands, () => _pauseRequested,
                    () =>
                    {
                        _input.Poll();
                        var input = InputSnapshotFunctions.Apply(_input.Polled, CurrentConfiguration(), _inputMapper, _controllerPointerSwitchPressed);
                        if (_cassetteInput is not null)
                        {
                            _cassetteInput.SetPhysicalInput(input);
                            input = _cassetteInput.NextFrame();
                        }
                        _core.SetInput(input);
                        if (_core is IEmulatorAudioBufferFeedback feedback)
                            feedback.SetAudioBufferStatus(_audio.BufferStatus);
                        _core.RunFrame();
                        if (_core is IEmulatorAudioBufferFeedback latency)
                            _audio.TrySetMinimumLatency(latency.MinimumAudioLatencyMilliseconds);
                        PublishOutputs(ref videoSequence);
                        if (_core.ShutdownRequested) _stopSource?.Cancel();
                    },
                    () => _core.FramesPerSecond, MachineFunctions.NextFrameTimestamp, frameTimer.WaitUntil,
                    cancellationToken, MachineConstants.PauseWaitMilliseconds, () => !_core.ShutdownRequested);
            },
            error => () =>
            {
                if (error is OperationCanceledException && cancellationToken.IsCancellationRequested)
                    _started?.TrySetCanceled(cancellationToken);
                else
                    ReportFailure(error);
            },
            initialized =>
            {
                var released = _mediaSuspended;
                try
                {
                    try
                    {
                        if (initialized && !_mediaSuspended) TryReleaseInput();
                        if (initialized && !_mediaSuspended) _core.Stop();
                    }
                    catch (Exception error) { ReportFailure(error); }
                    finally
                    {
                        if (!released) released = CommonCoreHostFunctions.MachineExecution.TryDisposeCore(_core, ReportFailure);
                    }
                }
                finally
                {
                    try { _audio.Stop(); }
                    finally
                    {
                        try { if (released) DeleteSessionDirectory(); }
                        finally
                        {
                            lock (_gate)
                            {
                                FailPendingCommands(new OperationCanceledException(ErrorMessages.MachineStopped));
                                if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
                            }
                        }
                    }
                }
            });
    }

    /// <summary>Contextualise une erreur et fait échouer le démarrage ainsi que les commandes en attente.</summary>
    /// <param name="error">Erreur de fonctionnement ou de libération à conserver comme origine du diagnostic.</param>
    /// <remarks>Le traducteur du backend peut préciser le diagnostic de démarrage. Une demande de média
    /// conserve sa catégorie Warning et ses lecteurs attendus. Les diagnostics du cœur sont ajoutés si le
    /// cœur est encore consultable ; une erreur de cette consultation ne remplace pas l’erreur originale.</remarks>
    private void ReportFailure(Exception error)
    {
        error = error is CoreMediaRequiredException ? RequiredMediaWarning(CurrentConfiguration(), error)
            : _startErrorTranslator?.Invoke(error) ?? MessageFunctions.Translate(error, Configuration);
        if (!_mediaSuspended)
        {
            try
            {
                if (_core.Diagnostics.Count > MachineConstants.EmptyCount)
                    error.Data[MachineConstants.DiagnosticDataKey] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(MachineConstants.DiagnosticTailCount));
            }
            catch { }
        }
        _started?.TrySetException(error);
        lock (_gate)
        {
            State = EmulationMachineState.Faulted;
            FailPendingCommands(error);
        }
    }

    /// <summary>Construit l’avertissement demandant les médias des lecteurs configurés.</summary>
    /// <param name="configuration">Configuration donnant les lecteurs activés et leurs libellés.</param>
    /// <param name="error">Erreur du cœur conservée comme origine de l’avertissement.</param>
    /// <returns>Diagnostic de média requis, avec catégories et noms des lecteurs concernés.</returns>
    internal static EmulationMessageException RequiredMediaWarning(MachineConfiguration configuration, Exception error)
    {
        var storage = StorageSettingsFunctions.Describe(configuration);
        var devices = storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot)).ToArray();
        return new EmulationMessageException(new EmulationMessage(EmulationMessageCategory.Media, EmulationMessageCode.RequiredMediaMissing,
            EmulationMessageSeverity.Warning, EmulationMessageTarget.Dialog,
            new EmulationRequiredMachineMediaMessageContext(configuration.MachineId, devices.Select(device => device.Slot.Category).Distinct().ToArray(),
                devices.Select(device => device.DisplayLabel ?? device.Slot.ToString()).ToArray())), error);
    }

    /// <summary>Démarre la sortie avec la fréquence et la latence minimale annoncées par le cœur.</summary>
    /// <remarks>Le premier démarrage est stéréo ; chaque bloc audio porte ensuite sa disposition exacte,
    /// interprétée par le même contrôleur commun pour les sorties multicanales.</remarks>
    private void StartAudio()
    {
        if (_core is IEmulatorAudioBufferFeedback latency)
            _audio.TrySetMinimumLatency(latency.MinimumAudioLatencyMilliseconds);
        _audio.Start(_core.SampleRate);
    }

    /// <summary>Publie les nouvelles images et vide la file audio produite par le cœur.</summary>
    /// <param name="videoSequence">Dernière séquence vidéo publiée, mise à jour seulement pour une nouvelle image.</param>
    /// <remarks>Les blocs audio passent par le contrôleur commun puis AudioChunkReady. Les abonnés reçoivent
    /// les buffers selon leur durée de validité contractuelle ; cette publication n’en prend pas possession.</remarks>
    private void PublishOutputs(ref long videoSequence) => CommonCoreHostFunctions.MachineExecution.PublishOutputs(this, ref videoSequence,
        static machine => machine._core.LatestVideoFrame,
        static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
        static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
        static (machine, audio) =>
        {
            machine._audio.Write(audio);
            machine.AudioChunkReady?.Invoke(machine, audio);
        });

    /// <summary>Relâche les entrées et remet à zéro la commande du pointeur par contrôleur.</summary>
    /// <remarks>Cette opération de fin de session tolère un refus du cœur ; elle ne détruit aucun périphérique reçu.</remarks>
    private void TryReleaseInput()
    {
        _controllerPointerSwitchPressed = false;
        _controllerPointerMode = false;
        _cassetteInput?.SetPhysicalInput(EmulationInputSnapshot.Empty);
        if (!_mediaSuspended) MachineFunctions.TryReleaseInput(_core);
    }

    /// <summary>Termine en échec les commandes que la boucle ne peut plus exécuter.</summary>
    /// <param name="error">Erreur transmise aux appelants attendant leur commande.</param>
    private void FailPendingCommands(Exception error) => CommonCoreHostFunctions.MachineExecution.FailPendingCommands(_commands, error);

    /// <summary>Réinitialise le cœur après vidage de la sortie audio.</summary>
    /// <param name="hardReset">True demande une réinitialisation matérielle ; false utilise la commande logicielle du cœur.</param>
    /// <remarks>Un backend exigeant une recréation utilise la configuration courante. Une session suspendue
    /// pour média n’a plus de cœur actif à réinitialiser ; elle reste suspendue jusqu’à la prochaine insertion.</remarks>
    private void ResetCore(bool hardReset = true)
    {
        if (_mediaSuspended) return;
        _audio.Reset();
        if (_requiresResetRestart) RestartCore(CurrentConfiguration());
        else if (hardReset) _core.HardReset();
        else _core.SoftReset();
    }

    /// <summary>Consulte le callback explicite puis la politique du backend pour le remplacement d’un média.</summary>
    /// <param name="slot">Lecteur logique visé.</param>
    /// <param name="path">Chemin du prochain média, ou null pour une éjection.</param>
    /// <returns>True lorsque cette modification exige un nouveau cœur.</returns>
    /// <remarks>Un callback fourni conserve sa priorité. En son absence, la politique reçoit la configuration ; sans décision disponible, la méthode retourne false.</remarks>
    private bool NeedsMediaRestart(EmulationMediaSlot slot, string? path = null) =>
        _requiresMediaRestart?.Invoke(slot, path) ?? _runtimePolicy?.RequiresMediaRestart(Configuration, slot, path) ?? false;

    /// <summary>Arrête et libère le cœur en attendant un média nécessaire à sa recréation.</summary>
    /// <remarks>L’état de pause antérieur est mémorisé. La sortie audio est arrêtée, le cœur est détruit dans
    /// finally, puis la machine reste Paused pour recevoir l’insertion suivante sur sa file de commandes.</remarks>
    private void SuspendForMedia()
    {
        if (_mediaSuspended) return;
        _resumeAfterMediaInsert = !_pauseRequested;
        _audio.Stop();
        try { _core.Stop(); }
        finally
        {
            _core.Dispose();
            _mediaSuspended = true;
        }
        lock (_gate)
        {
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
        }
    }

    /// <summary>Remplace le cœur avec les médias et réglages courants de la machine.</summary>
    /// <param name="configuration">Configuration validée à appliquer au cœur de remplacement.</param>
    /// <exception cref="NotSupportedException">Le backend n’a pas fourni de fabrique permettant cette recréation.</exception>
    /// <remarks>Le cœur précédent est libéré avant la création du suivant. Si son initialisation échoue,
    /// le remplaçant est libéré dans finally. La session reprend seulement si la suspension pour média avait
    /// interrompu une machine en marche ; une pause demandée par l’utilisateur reste une pause.</remarks>
    private void RestartCore(MachineConfiguration configuration)
    {
        ConfigurationFunctions.Validate(configuration);
        var factory = _coreFactory ?? throw new NotSupportedException(ErrorMessages.DynamicMediaUnsupported);
        _audio.Stop();
        var resumeAfterInsert = _mediaSuspended && _resumeAfterMediaInsert;
        if (!_mediaSuspended)
        {
            try { _core.Stop(); }
            finally
            {
                _core.Dispose();
                _mediaSuspended = true;
            }
        }
        var replacement = factory();
        _core = replacement;
        _mediaSuspended = false;
        var completed = false;
        try
        {
            replacement.Initialize(configuration, _sessionDirectory, _saveDirectory);
            if (_pauseRequested) replacement.SetInput(EmulationInputSnapshot.Empty);
            else
            {
                _input.Poll();
                replacement.SetInput(InputSnapshotFunctions.Apply(_input.Polled, configuration, _inputMapper, _controllerPointerSwitchPressed));
            }
            StartAudio();
            if (_pauseRequested) _audio.Pause();
            if (resumeAfterInsert)
            {
                lock (_gate)
                {
                    _pauseRequested = false;
                    State = EmulationMachineState.Running;
                }
                _audio.Resume();
            }
            _resumeAfterMediaInsert = false;
            completed = true;
        }
        finally
        {
            if (!completed)
            {
                replacement.Dispose();
                _mediaSuspended = true;
            }
        }
    }

    /// <summary>Refuse l’utilisation d’une machine déjà libérée.</summary>
    /// <exception cref="ObjectDisposedException">DisposeAsync a terminé son parcours de libération.</exception>
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>Supprime le dossier de session après libération effective du cœur.</summary>
    /// <remarks>Le callback du backend peut appliquer sa règle de nettoyage de session. Sans callback,
    /// le nettoyage commun conserve la vérification du nom GUID ; les médias et sauvegardes externes restent empruntés.</remarks>
    private void DeleteSessionDirectory()
    {
        if (_deleteSession is not null) _deleteSession(_sessionDirectory);
        else MachineFunctions.DeleteSessionDirectory(_sessionDirectory);
    }

    /// <summary>Arrête la machine et libère toutes ses ressources possédées, même avant son premier démarrage.</summary>
    /// <returns>Attente de la fin effective du thread et des libérations.</returns>
    /// <remarks>Après lancement, la boucle possède le cœur et le nettoyage de session. Avant lancement,
    /// ce parcours libère le cœur directement. Le dossier de session n’est supprimé qu’après réussite de cette
    /// libération ; une erreur ne permet pas de retirer les fichiers d’un cœur qui pourrait encore les utiliser.</remarks>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        var disposeUnstartedCore = _thread is null;
        var released = _mediaSuspended;
        try
        {
            await CommonCoreHostFunctions.MachineExecution.DisposeMachineAsync(() => StopAsync(),
                () =>
                {
                    if (disposeUnstartedCore && !released)
                    {
                        _core.Dispose();
                        released = true;
                    }
                },
                () => _audio.Dispose(), () => _stopSource?.Dispose(),
                () =>
                {
                    _disposed = true;
                    if (disposeUnstartedCore && released) DeleteSessionDirectory();
                }).ConfigureAwait(false);
        }
        finally { _disposed = true; }
    }
}
