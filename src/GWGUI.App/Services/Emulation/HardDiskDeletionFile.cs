using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace GWGUI.App.Services.Emulation;

internal static class HardDiskDeletionFile
{
    internal static string ResolveReferencePath(string path)
    {
        // Metadata-only access follows file and directory aliases, including while the candidate is locked for deletion.
        using var handle = CreateFile(path, 0, 7, 0, 3, 0, 0);
        if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error());
        return ResolveHandlePath(handle);
    }

    internal static SafeFileHandle Open(string path)
    {
        var handle = CreateFile(path, 0x80010000, 0, 0, 3, 0x00200000, 0); // READ | DELETE, open reparse point itself
        if (handle.IsInvalid) { handle.Dispose(); throw new Win32Exception(Marshal.GetLastWin32Error()); }
        try
        {
            if (!GetFileInformationByHandle(handle, out var information)) throw new Win32Exception(Marshal.GetLastWin32Error());
            if (information.NumberOfLinks != 1 || (information.Attributes & (uint)(FileAttributes.ReparsePoint | FileAttributes.Directory)) != 0)
                throw new IOException(GWGUI.App.Localization.Extensions.LocExtension.Get("Emulation.Hdd.Link"));
            return handle;
        }
        catch { handle.Dispose(); throw; }
    }

    internal static void Delete(SafeFileHandle handle)
    {
        byte delete = 1;
        if (!SetFileInformationByHandle(handle, 4, ref delete, 1)) throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    internal static IReadOnlyList<SafeFileHandle> Open(IReadOnlyList<string> paths)
    {
        var handles = new List<SafeFileHandle>(paths.Count);
        try
        {
            foreach (var path in paths) handles.Add(Open(path));
            return handles;
        }
        catch
        {
            foreach (var handle in handles) handle.Dispose();
            throw;
        }
    }

    internal static void Delete(IReadOnlyList<SafeFileHandle> handles, IReadOnlyList<string> paths)
    {
        if (handles.Count == 0 || handles.Count != paths.Count)
            throw new ArgumentException("Every validated image set member requires one open handle.", nameof(handles));
        for (var index = 0; index < handles.Count; index++)
        {
            var handle = handles[index];
            if (handle.IsInvalid || handle.IsClosed)
                throw new IOException("A validated image set member is no longer open.");
            var actual = ResolveHandlePath(handle);
            var expected = Path.GetFullPath(paths[index]);
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                throw new IOException("An open handle no longer identifies the validated image set member.");
        }
        foreach (var handle in handles) Delete(handle);
        foreach (var handle in handles) handle.Dispose();
        RemoveEmptySparseBundleDirectories(paths);
    }

    private static void RemoveEmptySparseBundleDirectories(IReadOnlyList<string> paths)
    {
        var root = paths.Select(path => new FileInfo(path).Directory)
            .SelectMany(directory => Ancestors(directory))
            .FirstOrDefault(directory => directory.Extension.Equals(".sparsebundle", StringComparison.OrdinalIgnoreCase));
        if (root is null) return;
        var bands = Path.Combine(root.FullName, "bands");
        if (Directory.Exists(bands)) Directory.Delete(bands, recursive: false);
        if (Directory.Exists(root.FullName)) Directory.Delete(root.FullName, recursive: false);
    }

    private static IEnumerable<DirectoryInfo> Ancestors(DirectoryInfo? directory)
    {
        while (directory is not null)
        {
            yield return directory;
            directory = directory.Parent;
        }
    }

    private static string ResolveHandlePath(SafeFileHandle handle)
    {
        var buffer = new StringBuilder(512);
        var length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Capacity, 0);
        if (length == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
        if (length >= buffer.Capacity)
        {
            buffer = new StringBuilder(checked((int)length + 1));
            length = GetFinalPathNameByHandle(handle, buffer, (uint)buffer.Capacity, 0);
            if (length == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
            if (length >= buffer.Capacity) throw new IOException("The resolved image path changed during the lookup.");
        }
        var resolved = buffer.ToString();
        if (resolved.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase)) return "\\\\" + resolved[8..];
        return resolved.StartsWith("\\\\?\\", StringComparison.Ordinal) ? resolved[4..] : resolved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileInformation
    {
        public uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
        public uint VolumeSerial, SizeHigh, SizeLow, NumberOfLinks, IndexHigh, IndexLow;
    }

    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string path, uint access, uint share, nint security,
        uint creation, uint attributes, nint template);
    [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandle(SafeFileHandle handle, StringBuilder path, uint capacity, uint flags);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileInformation information);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetFileInformationByHandle(SafeFileHandle handle, int informationClass, ref byte information, uint size);
}
