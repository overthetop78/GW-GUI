using CommonCoreHostFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreHostFunctions;
using AudioFunctions = GWGUI.Emulation.Nec.Common.Functions.AudioFunctions;
using GWGUI.Emulation;
using GWGUI.Emulation.Nec.Common.Exceptions;
using System.Collections.Concurrent;
using System.IO;

namespace GWGUI.Emulation.Nec.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime
{

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken, EmulationMessageCategory category = EmulationMessageCategory.Machine, EmulationMessageCode code = EmulationMessageCode.MachineStartFailed)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw MachineExceptions.MachineNotRunning();
        return EmulationMachineCommand.Enqueue(_commands, _gate, () =>
        {
            try
            {
                action();
            }
            catch (Exception error)
            {
                throw CommonCoreHostFunctions.Messages.Translate(error, category, code, new EmulationMachineMessageContext(Configuration.Model));
            }
        }, cancellationToken);
    }
    private void Run(CancellationToken cancellationToken)
    {
        CommonCoreHostFunctions.MachineExecution.RunCore(
            () => _core.Initialize(Configuration, _sessionDirectory, _saveDirectory),
            () =>
            {
                _audio.Start(_core.SampleRate);
                lock (_gate)
                {
                    State = EmulationMachineState.Running;
                    _started?.TrySetResult();
                }
                var nextFrame = TimeProvider.System.GetTimestamp();
                long videoSequence = 0;

                Action<Machine, AudioChunk> publishAudio = static (machine, audio) =>
                {
                    machine._audio.Write(audio);
                    machine.AudioChunkReady?.Invoke(machine, audio);
                };

                CommonCoreHostFunctions.MachineExecution.RunLoop(_gate, _commands, () => _pauseRequested,
                    () => {
                        _core.RunFrame();
                        EmulationFrameOutputFunctions.PublishOutputs(this, ref videoSequence,
                            static machine => machine._core.LatestVideoFrame,
                            static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
                            static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
                            publishAudio);

                        nextFrame = CommonCoreHostFunctions.MachineExecution.WaitForNextFrame(nextFrame, _core.FramesPerSecond, TimeProvider.System, Thread.Sleep);
                        return true;
                    },
                    () => !cancellationToken.IsCancellationRequested, cancellationToken, 100);
            },
            error => () =>
            {
                error = error is CoreMediaRequiredException
                    ? RequiredMediaWarning(Configuration, error)
                    : _startErrorTranslator?.Invoke(error)
                    ?? CommonCoreHostFunctions.Messages.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, new EmulationMachineMessageContext(Configuration.Model));
                if (_core.Diagnostics.Count > 0) error.Data[MachineConstants.Diagnostics] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(100));
                _started?.TrySetException(error);
                FailPendingCommands(error);
                lock (_gate) State = EmulationMachineState.Faulted;
            },
            initialized => {
                if (initialized)
                {
                    try { _core.Stop(); }
                    catch (Exception) { }
                }
                try { _audio.Stop(); }
                catch (Exception) { }
                FailPendingCommands(new OperationCanceledException());
                lock (_gate)
                    if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
            });
    }

    internal static EmulationMessageException RequiredMediaWarning(MachineConfiguration configuration, Exception error)
    {
        var storage = StorageSettingsFunctions.Describe(configuration);
        var devices = storage.AvailableDevices.Where(device => storage.ConfiguredSlots.Contains(device.Slot) && device.MediaType is EmulationMediaType.Cartridge or EmulationMediaType.CompactDisc).ToArray();
        return new EmulationMessageException(new EmulationMessage(
            EmulationMessageCategory.Media,
            EmulationMessageCode.RequiredMediaMissing,
            EmulationMessageSeverity.Warning,
            EmulationMessageTarget.Dialog,
            new EmulationRequiredMachineMediaMessageContext(configuration.Model, devices.Select(device => device.Slot.Category).Distinct().ToArray(), devices.Select(device => device.DisplayLabel ?? device.Slot.ToString()).ToArray())),
            error);
    }
    private void FailPendingCommands(Exception error)
        => EmulationMachineCommand.FailPending(_commands, error);

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            await CommonCoreHostFunctions.MachineExecution.DisposeMachineAsync(
                () => StopAsync(),
                () => { try { _stop?.Dispose(); } finally { _stop = null; } },
                () => _core.Dispose(),
                () => _audio.Dispose(),
                DeleteSessionDirectory).ConfigureAwait(false);
        }
        finally { _disposed = true; }
    }

    private void DeleteSessionDirectory()
    {
        if (_deleteSession is not null) { _deleteSession(_sessionDirectory); return; }
        EmulationSdk.SessionDirectories.DeleteSessionDirectory(_sessionDirectory);
    }
}
