using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class Machine : IEmulatedMachine, IEmulationLifecycle, IEmulationInput,
    IEmulationMedia, IEmulationVideo, IEmulationAudio, IEmulationSavedStates, IEmulationRuntime,
    IEmulationCassetteTransport
{
    private MachineConfiguration CurrentConfiguration() => Configuration with { Media = _mountedMedia.ToArray() };

    private ValueTask QueueCommand(Action action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (State is not EmulationMachineState.Running and not EmulationMachineState.Paused)
            throw new InvalidOperationException(ErrorMessages.MachineInvalidState);
        return CommonCoreHostFunctions.MachineExecution.EnqueueCommand(_commands, _gate, action, cancellationToken);
    }

    private void Run(CancellationToken cancellationToken)
    {
        CommonCoreHostFunctions.MachineExecution.RunCore(
            () => _core.Initialize(Configuration, _sessionDirectory, _saveDirectory),
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
                    () => {
                        _core.SetInput(_cassetteInput.NextFrame());
                        if (_core is IEmulatorAudioBufferFeedback feedback)
                            feedback.SetAudioBufferStatus(_audio.BufferStatus);
                        _core.RunFrame();
                        if (_core is IEmulatorAudioBufferFeedback latency)
                            _audio.TrySetMinimumLatency(latency.MinimumAudioLatencyMilliseconds);
                        PublishOutputs(ref videoSequence);
                    },
                    () => _core.FramesPerSecond, MachineFunctions.NextFrameTimestamp, frameTimer.WaitUntil,
                    cancellationToken, MachineConstants.PauseWaitMilliseconds);
            },
            error => () =>
            {
                error = MessageFunctions.Translate(error, Configuration);
                if (_core.Diagnostics.Count > MachineConstants.EmptyCount)
                    error.Data[MachineConstants.DiagnosticDataKey] = string.Join(Environment.NewLine, _core.Diagnostics.TakeLast(MachineConstants.DiagnosticTailCount));
                _started?.TrySetException(error);
                FailPendingCommands(error);
                lock (_gate) State = EmulationMachineState.Faulted;
            },
            initialized => {
                if (initialized) MachineFunctions.TryReleaseInput(_core);
                if (initialized)
                    try { _core.Stop(); } catch (Exception) { }
                CommonCoreHostFunctions.Protocol.DisposeTransportIgnoringErrors(_core);
                try { _audio.Stop(); } catch (Exception) { }
                MachineFunctions.DeleteSessionDirectory(_sessionDirectory);
                FailPendingCommands(new OperationCanceledException(ErrorMessages.MachineStopped));
                lock (_gate)
                    if (State != EmulationMachineState.Faulted) State = EmulationMachineState.Stopped;
            });
    }

    private void StartAudio()
    {
        _audio.Start(_core.SampleRate);
    }

    private void PublishOutputs(ref long videoSequence)
        => CommonCoreHostFunctions.MachineExecution.PublishOutputs(this, ref videoSequence,
            static machine => machine._core.LatestVideoFrame,
            static machine => machine._core.TryDequeueAudio(out var audio) ? audio : null,
            static (machine, video) => machine.VideoFrameReady?.Invoke(machine, video),
            static (machine, audio) =>
            {
                machine._audio.Write(audio);
                machine.AudioChunkReady?.Invoke(machine, audio);
            });

    private void FailPendingCommands(Exception error)
        => CommonCoreHostFunctions.MachineExecution.FailPendingCommands(_commands, error);

    private void ResetCore(string resetType)
    {
        _audio.Reset();
        if (Configuration.Core is Emulator.Hatari or Emulator.Hatari2014 or Emulator.HatariB)
            _core.SetOption(MachineValues.ResetType, resetType);
        _core.HardReset();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        var disposeUnstartedCore = _thread is null;
        try
        {
            await CommonCoreHostFunctions.MachineExecution.DisposeMachineAsync(
                () => StopAsync(),
                () => _stopSource?.Dispose(),
                () => { if (disposeUnstartedCore) _core.Dispose(); },
                () => _audio.Dispose(),
                () => MachineFunctions.DeleteSessionDirectory(_sessionDirectory)).ConfigureAwait(false);
        }
        finally { _disposed = true; }
    }
}
