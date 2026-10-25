using CommonCoreHostFunctions = GWGUI.Emulation.Apple.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Interop;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Apple.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    /// <summary>Consulte le jeu de disques courant par la file de commandes du cœur.</summary>
    /// <param name="cancellationToken">Annulation de la commande en attente.</param>
    /// <returns>Instantané partagé du nombre et de la sélection des images, de leur éjection, de leurs chemins et libellés.</returns>
    /// <remarks>La consultation suit les mêmes règles de session active que les autres commandes.
    /// Elle ne monte ni ne retire de média. Les données retournées sont des valeurs gérées
    /// et ne transfèrent aucune ressource native du cœur au consommateur.</remarks>
    public async ValueTask<DiskStatus> GetDiskStatusAsync(CancellationToken cancellationToken = default)
    {
        DiskStatus? status = null;
        await QueueCommand(() => status = _core.GetDiskStatus(), cancellationToken).ConfigureAwait(false);
        // La file ne termine avec succès que lorsque la consultation a effectivement renseigné le résultat.
        return status!;
    }

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
                CommonCoreHostFunctions.MachineExecution.StartThread(
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
        await CommonCoreHostFunctions.MachineExecution.WaitForStartOrStopAsync(
            started, cancellationToken, () => StopAsync()).ConfigureAwait(false);
    }

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
        return QueueCommand(() => MachineFunctions.ReleaseInput(_core), cancellationToken);
    }

    public ValueTask ResumeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (State != EmulationMachineState.Paused) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            _audio.Resume();
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(hardReset: false), cancellationToken);

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(), cancellationToken);

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
            await CommonCoreHostFunctions.MachineExecution.WaitForThreadAsync(loop, _thread).ConfigureAwait(false);
        }
    }

    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(MapInput(snapshot));

    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) => QueueCommand(() =>
    {
        _core.SetControllerPortDevice(port, peripheral);
        var previous = _inputConfiguration.Input.Controllers?.FirstOrDefault(binding => binding.Port == port);
        var binding = previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral };
        _inputConfiguration = _inputConfiguration with { Input = _inputConfiguration.Input with { Controllers = (_inputConfiguration.Input.Controllers ?? []).Where(item => item.Port != port).Append(binding).ToArray() } };
    }, CancellationToken.None).AsTask().GetAwaiter().GetResult();

    ValueTask<bool> IEmulationInput.SwitchControllerPointerAsync(CancellationToken cancellationToken) => ValueTask.FromResult(false);

    public void SetAudioMuted(bool muted)
    {
        _audio.SetMuted(muted);
    }

    public void SetAudioVolume(float volume) => _audio.SetVolume(volume);

    public void SetAudioOutputFactory(Func<IAudioOutput?>? factory) => _audio.ReplaceFactory(factory);

    public ValueTask InsertMediaAsync(MediaConfiguration media, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            var inserted = media with { IsInserted = true };
            var replaces = _runtimePolicy?.ReplacesMountedMediaOnInsert(Configuration) == true;
            if (NeedsMediaRestart(inserted.Slot, inserted.Path))
                RestartCore(CurrentConfiguration() with
                {
                    Media = (replaces ? [] : _mountedMedia.Where(item => item.Slot != inserted.Slot)).Append(inserted).ToArray()
                });
            else _core.InsertMedia(inserted);
            if (replaces) _mountedMedia.Clear();
            MediaRuntimeFunctions.Register(_mountedMedia, inserted);
        }, cancellationToken);

    public ValueTask EjectMediaAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (NeedsMediaRestart(slot))
                RestartCore(CurrentConfiguration() with
                {
                    Media = _mountedMedia.Where(item => item.Slot != slot).ToArray()
                });
            else _core.EjectMedia(slot);
            _mountedMedia.RemoveAll(item => item.Slot == slot);
        }, cancellationToken);

    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SelectDisk(index), cancellationToken);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!_core.SupportsSaveStates)
                throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
            var state = _core.SaveState();
            var header = SavedStateFunctions.CreateHeader(CurrentConfiguration(), _core, state);
            StateStore.Write(path, header, state);
        }, cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!_core.SupportsSaveStates)
                throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
            var saved = StateStore.Read(path);
            SavedStateFunctions.Validate(saved.Header, CurrentConfiguration(), _core);
            _core.LoadState(saved.State);
        }, cancellationToken);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SetOption(key, value), cancellationToken);


    /// <inheritdoc/>
    /// <remarks>La commande rejoint la file du cœur ; les politiques de copie, de protection et de
    /// sauvegarde automatique restent celles du backend. Aucun autre lecteur n’est sélectionné.</remarks>
    public ValueTask SaveChangesAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SaveMediaChanges(slot), cancellationToken);

    /// <inheritdoc/>
    /// <remarks>Le résultat est capturé sur le même thread que les frames et les changements de médias.
    /// La consultation ne publie aucun fichier et ne modifie aucun montage.</remarks>
    public async ValueTask<bool> HasUnsavedChangesAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default)
    {
        var hasChanges = false;
        await QueueCommand(() => hasChanges = _core.HasUnsavedMediaChanges(slot), cancellationToken).ConfigureAwait(false);
        return hasChanges;
    }
}
