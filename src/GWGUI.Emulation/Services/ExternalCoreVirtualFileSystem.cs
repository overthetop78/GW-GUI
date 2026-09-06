using System.Buffers;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreVirtualFileSystem : IDisposable
{
    private const uint InterfaceVersion = 1;
    private const uint ReadAccess = 1;
    private const uint WriteAccess = 2;
    private const uint UpdateExisting = 4;
    private const int Failure = -1;
    private readonly object _gate = new();
    private readonly Dictionary<nint, OpenFile> _files = [];
    private readonly Action<string, long>? _readObserver;
    private readonly ExternalCoreApi.VfsGetPath _getPath;
    private readonly ExternalCoreApi.VfsOpen _open;
    private readonly ExternalCoreApi.VfsClose _close;
    private readonly ExternalCoreApi.VfsSize _size;
    private readonly ExternalCoreApi.VfsTell _tell;
    private readonly ExternalCoreApi.VfsSeek _seek;
    private readonly ExternalCoreApi.VfsRead _read;
    private readonly ExternalCoreApi.VfsWrite _write;
    private readonly ExternalCoreApi.VfsFlush _flush;
    private readonly ExternalCoreApi.VfsRemove _remove;
    private readonly ExternalCoreApi.VfsRename _rename;
    private readonly nint _interface;
    private long _nextHandle;
    private bool _disposed;

    internal ExternalCoreVirtualFileSystem(Action<string, long>? readObserver = null)
    {
        _readObserver = readObserver;
        _getPath = GetPath;
        _open = Open;
        _close = Close;
        _size = Size;
        _tell = Tell;
        _seek = Seek;
        _read = Read;
        _write = Write;
        _flush = Flush;
        _remove = Remove;
        _rename = Rename;
        var descriptor = new ExternalCoreApi.VfsInterface
        {
            GetPath = Marshal.GetFunctionPointerForDelegate(_getPath),
            Open = Marshal.GetFunctionPointerForDelegate(_open),
            Close = Marshal.GetFunctionPointerForDelegate(_close),
            Size = Marshal.GetFunctionPointerForDelegate(_size),
            Tell = Marshal.GetFunctionPointerForDelegate(_tell),
            Seek = Marshal.GetFunctionPointerForDelegate(_seek),
            Read = Marshal.GetFunctionPointerForDelegate(_read),
            Write = Marshal.GetFunctionPointerForDelegate(_write),
            Flush = Marshal.GetFunctionPointerForDelegate(_flush),
            Remove = Marshal.GetFunctionPointerForDelegate(_remove),
            Rename = Marshal.GetFunctionPointerForDelegate(_rename)
        };
        _interface = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.VfsInterface>());
        Marshal.StructureToPtr(descriptor, _interface, false);
    }

    internal bool Provide(nint data)
    {
        if (_disposed || data == nint.Zero) return false;
        var request = Marshal.PtrToStructure<ExternalCoreApi.VfsInterfaceInfo>(data);
        if (request.RequiredInterfaceVersion > InterfaceVersion) return false;
        request.RequiredInterfaceVersion = InterfaceVersion;
        request.Interface = _interface;
        Marshal.StructureToPtr(request, data, false);
        return true;
    }

    private nint GetPath(nint handle) => TryGet(handle, out var file) ? file.Path.Pointer : nint.Zero;

    private nint Open(nint pathPointer, uint mode, uint hints)
    {
        try
        {
            var path = Marshal.PtrToStringUTF8(pathPointer);
            if (string.IsNullOrWhiteSpace(path) || Directory.Exists(path)) return nint.Zero;
            var reads = (mode & ReadAccess) != 0;
            var writes = (mode & WriteAccess) != 0;
            if (!reads && !writes) return nint.Zero;
            var access = reads && writes ? FileAccess.ReadWrite : reads ? FileAccess.Read : FileAccess.Write;
            var fileMode = writes
                ? (mode & UpdateExisting) != 0 ? FileMode.OpenOrCreate : FileMode.Create
                : FileMode.Open;
            var fullPath = Path.GetFullPath(path);
            var stream = new FileStream(fullPath, fileMode, access, FileShare.ReadWrite | FileShare.Delete);
            var file = new OpenFile(fullPath, stream);
            lock (_gate)
            {
                var handle = checked((nint)(++_nextHandle));
                _files.Add(handle, file);
                return handle;
            }
        }
        catch { return nint.Zero; }
    }

    private int Close(nint handle)
    {
        OpenFile? file;
        lock (_gate)
        {
            if (!_files.Remove(handle, out file)) return Failure;
        }
        try { file.Dispose(); return 0; }
        catch { return Failure; }
    }

    private long Size(nint handle)
    {
        try { return TryGet(handle, out var file) ? file.Stream.Length : Failure; }
        catch { return Failure; }
    }

    private long Tell(nint handle)
    {
        try { return TryGet(handle, out var file) ? file.Stream.Position : Failure; }
        catch { return Failure; }
    }

    private long Seek(nint handle, long offset, int origin)
    {
        try
        {
            if (!TryGet(handle, out var file) || origin is < 0 or > 2) return Failure;
            return file.Stream.Seek(offset, (SeekOrigin)origin);
        }
        catch { return Failure; }
    }

    private long Read(nint handle, nint destination, ulong requestedLength)
    {
        if (destination == nint.Zero || requestedLength > int.MaxValue || !TryGet(handle, out var file)) return Failure;
        var length = checked((int)requestedLength);
        var buffer = ArrayPool<byte>.Shared.Rent(Math.Max(length, 1));
        try
        {
            var read = file.Stream.Read(buffer, 0, length);
            if (read > 0)
            {
                Marshal.Copy(buffer, 0, destination, read);
                _readObserver?.Invoke(file.FullPath, read);
            }
            return read;
        }
        catch { return Failure; }
        finally { ArrayPool<byte>.Shared.Return(buffer); }
    }

    private long Write(nint handle, nint source, ulong requestedLength)
    {
        if (source == nint.Zero || requestedLength > int.MaxValue || !TryGet(handle, out var file)) return Failure;
        var length = checked((int)requestedLength);
        var buffer = ArrayPool<byte>.Shared.Rent(Math.Max(length, 1));
        try
        {
            Marshal.Copy(source, buffer, 0, length);
            file.Stream.Write(buffer, 0, length);
            return length;
        }
        catch { return Failure; }
        finally { ArrayPool<byte>.Shared.Return(buffer); }
    }

    private int Flush(nint handle)
    {
        try
        {
            if (!TryGet(handle, out var file)) return Failure;
            file.Stream.Flush();
            return 0;
        }
        catch { return Failure; }
    }

    private static int Remove(nint pathPointer)
    {
        try
        {
            var path = Marshal.PtrToStringUTF8(pathPointer);
            if (string.IsNullOrWhiteSpace(path)) return Failure;
            File.Delete(path);
            return 0;
        }
        catch { return Failure; }
    }

    private static int Rename(nint oldPathPointer, nint newPathPointer)
    {
        try
        {
            var oldPath = Marshal.PtrToStringUTF8(oldPathPointer);
            var newPath = Marshal.PtrToStringUTF8(newPathPointer);
            if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath)) return Failure;
            File.Move(oldPath, newPath);
            return 0;
        }
        catch { return Failure; }
    }

    private bool TryGet(nint handle, out OpenFile file)
    {
        lock (_gate) return _files.TryGetValue(handle, out file!);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        OpenFile[] files;
        lock (_gate)
        {
            files = [.. _files.Values];
            _files.Clear();
        }
        foreach (var file in files) file.Dispose();
        Marshal.FreeHGlobal(_interface);
    }

    private sealed class OpenFile(string fullPath, FileStream stream) : IDisposable
    {
        internal string FullPath { get; } = fullPath;
        internal FileStream Stream { get; } = stream;
        internal ExternalCoreUtf8String Path { get; } = new(fullPath);

        public void Dispose()
        {
            Stream.Dispose();
            Path.Dispose();
        }
    }
}
