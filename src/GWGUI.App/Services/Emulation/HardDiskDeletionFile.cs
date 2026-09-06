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
