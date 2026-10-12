using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed class SessionContentDirectory : IDisposable
{
    private readonly string _directory;
    private readonly string _sourceRoot;
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    internal SessionContentDirectory(string sessionDirectory, string sourceRoot)
    {
        _sourceRoot = Path.GetFullPath(sourceRoot);
        _directory = Path.Combine(Path.GetFullPath(sessionDirectory), CoreDirectoryConstants.ContentDirectoryName, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    internal string AddFile(string sourcePath, string? relativeDestination = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var source = Path.GetFullPath(sourcePath);
        if (!File.Exists(source)) throw new FileNotFoundException(null, source);
        var destination = Path.GetFullPath(Path.Combine(_directory, relativeDestination ?? Path.GetRelativePath(_sourceRoot, source)));
        if (!destination.StartsWith(_directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(nameof(sourcePath));
        if (_files.Contains(destination)) return destination;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        _files.Add(destination);
        File.Copy(source, destination, overwrite: false);
        return destination;
    }

    internal string AddText(string relativeDestination, string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var destination = Path.GetFullPath(Path.Combine(_directory, relativeDestination));
        if (!destination.StartsWith(_directory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(nameof(relativeDestination));
        if (_files.Contains(destination) || File.Exists(destination)) throw new IOException(destination);
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        _files.Add(destination);
        using var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(text);
        return destination;
    }

    public void Dispose()
    {
        if (_disposed) return;
        var failures = new List<Exception>();
        foreach (var file in _files)
        {
            try { DeleteOwnedFile(file); }
            catch (Exception exception) { failures.Add(exception); }
        }
        if (failures.Count != 0) throw new AggregateException(failures);
        Directory.Delete(_directory, recursive: true);
        _files.Clear();
        _disposed = true;
    }

    private static void DeleteOwnedFile(string path)
    {
        if (!File.Exists(path)) return;
        SafeFileHandle? handle = null;
        try
        {
            handle = CreateFile(NativePath(path), SessionContentConstants.DeleteAccess, SessionContentConstants.ShareReadWriteDelete, nint.Zero, SessionContentConstants.OpenExisting, SessionContentConstants.OpenReparsePoint, nint.Zero);
            if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
            var flags = SessionContentConstants.DeletePosixIgnoreReadOnly;
            if (!SetFileInformationByHandle(handle, SessionContentConstants.FileDispositionInfoEx, ref flags, sizeof(uint))) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally { handle?.Dispose(); }
        if (File.Exists(path)) throw new IOException(path);
    }

    private static string NativePath(string path) => path.StartsWith(SessionContentConstants.ExtendedPathPrefix, StringComparison.Ordinal)
        ? path
        : path.StartsWith(SessionContentConstants.UncPathPrefix, StringComparison.Ordinal)
        ? SessionContentConstants.ExtendedUncPathPrefix + path[SessionContentConstants.UncPathPrefix.Length..]
        : SessionContentConstants.ExtendedPathPrefix + path;

    [DllImport(SessionContentConstants.NativeLibrary, EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string path, uint access, uint share, nint securityAttributes, uint creationDisposition, uint flags, nint template);
    [DllImport(SessionContentConstants.NativeLibrary, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandle(SafeFileHandle handle, int informationClass, ref uint information, uint size);
}
