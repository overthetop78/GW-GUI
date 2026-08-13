using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

namespace GWGUI.Infrastructure.Hardware.Greaseweazle;

[SupportedOSPlatform("windows")]
public sealed class WindowsGreaseweazleSerialTransport : IGreaseweazleSerialTransport
{
    private SafeFileHandle? _handle;
    private FileStream? _stream;

    public bool IsOpen => _handle is { IsClosed: false, IsInvalid: false };

    public ValueTask OpenAsync(
        string portName,
        int baudRate,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(portName);
        cancellationToken.ThrowIfCancellationRequested();
        if (IsOpen) throw new InvalidOperationException("The serial transport is already open.");

        _handle = CreateFile(
            $@"\.\{portName}",
            GenericRead | GenericWrite,
            0,
            IntPtr.Zero,
            OpenExisting,
            FileFlagOverlapped,
            IntPtr.Zero);
        if (_handle.IsInvalid)
        {
            var error = Marshal.GetLastWin32Error();
            _handle.Dispose();
            _handle = null;
            throw new Win32Exception(error, $"Unable to open serial port {portName}.");
        }

        try
        {
            Configure(baudRate);
            _stream = new FileStream(_handle, FileAccess.ReadWrite, 4096, isAsync: true);
            return ValueTask.CompletedTask;
        }
        catch
        {
            _handle.Dispose();
            _handle = null;
            throw;
        }
    }

    public ValueTask SetBaudRateAsync(int baudRate, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Configure(baudRate);
        return ValueTask.CompletedTask;
    }

    public ValueTask DiscardBuffersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();
        if (!PurgeComm(_handle!, PurgeReceiveAbort | PurgeReceiveClear | PurgeTransmitAbort | PurgeTransmitClear))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        return ValueTask.CompletedTask;
    }

    public async ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        await _stream!.WriteAsync(buffer, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    public async ValueTask ReadExactlyAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        await _stream!.ReadExactlyAsync(buffer, cancellationToken);
    }

    public ValueTask CloseAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _stream?.Dispose();
        _stream = null;
        _handle?.Dispose();
        _handle = null;
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync() => await CloseAsync(CancellationToken.None);

    private void Configure(int baudRate)
    {
        if (baudRate <= 0) throw new ArgumentOutOfRangeException(nameof(baudRate));
        EnsureOpen();
        var state = new DeviceControlBlock { Length = (uint)Marshal.SizeOf<DeviceControlBlock>() };
        if (!GetCommState(_handle!, ref state)) throw new Win32Exception(Marshal.GetLastWin32Error());
        state.BaudRate = (uint)baudRate;
        state.Flags |= BinaryMode;
        state.ByteSize = 8;
        state.Parity = 0;
        state.StopBits = 0;
        if (!SetCommState(_handle!, ref state)) throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private void EnsureOpen()
    {
        if (!IsOpen) throw new InvalidOperationException("The serial transport is closed.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DeviceControlBlock
    {
        public uint Length;
        public uint BaudRate;
        public uint Flags;
        public ushort Reserved;
        public ushort XonLimit;
        public ushort XoffLimit;
        public byte ByteSize;
        public byte Parity;
        public byte StopBits;
        public byte XonCharacter;
        public byte XoffCharacter;
        public byte ErrorCharacter;
        public byte EndOfFileCharacter;
        public byte EventCharacter;
        public ushort Reserved1;
    }

    private const uint GenericRead = 0x80000000;
    private const uint GenericWrite = 0x40000000;
    private const uint OpenExisting = 3;
    private const uint FileFlagOverlapped = 0x40000000;
    private const uint PurgeTransmitAbort = 0x0001;
    private const uint PurgeReceiveAbort = 0x0002;
    private const uint PurgeTransmitClear = 0x0004;
    private const uint PurgeReceiveClear = 0x0008;
    private const uint BinaryMode = 0x0001;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetCommState(SafeFileHandle file, ref DeviceControlBlock state);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetCommState(SafeFileHandle file, ref DeviceControlBlock state);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool PurgeComm(SafeFileHandle file, uint flags);
}
