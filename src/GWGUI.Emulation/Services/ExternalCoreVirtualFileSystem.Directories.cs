using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed partial class ExternalCoreVirtualFileSystem
{
    private const int DirectoryAlreadyExists = -2;
    private readonly Dictionary<nint, OpenDirectoryState> _directories = [];
    private readonly ExternalCoreApi.VfsMakeDirectory _makeDirectory;
    private readonly ExternalCoreApi.VfsOpenDirectory _openDirectory;
    private readonly ExternalCoreApi.VfsReadDirectory _readDirectory;
    private readonly ExternalCoreApi.VfsDirectoryEntryName _directoryEntryName;
    private readonly ExternalCoreApi.VfsDirectoryEntryIsDirectory _directoryEntryIsDirectory;
    private readonly ExternalCoreApi.VfsCloseDirectory _closeDirectory;

    private int MakeDirectory(nint pathPointer)
    {
        try
        {
            var path = Marshal.PtrToStringUTF8(pathPointer);
            if (_disposed || string.IsNullOrWhiteSpace(path)) return Failure;
            var resolved = Resolve(path);
            if (resolved.IsReadOnly) return Failure;
            if (Directory.Exists(resolved.Path)) return DirectoryAlreadyExists;
            Directory.CreateDirectory(resolved.Path);
            return 0;
        }
        catch { return Failure; }
    }

    private nint OpenDirectory(nint pathPointer, bool includeHidden)
    {
        OpenDirectoryState? directory = null;
        var ownershipTransferred = false;
        try
        {
            var path = Marshal.PtrToStringUTF8(pathPointer);
            if (_disposed || string.IsNullOrWhiteSpace(path)) return nint.Zero;
            var resolved = Resolve(path);
            if (!Directory.Exists(resolved.Path)) return nint.Zero;
            directory = new OpenDirectoryState(Directory.EnumerateFileSystemEntries(resolved.Path).GetEnumerator(), includeHidden);
            lock (_gate)
            {
                if (_disposed) return nint.Zero;
                var handle = checked((nint)(++_nextHandle));
                _directories.Add(handle, directory);
                ownershipTransferred = true;
                return handle;
            }
        }
        catch { return nint.Zero; }
        finally
        {
            if (!ownershipTransferred)
            {
                try { directory?.Dispose(); }
                catch { }
            }
        }
    }

    private bool ReadDirectory(nint handle)
    {
        try { return TryGetDirectory(handle, out var directory) && directory.MoveNext(); }
        catch { return false; }
    }

    private nint DirectoryEntryName(nint handle) => TryGetDirectory(handle, out var directory) ? directory.Name?.Pointer ?? nint.Zero : nint.Zero;

    private bool DirectoryEntryIsDirectory(nint handle) => TryGetDirectory(handle, out var directory) && directory.IsDirectory;

    private bool TryGetDirectory(nint handle, out OpenDirectoryState directory)
    {
        lock (_gate) return _directories.TryGetValue(handle, out directory!);
    }

    private int CloseDirectory(nint handle)
    {
        OpenDirectoryState? directory;
        lock (_gate)
        {
            if (!_directories.Remove(handle, out directory)) return Failure;
        }
        try { directory.Dispose(); return 0; }
        catch { return Failure; }
    }

    private void DisposeDirectories()
    {
        OpenDirectoryState[] directories;
        lock (_gate)
        {
            directories = [.. _directories.Values];
            _directories.Clear();
        }
        List<Exception>? errors = null;
        foreach (var directory in directories)
        {
            try { directory.Dispose(); }
            catch (Exception error) { (errors ??= []).Add(error); }
        }
        if (errors is not null) throw new AggregateException(errors);
    }

    private sealed class OpenDirectoryState(IEnumerator<string> entries, bool includeHidden) : IDisposable
    {
        internal ExternalCoreUtf8String? Name { get; private set; }
        internal bool IsDirectory { get; private set; }

        internal bool MoveNext()
        {
            Name?.Dispose();
            Name = null;
            IsDirectory = false;
            while (entries.MoveNext())
            {
                var path = entries.Current;
                var attributes = File.GetAttributes(path);
                if (!includeHidden && (attributes & FileAttributes.Hidden) != 0) continue;
                Name = new ExternalCoreUtf8String(Path.GetFileName(path));
                IsDirectory = (attributes & FileAttributes.Directory) != 0;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            try { entries.Dispose(); }
            finally { Name?.Dispose(); Name = null; }
        }
    }
}
