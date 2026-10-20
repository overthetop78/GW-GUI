using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        Task started;
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (State != EmulationMachineState.Created)
                throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
            State = EmulationMachineState.Starting;
            _stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _thread = new Thread(() =>
            {
                try { Run(_stopSource.Token); }
                finally { completed.TrySetResult(); }
            })
            {
                IsBackground = true,
                Name = MachineFunctions.ThreadName(Id, Configuration.Core)
            };
            _runLoop = completed.Task;
            try { _thread.Start(); }
            catch
            {
                _thread = null;
                completed.TrySetResult();
                _stopSource.Dispose();
                _stopSource = null;
                State = EmulationMachineState.Faulted;
                throw;
            }
            started = _started.Task;
        }
        await GWGUI.Emulation.Functions.ExternalCoreLifecycleFunctions.WaitForStartOrStopAsync(
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
            if (State != EmulationMachineState.Paused || _mediaSuspended) return ValueTask.CompletedTask;
            _pauseRequested = false;
            State = EmulationMachineState.Running;
            _audio.Resume();
            Monitor.PulseAll(_gate);
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask SoftResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(), cancellationToken);

    public ValueTask HardResetAsync(CancellationToken cancellationToken = default) => QueueCommand(() => ResetCore(), cancellationToken);

    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        Task? loop;
        lock (_gate)
        {
            if (State == EmulationMachineState.Created) return;
            loop = _runLoop;
            if (State is not EmulationMachineState.Faulted and not EmulationMachineState.Stopped)
            {
                State = EmulationMachineState.Stopping;
                _stopSource?.Cancel();
                _pauseRequested = false;
                Monitor.PulseAll(_gate);
            }
        }
        try
        {
            if (loop is not null) await loop.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await ExternalCoreLifecycleFunctions.WaitForThreadAsync(loop, _thread).ConfigureAwait(false);
        }
    }

    public void SetInput(EmulationInputSnapshot snapshot) => _input.Update(InputSnapshotFunctions.Apply(snapshot, _inputConfiguration, Configuration.Model, _inputMapper));

    public void SetControllerPortDevice(int port, PeripheralCategory peripheral) =>
        QueueCommand(() =>
        {
            if (port < ControllerConstants.FirstPort || port >= ConfigurationFunctions.ControllerPortCount(Configuration.Model) || !InputSettingsFunctions.Peripherals(Configuration.Model, Configuration.Core).Contains(peripheral))
                throw new ArgumentException(ErrorMessages.UnsupportedControllerDevice, nameof(peripheral));
            var previous = _inputConfiguration.Controllers?.FirstOrDefault(binding => binding.Port == port);
            var binding = previous is null ? new ControllerBinding(port, peripheral) : previous with { Peripheral = peripheral };
            if (!_mediaSuspended) _core.SetControllerPortDevice(port, peripheral);
            _inputConfiguration = _inputConfiguration with { Controllers = (_inputConfiguration.Controllers ?? []).Where(item => item.Port != port).Append(binding).ToArray() };
        }, CancellationToken.None)
            .AsTask().GetAwaiter().GetResult();

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
            if (NeedsMediaRestart(inserted.Slot, inserted.Path))
                RestartCore(CurrentConfiguration() with
                {
                    Media = _mountedMedia.Where(item => item.Slot != inserted.Slot).Append(inserted).ToArray()
                });
            else _core.InsertMedia(inserted);
            MediaRuntimeFunctions.Register(_mountedMedia, inserted);
        }, cancellationToken);

    public ValueTask EjectMediaAsync(EmulationMediaSlot slot, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!_mountedMedia.Any(item => item.Slot == slot)) return;
            if (NeedsMediaRestart(slot))
            {
                if (Configuration.Family == MachineFamily.P2000)
                {
                    RestartCore(CurrentConfiguration() with
                    {
                        Media = _mountedMedia.Where(item => item.Slot != slot).ToArray()
                    });
                    _mountedMedia.RemoveAll(item => item.Slot == slot);
                    return;
                }
                SuspendForMedia();
                _mountedMedia.RemoveAll(item => item.Slot == slot);
                return;
            }
            _core.EjectMedia(slot);
            _mountedMedia.RemoveAll(item => item.Slot == slot);
        }, cancellationToken);

    public ValueTask SelectDiskAsync(int index, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SelectDisk(index), cancellationToken);

    public ValueTask SaveStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!SupportsSaveStates)
                throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
            var state = _core.SaveState();
            var header = SavedStateFunctions.CreateHeader(CurrentConfiguration(), _core, state);
            StateFileFunctions.Write(path, header, state);
        }, cancellationToken);

    public ValueTask LoadStateAsync(string path, CancellationToken cancellationToken = default) =>
        QueueCommand(() =>
        {
            if (!SupportsSaveStates)
                throw SavedStateFunctions.Invalid(ErrorCode.StateInvalid, ErrorMessages.StateUnavailable);
            var saved = StateFileFunctions.Read(path);
            SavedStateFunctions.Validate(saved.Header, CurrentConfiguration(), _core);
            _core.LoadState(saved.State);
        }, cancellationToken);

    public ValueTask SetOptionAsync(string key, string value, CancellationToken cancellationToken = default) => QueueCommand(() => _core.SetOption(key, value), cancellationToken);


}
