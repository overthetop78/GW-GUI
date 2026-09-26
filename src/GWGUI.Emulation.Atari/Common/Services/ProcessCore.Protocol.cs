using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;
using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Atari.Common.Services;

[SupportedOSPlatform(ProcessCoreConstants.Windows)]
internal sealed partial class ProcessCore : IEmulatorCore
{
private void Request(HostCommand command, Action<BinaryWriter>? write = null,
        Action<BinaryReader>? read = null, bool allowUninitialized = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_connectionFailed) throw new InvalidOperationException(CoreHostErrors.ProcessUnavailable);
        if (_cancellationToken.IsCancellationRequested)
            FailConnection(new OperationCanceledException(_cancellationToken));
        if (!allowUninitialized && !_initialized)
            throw new InvalidOperationException(CoreHostErrors.ProcessNotInitialized);
        _requestGate.Wait();
        try
        {
            var writer = _writer ?? throw new InvalidOperationException(CoreHostErrors.ProcessUnavailable);
            CoreHostFunctions.WriteRequestHeader(writer, command);
            write?.Invoke(writer);
            writer.Flush();
            using var reader = new BinaryReader(new MemoryStream(ReadResponse(), writable: false), Encoding.UTF8,
                leaveOpen: false);
            var status = CoreHostFunctions.ReadResponseHeader(reader);
            if (status == HostResponseStatus.Failure) ThrowRemoteError(CoreHostFunctions.ReadError(reader));
            if (status != HostResponseStatus.Success)
                throw new InvalidDataException(CoreHostErrors.CommunicationFailed);
            read?.Invoke(reader);
        }
        catch (Exception error) when (error is IOException or EndOfStreamException or OperationCanceledException
                                      or InvalidDataException)
        {
            FailConnection(error);
        }
        finally
        {
            _requestGate.Release();
        }
    }

    private byte[] ReadResponse()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
        timeout.CancelAfter(_responseTimeout);
        var header = new byte[sizeof(int)];
        _pipe!.ReadExactlyAsync(header, timeout.Token).AsTask().GetAwaiter().GetResult();
        var length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 0 or > EmulationHostProtocolConstants.MaximumBlobLength)
            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture,
                CoreHostErrors.InvalidResponseLengthFormat, length));
        var response = GC.AllocateUninitializedArray<byte>(length);
        _pipe.ReadExactlyAsync(response, timeout.Token).AsTask().GetAwaiter().GetResult();
        return response;
    }

    private static void ThrowRemoteError(HostError error)
    {
        if (error.Category is { } category && error.Code is { } code)
            throw new EmulationException(category, code, error.Message, error.Context,
                isLocalized: error.IsLocalized);
        throw new InvalidOperationException(error.Message);
    }

    private void FailConnection(Exception error)
    {
        var cancelled = _cancellationToken.IsCancellationRequested;
        var timedOut = error is OperationCanceledException && !cancelled;
        var exitSuffix = _process is { HasExited: true }
            ? string.Format(CultureInfo.InvariantCulture, CoreHostErrors.ProcessExitSuffixFormat,
                _process.ExitCode)
            : string.Empty;
        _connectionFailed = true;
        TerminateHostProcess();
        var message = cancelled
            ? CoreHostErrors.RequestCancelled
            : timedOut
                ? CoreHostErrors.ResponseTimeout
                : CoreHostErrors.CommunicationFailed + exitSuffix;
        throw new InvalidOperationException(message, error);
    }

    private void TerminateHostProcess()
    {
        _pipe?.Dispose();
        if (_process is null) return;
        try
        {
            if (!_process.HasExited) _process.Kill(entireProcessTree: true);
            _process.WaitForExit(CoreHostConstants.GracefulExitTimeoutMilliseconds);
        }
        catch (Exception)
        {
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (!_connectionFailed && _pipe?.IsConnected == true)
        {
            try
            {
                Request(HostCommand.Dispose, allowUninitialized: true);
            }
            catch (Exception)
            {
            }
        }
        _disposed = true;
        CoreHostFunctions.DisposeTransport(_writer);
        CoreHostFunctions.DisposeTransport(_pipe);
        _videoMap?.Dispose();
        _videoMemory?.Dispose();
        if (_process is not null)
        {
            try
            {
                if (!_process.WaitForExit(CoreHostConstants.GracefulExitTimeoutMilliseconds))
                    _process.Kill(entireProcessTree: true);
                _process.WaitForExit(CoreHostConstants.GracefulExitTimeoutMilliseconds);
            }
            catch (Exception)
            {
            }
            _process.Dispose();
        }
        _requestGate.Dispose();
        while (_audio.TryDequeue(out _))
        {
        }
    }
}
