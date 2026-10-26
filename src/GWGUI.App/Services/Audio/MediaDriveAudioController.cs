using System.Diagnostics;
using GWGUI.Emulation;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interfaces;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Services;
using C = GWGUI.App.Constants.Audio.MediaDriveAudioConstants;

namespace GWGUI.App.Services.Audio;

/// <summary>Mélange les lecteurs des outils avec la sortie existante ; les mécaniques restent empruntées.</summary>
internal sealed class MediaDriveAudioController : IDisposable
{
    private readonly object _gate = new();
    private readonly MediaDriveService _source;
    private readonly Action<Exception, string> _writeError;
    private readonly Func<IMultichannelAudioOutput> _createOutput;
    private readonly HashSet<IMediaDriveMechanics> _active = [];
    private readonly CancellationTokenSource _shutdown = new();
    private readonly SemaphoreSlim _wake = new(0, 1);
    private Task? _worker;
    private bool _disposed;

    /// <summary>Relie le service média sans ouvrir de périphérique avant la première activité.</summary>
    internal MediaDriveAudioController(MediaDriveService source, Action<Exception, string> writeError,
        Func<IMultichannelAudioOutput>? createOutput = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(writeError);
        _source = source;
        _writeError = writeError;
        _createOutput = createOutput ?? (() => new WasapiAudioOutput());
        source.AudioAvailable += OnAudioAvailable;
    }

    private void OnAudioAvailable(IMediaDriveMechanics drive)
    {
        lock (_gate)
        {
            if (_disposed) return;
            _active.Add(drive);
            _worker ??= Task.Run(PumpAsync);
            if (_wake.CurrentCount == 0) _wake.Release();
        }
    }

    private async Task PumpAsync()
    {
        var cancellationToken = _shutdown.Token;
        try
        {
            while (true)
            {
                await _wake.WaitAsync(cancellationToken).ConfigureAwait(false);
                lock (_gate) if (_active.Count == 0) continue;
                try { await RenderBurstAsync(cancellationToken).ConfigureAwait(false); }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
                catch (Exception error)
                {
                    lock (_gate) _active.Clear();
                    _writeError(error, nameof(MediaDriveAudioController));
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private async Task RenderBurstAsync(CancellationToken cancellationToken)
    {
        IMultichannelAudioOutput? output = null;
        try
        {
            output = _createOutput();
            output.Start(C.SampleRate, EmulationAudioChannelLayout.Mono);
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                IMediaDriveMechanics[] drives;
                lock (_gate) drives = _active.ToArray();
                if (drives.Length == 0) break;
                var mixed = new double[C.FramesPerChunk];
                foreach (var drive in drives)
                {
                    try
                    {
                        if (!drive.IsInUse && drive.State.MotorRunning) drive.SetMotor(false);
                        var samples = drive.RenderAudio(C.SampleRate, C.FramesPerChunk);
                        for (var index = 0; index < samples.Length; index++) mixed[index] += samples[index];
                    }
                    catch (Exception error)
                    {
                        lock (_gate) _active.Remove(drive);
                        _writeError(error, nameof(MediaDriveAudioController));
                    }
                }
                output.Write(EmulationSdk.MediaProcessing.ProtectMechanicalAudioMix(mixed));
                await Task.Delay(C.ChunkMilliseconds, cancellationToken).ConfigureAwait(false);
                lock (_gate) _active.RemoveWhere(drive => !drive.HasPendingAudio);
            }
            var draining = Stopwatch.GetTimestamp();
            while (output.BufferStatus is { IsActive: true, BufferedFrames: > 0 }
                && Stopwatch.GetElapsedTime(draining).TotalMilliseconds < C.MaximumDrainMilliseconds)
                await Task.Delay(C.ChunkMilliseconds, cancellationToken).ConfigureAwait(false);
        }
        finally { output?.Dispose(); }
    }

    /// <summary>Détache les notifications, annule et attend la tâche avant de libérer ses ressources.</summary>
    public void Dispose()
    {
        Task? worker;
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _source.AudioAvailable -= OnAudioAvailable;
            worker = _worker;
        }
        try
        {
            try { _shutdown.Cancel(); }
            finally { worker?.GetAwaiter().GetResult(); }
        }
        finally
        {
            try { _shutdown.Dispose(); }
            finally
            {
                _wake.Dispose();
                lock (_gate) _active.Clear();
            }
        }
    }
}
