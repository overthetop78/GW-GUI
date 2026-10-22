using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Thomson.Common.Functions.CoreHostFunctions;
using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Services;

internal sealed class LoadedContent : IDisposable
{
    private readonly string _original;
    private readonly string _runtime;
    private readonly bool _writeBack;
    private bool _loaded;
    private IEmulationNativeString? _path;
    internal nint GameInfo { get; private set; }

    internal LoadedContent(string original, string sessionDirectory, bool readOnly)
    {
        _original = Path.GetFullPath(original);
        var directory = Path.Combine(Path.GetFullPath(sessionDirectory), "Content");
        Directory.CreateDirectory(directory);
        _runtime = Path.Combine(directory, Path.GetFileName(_original));
        _writeBack = !readOnly;
        var completed = false;
        try
        {
            File.Copy(_original, _runtime, overwrite: true);
            File.SetAttributes(_runtime, File.GetAttributes(_runtime) & ~FileAttributes.ReadOnly);
            _path = CommonCoreHostFunctions.NativeResources.CreateString(_runtime);
            GameInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = _path.Pointer }, GameInfo, false);
            completed = true;
        }
        finally { if (!completed) ReleaseAllocations(); }
    }

    internal void MarkLoaded() => _loaded = true;

    public void Dispose()
    {
        if (GameInfo == nint.Zero) return;
        try
        {
            if (_writeBack && _loaded)
            {
                var temporary = _original + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try { File.Copy(_runtime, temporary); ConfigurationFileAccessFunctions.ReplaceFile(temporary, _original); }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
        }
        finally { ReleaseAllocations(); }
    }

    private void ReleaseAllocations()
    {
        var pointer = GameInfo;
        GameInfo = nint.Zero;
        try { if (pointer != nint.Zero) Marshal.FreeHGlobal(pointer); }
        finally { _path?.Dispose(); _path = null; }
    }
}
