using CommonControllerFunctions = GWGUI.Emulation.Example.Common.Functions.ControllerFunctions;
using GWGUI.Emulation.Example.Common.Functions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;
using System.IO;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Example.Common.Constants;
using GWGUI.Emulation.Example.Common.Enums;
using GWGUI.Emulation.Example.Common.Exceptions;
using GWGUI.Emulation.Example.Common.Machines.Common.Constants;
using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Example.Common.Machines.Common.Enums;
using GWGUI.Emulation.Example.Common.Machines.Common.Functions;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Partie de la machine exposant les opérations de cycle de vie, d’entrées, de médias et d’états sauvegardés.</summary>
/// <remarks>Les opérations touchant au cœur passent par QueueCommand. Les changements d’état sont protégés par le verrou
/// de la machine ; l’arrêt attend la fin effective du thread, y compris lorsque l’attente appelante est annulée.</remarks>
internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput, IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    /// <summary>Lance le thread du cœur et attend que son initialisation soit terminée.</summary>
    /// <param name="cancellationToken">Annulation du démarrage ; le traitement commun demande alors l’arrêt.</param>
    /// <returns>Attente du démarrage réussi ou de son erreur.</returns>
    /// <exception cref="ObjectDisposedException">La machine a été détruite.</exception>
    /// <exception cref="InvalidOperationException">La machine n’est plus dans l’état Created.</exception>
    /// <remarks>Un échec de lancement du thread place la machine en Faulted ; un thread lancé possède le nettoyage du cœur.</remarks>
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Task started;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (State != EmulationMachineState.Created)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
            State = EmulationMachineState.Starting;
            var threadStarted = false;
            try
            {
                EmulationSdk.MachineExecution.StartThread(
                    Run, MachineFunctions.ThreadName(Id, Configuration.Core), cancellationToken,
                    ref _stopSource, out _started, out _runLoop, ref _thread);
                threadStarted = true;
                started = _started.Task;
            }
            finally
            {
                if (!threadStarted) State = EmulationMachineState.Faulted;
            }
        }
        await EmulationSdk.MachineExecution.WaitForStartOrStopAsync(
            started, cancellationToken, () => StopAsync()).ConfigureAwait(false);
    }

    /// <summary>Demande la pause de la boucle, suspend la sortie audio et libère les entrées maintenues.</summary>
    /// <param name="cancellationToken">Annulation de la demande et de la commande de libération des entrées.</param>
    /// <returns>Attente de la commande d’entrées ; attente déjà achevée si la machine n’était pas Running.</returns>
    public ValueTask PauseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Running) return ValueTask.CompletedTask;
            _pauseRequested = true;
            State = EmulationMachineState.Paused;
            _audio.Pause();
        }
        return QueueCommand(() => { if (!_mediaSuspended) MachineFunctions.ReleaseInput(_core); }, cancellationToken);
    }

    /// <summary>Réactive la boucle et l’audio d’une machine en pause.</summary>
    /// <param name="cancellationToken">Annulation vérifiée avant le changement d’état.</param>
    /// <returns>Attente déjà achevée après signalement de la reprise, ou si la machine n’était pas Paused.</returns>
    /// <remarks>Une machine suspendue pour changement de média attend une insertion permettant de recréer son cœur.</remarks>
    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Paused || _mediaSuspended) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            _audio.Resume();
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    /// <summary>Demande la réinitialisation logicielle du cœur après vidage audio.</summary>
    /// <param name="cancellationToken">Annulation de la commande.</param>
    /// <returns>Attente de SoftReset, ou de la recréation exigée par le backend, sur le thread du cœur.</returns>
    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(hardReset: false), cancellationToken);

    /// <summary>Demande la réinitialisation matérielle après vidage de la sortie audio.</summary>
    /// <param name="cancellationToken">Annulation de la commande.</param>
    /// <returns>Attente de l’exécution sur le thread du cœur.</returns>
    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(), cancellationToken);

    /// <summary>Demande l’arrêt et attend la fin effective de la boucle et de son thread.</summary>
    /// <param name="cancellationToken">Annulation de l’attente principale ; le finally attend néanmoins la fermeture effective du thread.</param>
    /// <returns>Attente de l’arrêt et du nettoyage réalisés par la boucle.</returns>
    /// <remarks>Une machine jamais démarrée reste Created ; son cœur sera détruit par DisposeAsync.
    /// Une pause est levée pour permettre à la boucle de terminer. Le statut Faulted est conservé.</remarks>
    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop = null;
        try
        {
            lock (_gate)
            {
                if (State == EmulationMachineState.Created) return;
                loop = _runLoop;
                if (State is not EmulationMachineState.Faulted and not EmulationMachineState.Stopped)
                {
                    State = EmulationMachineState.Stopping;
                    try { _stopSource?.Cancel(); }
                    finally
                    {
                        _pauseRequested = false;
                        Monitor.PulseAll(_gate);
                    }
                }
            }
            if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await EmulationSdk.MachineExecution.WaitForThreadAsync(loop, _thread).ConfigureAwait(false);
        }
    }

    /// <summary>Accumule les entrées physiques pour leur projection sur le thread du cœur.</summary>
    /// <param name="snapshot">Instantané emprunté ; le service commun copie ses collections et accumule ses déplacements relatifs.</param>
    /// <remarks>La projection est effectuée une seule fois, lors de la frame. Elle utilise alors les associations
    /// courantes et la commande de bascule du pointeur, sans appeler le cœur depuis le thread de l’interface graphique.</remarks>
    public void SetInput(EmulationInputSnapshot snapshot)
    {
        ThrowIfDisposed();
        _input.Update(snapshot);
    }

    /// <summary>Change le périphérique d’un port sur le thread du cœur et attend synchroniquement sa commande.</summary>
    /// <param name="port">Index du port attendu par le cœur.</param>
    /// <param name="peripheral">Catégorie interne du périphérique à connecter.</param>
    /// <remarks>Les autres ports et les associations du port concerné sont conservés. Pendant une suspension
    /// pour média, la nouvelle sélection est enregistrée pour le cœur qui sera recréé à l’insertion suivante.</remarks>
    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) => QueueCommand(() =>
    {
        var updated = CommonControllerFunctions.WithPeripheral(CurrentConfiguration(), port, peripheral);
        if (!_mediaSuspended) _core.SetControllerPortDevice(port, peripheral);
        _inputConfiguration = updated.Input;
        Configuration = updated;
    }, CancellationToken.None).AsTask().GetAwaiter().GetResult();

    /// <inheritdoc/>
    /// <remarks>Émet l’impulsion attendue par les mappers de contrôleur/pointeur. Le relâchement est garanti
    /// dans finally, y compris lorsque l’attente est annulée. Les frames lisent cette commande avec les entrées
    /// accumulées ; aucun déplacement relatif n’est ajouté une seconde fois pour provoquer la bascule.</remarks>
    async ValueTask<bool> IEmulationInput.SwitchControllerPointerAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_supportsPointerCapture) return false;
        lock (_gate)
        {
            ThrowIfDisposed();
            _controllerPointerSwitchPressed = true;
        }
        var completed = false;
        try
        {
            await Task.Delay(MachineConstants.ControllerPointerPulseMilliseconds, cancellationToken).ConfigureAwait(false);
            completed = true;
        }
        finally
        {
            lock (_gate)
            {
                _controllerPointerSwitchPressed = false;
                if (completed) _controllerPointerMode = !_controllerPointerMode;
            }
        }
        return _controllerPointerMode;
    }

    /// <summary>Transmet une commande au contrôleur cassette de la session sur le thread du cœur.</summary>
    /// <param name="command">Commande déclarée disponible par la facette de transport.</param>
    /// <param name="cancellationToken">Annulation de la commande en file.</param>
    /// <returns>Attente de l’enregistrement de l’impulsion cassette ; la frame suivante la consomme.</returns>
    /// <exception cref="NotSupportedException">Le contrôleur est absent ou ne propose pas cette commande.</exception>
    ValueTask IEmulationCassetteTransport.ExecuteAsync(EmulationCassetteCommand command, CancellationToken cancellationToken) => QueueCommand(() =>
    {
        if (_cassetteInput is null || command != EmulationCassetteCommand.Play) throw new NotSupportedException(command.ToString());
        _cassetteInput.Play();
    }, cancellationToken, EmulationMessageCategory.Media, EmulationMessageCode.MediaOperationFailed);

    /// <summary>Coupe ou réactive la sortie audio de la machine.</summary>
    /// <param name="muted">Vrai pour couper le son, faux pour l’autoriser.</param>
    public void SetAudioMuted(bool muted)
    {
        _audio.SetMuted(muted);
    }

    /// <summary>Applique un facteur de volume au contrôleur de sortie.</summary>
    /// <param name="volume">Facteur normalisé par les règles du contrôleur audio.</param>
    public void SetAudioVolume(float volume) => _audio.SetVolume(volume);

    /// <summary>Remplace la fabrique de sorties audio utilisée par le contrôleur.</summary>
    /// <param name="factory">Fabrique facultative ; les sorties qu’elle crée appartiennent au contrôleur.</param>
    public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) => _audio.ReplaceFactory(factory);

    /// <summary>Insère un média dans le cœur puis met à jour la liste montée.</summary>
    /// <param name="media">Description du média ; la commande force son indicateur IsInserted à true.</param>
    /// <param name="cancellationToken">Annulation de la commande.</param>
    /// <returns>Attente de l’insertion ; la liste n’est modifiée qu’après réussite du cœur.</returns>
    /// <remarks>La règle du backend détermine si le cœur peut monter directement le média ou doit être
    /// recréé avec les montages courants. Une session suspendue pour média est toujours recréée.</remarks>
    public ValueTask InsertMediaAsync(MediaConfiguration media, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var inserted = media with { IsInserted = true };
            if (_mediaSuspended || NeedsMediaRestart(inserted.Slot, inserted.Path))
                RestartCore(CurrentConfiguration() with { Media = _mountedMedia.Where(item => item.Slot != inserted.Slot).Append(inserted).ToArray() });
            else
                _core.InsertMedia(inserted);
            MediaRuntimeFunctions.Register(_mountedMedia, inserted);
        }, cancellationToken, EmulationMessageCategory.Media, EmulationMessageCode.MediaOperationFailed);

    /// <summary>Éjecte le contenu d’un emplacement en conservant sa description pour une réinsertion.</summary>
    /// <param name="slot">Emplacement à vider.</param>
    /// <param name="cancellationToken">Annulation de la commande.</param>
    /// <returns>Attente de l’éjection et de la mise à jour du montage.</returns>
    /// <remarks>Le chemin et les réglages restent disponibles avec IsInserted à false. Lorsqu’un nouveau
    /// cœur est nécessaire, les autres médias insérés ou la capacité sans contenu permettent sa recréation ;
    /// sinon, la session reste suspendue en attendant une insertion.</remarks>
    public ValueTask EjectMediaAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!_mountedMedia.Any(item => item.Slot == slot && item.IsInserted)) return;
            if (!_mediaSuspended)
            {
                if (NeedsMediaRestart(slot))
                {
                    var remaining = _mountedMedia.Select(item => item.Slot == slot ? item with { IsInserted = false } : item).ToArray();
                    if (_supportsNoContent || remaining.Any(item => item.IsInserted)) RestartCore(CurrentConfiguration() with { Media = remaining });
                    else SuspendForMedia();
                }
                else _core.EjectMedia(slot);
            }
            MediaRuntimeFunctions.MarkEjected(_mountedMedia, slot);
        }, cancellationToken, EmulationMessageCategory.Media, EmulationMessageCode.MediaOperationFailed);

    /// <summary>Demande au cœur de sélectionner un disque par son index.</summary>
    /// <param name="index">Index transmis sans modification au cœur.</param>
    /// <param name="cancellationToken">Annulation de la commande.</param>
    /// <returns>Attente de la sélection sur le thread du cœur.</returns>
    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) => QueueCommand(() =>
    {
        if (_mediaSuspended) throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
        _core.SelectDisk(index);
    }, cancellationToken, EmulationMessageCategory.Media, EmulationMessageCode.MediaOperationFailed);

    /// <summary>Capture l’état du cœur et l’écrit avec les empreintes permettant sa validation ultérieure.</summary>
    /// <param name="path">Chemin du fichier d’état à écrire.</param>
    /// <param name="cancellationToken">Annulation de la commande avant son exécution.</param>
    /// <returns>Attente de la capture et de l’écriture sur le thread du cœur.</returns>
    /// <exception cref="EmulationException">Le cœur n’annonce pas de capacité de sauvegarde d’état.</exception>
    /// <remarks>L’en-tête inclut version de format, identité du cœur et du modèle, empreintes du cœur, de la configuration,
    /// des contenus et des octets d’état. Le traitement de fichier utilise le format commun.</remarks>
    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) => QueueCommand(() =>
    {
        if (!SupportsSaveStates)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
        var configuration = StateConfiguration();
        var state = _core.SaveState();
        var header = new EmulationSavedStateHeader<MachineModel, Emulator>(
            EmulationSavedStateConstants.CurrentFormatVersion, _core.Emulator, _core.CoreName, _core.CoreVersion,
            _core.CoreSha256, configuration.Model, EmulationSdk.StateFiles.HashDocument(configuration),
            StateContentHash(configuration), EmulationSdk.StateFiles.HashBytes(state));
        EmulationSdk.StateFiles.WriteState(path, header, state);
    }, cancellationToken, EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateOperationFailed);

    /// <summary>Lit et valide un état avant de le restaurer dans le cœur.</summary>
    /// <param name="path">Chemin du fichier d’état à lire.</param>
    /// <param name="cancellationToken">Annulation de la commande avant son exécution.</param>
    /// <returns>Attente de la lecture, du contrôle de compatibilité et de la restauration.</returns>
    /// <exception cref="EmulationException">Les états sont indisponibles, le fichier est invalide ou son identité est incompatible.</exception>
    /// <remarks>Le cœur reçoit les octets uniquement après vérification de l’en-tête et des empreintes des contenus courants.</remarks>
    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) => QueueCommand(() =>
    {
        if (!SupportsSaveStates)
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
        var saved = EmulationSdk.StateFiles.ReadState<MachineModel, Emulator>(path,
            static () => new EmulationException(ErrorCategory.State, ErrorCode.StateInvalid, ErrorMessages.StateInvalid));
        var configuration = StateConfiguration();
        if (!EmulationSdk.StateFiles.IsCompatible(saved.Header, configuration.Model, _core.Emulator, _core,
                () => EmulationSdk.StateFiles.HashDocument(configuration), () => StateContentHash(configuration)))
            throw new EmulationException(ErrorCategory.State, ErrorCode.StateIncompatible, ErrorMessages.StateInvalid);
        _core.LoadState(saved.State);
    }, cancellationToken, EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateOperationFailed);

    /// <summary>Construit la configuration décrivant réellement la session au moment d’une sauvegarde ou restauration.</summary>
    /// <returns>Copie avec identifiant d’instance neutralisé, entrées et médias courants et valeurs effectives des options.</returns>
    /// <remarks>Les options de configuration qui ne sont pas annoncées par le cœur restent présentes.
    /// Les options natives annoncées remplacent seulement leurs propres valeurs par celles du runtime.</remarks>
    private MachineConfiguration StateConfiguration()
    {
        var options = new Dictionary<string, string>(Configuration.Options, StringComparer.Ordinal);
        foreach (var option in _core.Options) options[option.Key] = option.CurrentValue;
        return CurrentConfiguration() with { Id = Guid.Empty, Options = options };
    }

    /// <summary>Calcule une empreinte déterministe des firmwares et médias insérés.</summary>
    /// <param name="configuration">Configuration de session dont les contenus doivent être identifiés.</param>
    /// <returns>Empreinte du document de contenus ordonné par catégorie de firmware, puis slot et chemin de média.</returns>
    private static string StateContentHash(MachineConfiguration configuration) => EmulationSdk.StateFiles.HashDocument(
        configuration.Firmwares.OrderBy(firmware => firmware.FieldId, StringComparer.Ordinal).ThenBy(firmware => firmware.Category)
            .Select(firmware => new EmulationStateContentEntry("firmware", firmware.FieldId ?? firmware.Category.ToString(), HashStatePath(firmware.Path)))
            .Concat(configuration.Media.Where(media => media.IsInserted).OrderBy(media => media.Slot)
                .ThenBy(media => media.Path, StringComparer.OrdinalIgnoreCase)
                .Select(media => new EmulationStateContentEntry("media", media.Category + ":" + media.Slot,
                    HashStatePath(media.Path)))).ToArray());

    /// <summary>Calcule l’empreinte d’un fichier ou d’une arborescence de contenu avec le lecteur commun.</summary>
    /// <param name="path">Chemin du contenu à identifier.</param>
    /// <returns>Empreinte utilisant tous les fichiers, le séparateur canonique et le tampon de transfert du module.</returns>
    /// <exception cref="FileNotFoundException">Le contenu attendu n’est pas disponible.</exception>
    private static string HashStatePath(string path) => EmulationSdk.StateFiles.HashPath(path, "*", ConfigurationStoreConstants.StoredDirectorySeparator, BufferConstants.FileTransferBufferSize,
        () => new FileNotFoundException(ErrorMessages.StateInvalid, path));

    /// <summary>Transmet une nouvelle valeur d’option au cœur sur son thread.</summary>
    /// <param name="key">Identifiant exact de l’option.</param>
    /// <param name="value">Valeur textuelle attendue par le cœur.</param>
    /// <param name="cancellationToken">Annulation de la commande.</param>
    /// <returns>Attente de l’application ; cette opération ne réécrit pas le document persistant de configuration.</returns>
    /// <remarks>Les options exigeant un redémarrage passent par la même recréation de cœur que les médias.
    /// Pendant une suspension pour média, la valeur est conservée pour la prochaine initialisation.
    /// La configuration courante n’est remplacée qu’après validation et réussite de l’opération du cœur.</remarks>
    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) => QueueCommand(() =>
    {
        var options = new Dictionary<string, string>(Configuration.Options, StringComparer.Ordinal) { [key] = value };
        var updated = CurrentConfiguration() with { Options = options };
        ConfigurationFunctions.Validate(updated);
        if (!_mediaSuspended)
        {
            if (_runtimePolicy?.RequiresOptionRestart(key) == true || RuntimeOptionFunctions.RequiresRestart(Configuration.Core, key)) RestartCore(updated);
            else _core.SetOption(key, value);
        }
        Configuration = updated;
    }, cancellationToken, EmulationMessageCategory.Machine, EmulationMessageCode.OptionInvalid);


}
