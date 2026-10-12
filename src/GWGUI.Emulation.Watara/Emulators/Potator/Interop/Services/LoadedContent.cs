using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;

internal sealed class LoadedContent : IDisposable
{
    private ExternalCoreUtf8String? _path;
    private nint _data;
    internal nint GameInfo { get; private set; }

    internal LoadedContent(string original)
    {
        var completed = false;
        try
        {
            var path = Path.GetFullPath(original);
            var bytes = File.ReadAllBytes(path);
            if (bytes.Length == 0) throw new InvalidDataException("The cartridge is empty.");
            _path = new ExternalCoreUtf8String(path);
            _data = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, _data, bytes.Length);
            GameInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo { Path = _path.Pointer, Data = _data, Size = (nuint)bytes.Length }, GameInfo, false);
            completed = true;
        }
        finally { if (!completed) Dispose(); }
    }

    public void Dispose()
    {
        var info = GameInfo;
        var data = _data;
        GameInfo = nint.Zero;
        _data = nint.Zero;
        try { if (info != nint.Zero) Marshal.FreeHGlobal(info); }
        finally
        {
            try { if (data != nint.Zero) Marshal.FreeHGlobal(data); }
            finally { _path?.Dispose(); _path = null; }
        }
    }
}