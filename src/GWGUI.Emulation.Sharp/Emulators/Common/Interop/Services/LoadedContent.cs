using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;

internal sealed class LoadedContent : IDisposable
{
    private ContentPath? _path;
    private nint _data;

    internal LoadedContent(nint gameInfo, ContentPath path, nint data)
    {
        GameInfo = gameInfo;
        _path = path;
        _data = data;
    }

    internal nint GameInfo { get; private set; }

    public void Dispose()
    {
        var gameInfo = GameInfo;
        var data = _data;
        var path = _path;
        GameInfo = 0;
        _data = 0;
        _path = null;
        try { if (gameInfo != 0) Marshal.FreeHGlobal(gameInfo); }
        finally
        {
            try { if (data != 0) Marshal.FreeHGlobal(data); }
            finally { path?.Dispose(); }
        }
    }
}