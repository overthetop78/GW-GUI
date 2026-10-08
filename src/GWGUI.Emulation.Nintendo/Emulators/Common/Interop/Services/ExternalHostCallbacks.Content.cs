using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks
{
    internal const uint SetContentInfoOverride = 65;
    internal const uint GetGameInfoExtended = 66;
    private const char ContentExtensionSeparator = '|';
    private const char ContentExtensionPrefix = '.';
    private readonly Dictionary<string, bool> _contentPathOverrides = new(StringComparer.OrdinalIgnoreCase);
    private nint _extendedGameInfo;

    [StructLayout(LayoutKind.Sequential)]
    private struct ContentInfoOverride
    {
        internal nint Extensions;
        [MarshalAs(UnmanagedType.I1)] internal bool NeedFullPath;
        [MarshalAs(UnmanagedType.I1)] internal bool PersistentData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ExtendedGameInfo
    {
        internal nint FullPath, ArchivePath, ArchiveFile, Directory, Name, Extension, Metadata, Data;
        internal nuint Size;
        [MarshalAs(UnmanagedType.I1)] internal bool FileInArchive;
        [MarshalAs(UnmanagedType.I1)] internal bool PersistentData;
    }

    private bool CaptureContentOverrides(nint data)
    {
        if (data == nint.Zero) return false;
        var size = Marshal.SizeOf<ContentInfoOverride>();
        for (var current = data; ; current += size)
        {
            var item = Marshal.PtrToStructure<ContentInfoOverride>(current);
            if (item.Extensions == nint.Zero) break;
            foreach (var extension in Marshal.PtrToStringUTF8(item.Extensions)!.Split(ContentExtensionSeparator))
                _contentPathOverrides.TryAdd(extension, item.NeedFullPath);
        }
        return true;
    }

    internal bool NeedsContentPath(string path, bool defaultValue) =>
        _contentPathOverrides.TryGetValue(Path.GetExtension(path).TrimStart(ContentExtensionPrefix), out var value)
            ? value : defaultValue;

    internal void SetExtendedGameInfo(string path, nint data, nuint size)
    {
        ReleaseExtendedGameInfo();
        _extendedGameInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ExtendedGameInfo>());
        Marshal.StructureToPtr(new ExtendedGameInfo
        {
            FullPath = NativeString(path),
            Directory = NativeString(Path.GetDirectoryName(path)!),
            Name = NativeString(Path.GetFileNameWithoutExtension(path)),
            Extension = NativeString(Path.GetExtension(path).TrimStart(ContentExtensionPrefix).ToLowerInvariant()),
            Data = data,
            Size = size,
            PersistentData = true
        }, _extendedGameInfo, false);
    }

    private bool ProvideExtendedGameInfo(nint data)
    {
        if (data == nint.Zero || _extendedGameInfo == nint.Zero) return false;
        Marshal.WriteIntPtr(data, _extendedGameInfo);
        return true;
    }

    private void ReleaseExtendedGameInfo()
    {
        if (_extendedGameInfo == nint.Zero) return;
        Marshal.FreeHGlobal(_extendedGameInfo);
        _extendedGameInfo = nint.Zero;
    }
}
