using System.IO;
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

namespace GWGUI.Emulation.Acorn.Common.Services;

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
        try { CoreHostFunctions.DisposeTransport(_pipe); }
        finally { WaitForHostExit(graceful: false); }
    }

    private void WaitForHostExit(bool graceful)
    {
        var process = _process;
        if (process is null) return;
        if (graceful && process.WaitForExit(CoreHostConstants.GracefulExitTimeoutMilliseconds)) return;
        if (!process.HasExited)
        {
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
        }
        process.WaitForExit();
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
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
        }
        finally
        {
            _disposed = true;
            using var process = _process;
            using var videoMemory = _videoMemory;
            using var videoMap = _videoMap;
            using var requestGate = _requestGate;
            try
            {
                try
                {
                    try { CoreHostFunctions.DisposeTransport(_writer); }
                    finally { CoreHostFunctions.DisposeTransport(_pipe); }
                }
                finally { WaitForHostExit(graceful: true); }
            }
            finally
            {
                _writer = null;
                _pipe = null;
                _videoMap = null;
                _videoMemory = null;
                while (_audio.TryDequeue(out _))
                {
                }
            }
        }
    }
}
