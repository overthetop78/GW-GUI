using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;

internal sealed class LoadedContent : IDisposable
{
    private IEmulationLoadedContent? _content;
    private readonly string _contentFilePath;
    private SessionContentDirectory? _contentDirectory;

    private LoadedContent(IEmulationLoadedContent content, string contentFilePath)
    {
        _content = content;
        _contentFilePath = contentFilePath;
    }

    internal nint GameInfo => _content?.GameInfo ?? nint.Zero;
    internal string? ContentFilePath => _content is null ? null : _contentFilePath;

    internal static LoadedContent Create(string absolutePath, bool needsFullPath, bool useWindowsAnsiPath)
    {
        var content = CommonCoreHostFunctions.NativeResources.CreateLoadedContent(absolutePath, needsFullPath, useWindowsAnsiPath);
        var transferred = false;
        try
        {
            var result = new LoadedContent(content, absolutePath);
            transferred = true;
            return result;
        }
        finally { if (!transferred) content.Dispose(); }
    }

    internal void OwnContentDirectory(SessionContentDirectory directory)
    {
        if (GameInfo == nint.Zero || _contentDirectory is not null) throw new InvalidOperationException(nameof(OwnContentDirectory));
        _contentDirectory = directory;
    }

    public void Dispose()
    {
        var content = _content;
        var contentDirectory = _contentDirectory;
        _content = null;
        _contentDirectory = null;
        try { content?.Dispose(); }
        finally { contentDirectory?.Dispose(); }
    }
}
