using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed class LoadedContent : IDisposable
{
    private ContentPath? _path;
    private nint _data;
    private SessionContentDirectory? _contentDirectory;

    internal LoadedContent(nint gameInfo, ContentPath path, nint data)
    {
        GameInfo = gameInfo;
        _path = path;
        _data = data;
    }

    internal nint GameInfo { get; private set; }
    internal string? ContentFilePath => _path?.Value;

    internal void OwnContentDirectory(SessionContentDirectory directory)
    {
        if (GameInfo == 0 || _contentDirectory is not null) throw new InvalidOperationException(nameof(OwnContentDirectory));
        _contentDirectory = directory;
    }

    public void Dispose()
    {
        var gameInfo = GameInfo;
        var data = _data;
        var path = _path;
        var contentDirectory = _contentDirectory;
        GameInfo = 0;
        _data = 0;
        _path = null;
        _contentDirectory = null;
        try { if (gameInfo != 0) Marshal.FreeHGlobal(gameInfo); }
        finally
        {
            try { if (data != 0) Marshal.FreeHGlobal(data); }
            finally
            {
                try { path?.Dispose(); }
                finally { contentDirectory?.Dispose(); }
            }
        }
    }
}
