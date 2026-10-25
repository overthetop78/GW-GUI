using System.IO;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreLoadedContent : IEmulationLoadedContent
{
    private ExternalCoreUtf8String? _path;
    private nint _data;

    private ExternalCoreLoadedContent(nint gameInfo, ExternalCoreUtf8String path, nint data)
    {
        GameInfo = gameInfo;
        _path = path;
        _data = data;
    }

    public nint GameInfo { get; private set; }

    internal static ExternalCoreLoadedContent Create(string absolutePath, bool needsFullPath, bool useWindowsAnsiPath)
    {
        var path = new ExternalCoreUtf8String(absolutePath, useWindowsAnsiPath);
        var data = nint.Zero;
        var gameInfo = nint.Zero;
        var transferred = false;
        try
        {
            nuint size = nuint.Zero;
            if (!needsFullPath)
            {
                var bytes = File.ReadAllBytes(absolutePath);
                data = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, data, bytes.Length);
                size = (nuint)bytes.Length;
            }
            gameInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
            {
                Path = path.Pointer,
                Data = data,
                Size = size,
                Metadata = nint.Zero
            }, gameInfo, false);
            var result = new ExternalCoreLoadedContent(gameInfo, path, data);
            transferred = true;
            return result;
        }
        finally
        {
            if (!transferred)
            {
                try { if (gameInfo != nint.Zero) Marshal.FreeHGlobal(gameInfo); }
                finally
                {
                    try { if (data != nint.Zero) Marshal.FreeHGlobal(data); }
                    finally { path.Dispose(); }
                }
            }
        }
    }

    public void Dispose()
    {
        var gameInfo = GameInfo;
        var data = _data;
        var path = _path;
        GameInfo = nint.Zero;
        _data = nint.Zero;
        _path = null;
        try { if (gameInfo != nint.Zero) Marshal.FreeHGlobal(gameInfo); }
        finally
        {
            try { if (data != nint.Zero) Marshal.FreeHGlobal(data); }
            finally { path?.Dispose(); }
        }
    }
}
