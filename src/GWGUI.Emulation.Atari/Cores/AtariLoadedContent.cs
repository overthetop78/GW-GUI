using System.Runtime.InteropServices;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariLoadedContent : IDisposable
{
    private ExternalCoreUtf8String? _path;
    private nint _data;

    internal AtariLoadedContent(nint gameInfo, ExternalCoreUtf8String path, nint data)
    {
        GameInfo = gameInfo;
        _path = path;
        _data = data;
    }

    internal nint GameInfo { get; private set; }

    public void Dispose()
    {
        if (GameInfo != 0)
        {
            Marshal.FreeHGlobal(GameInfo);
            GameInfo = 0;
        }
        if (_data != 0)
        {
            Marshal.FreeHGlobal(_data);
            _data = 0;
        }
        _path?.Dispose();
        _path = null;
    }
}
