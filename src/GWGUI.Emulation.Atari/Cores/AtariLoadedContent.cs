using System.Runtime.InteropServices;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariLoadedContent : IDisposable
{
    private ExternalCoreUtf8String? _path;
    private nint _data;

    private AtariLoadedContent(nint gameInfo, ExternalCoreUtf8String path, nint data)
    {
        GameInfo = gameInfo;
        _path = path;
        _data = data;
    }

    internal nint GameInfo { get; private set; }

    internal static AtariLoadedContent Create(string contentPath, bool needsFullPath,
        IReadOnlySet<string> supportedExtensions)
    {
        if (!File.Exists(contentPath))
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentNotFound,
                AtariErrorMessages.ContentFileMissing,
                new Dictionary<string, string> { [AtariConstants.PathContextKey] = contentPath });
        var extension = Path.GetExtension(contentPath).TrimStart(AtariConstants.ExtensionPrefix);
        if (!supportedExtensions.Contains(extension))
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                AtariErrorMessages.ContentExtensionUnsupported,
                new Dictionary<string, string> { [AtariConstants.ExtensionContextKey] = extension });

        var path = new ExternalCoreUtf8String(Path.GetFullPath(contentPath));
        nint data = 0;
        nint gameInfo = 0;
        try
        {
            nuint size = 0;
            if (!needsFullPath)
            {
                var bytes = File.ReadAllBytes(contentPath);
                data = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, AtariConstants.FirstBufferIndex, data, bytes.Length);
                size = (nuint)bytes.Length;
            }

            gameInfo = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.GameInfo>());
            Marshal.StructureToPtr(new ExternalCoreApi.GameInfo
            {
                Path = path.Pointer,
                Data = data,
                Size = size,
                Metadata = 0
            }, gameInfo, false);
            return new AtariLoadedContent(gameInfo, path, data);
        }
        catch
        {
            if (gameInfo != 0) Marshal.FreeHGlobal(gameInfo);
            if (data != 0) Marshal.FreeHGlobal(data);
            path.Dispose();
            throw;
        }
    }

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
