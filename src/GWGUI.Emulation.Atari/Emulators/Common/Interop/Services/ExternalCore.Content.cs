using CommonCoreHostFunctions = GWGUI.Emulation.Atari.Common.Functions.CoreHostFunctions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
    private void ReplaceCartridge(MediaConfiguration media)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var exports = RequireExports();
        var cartridgeExtensions = _mediaAdapter.CartridgeExtensions ?? throw CartridgeExceptions.UnsupportedCore();
        var prepared = CartridgeFunctions.Prepare(configuration, media, Emulator, cartridgeExtensions, _info.NeedsFullPath, _info.Extensions);
        CartridgeFunctions.ValidateNoUnsupportedMetadata(media);
        foreach (var option in _mediaAdapter.PrepareOptions(CartridgeFunctions.GetMediaOptions(media, _mediaAdapter.SupportsCartridgeRegion)))
            RequireCallbacks().SetOption(option.Key, option.Value);
        var candidate = ContentFunctions.Create(prepared.RuntimePath, prepared.NeedsFullPath, _info.Extensions);
        var previousContent = _content;
        var transferred = false;
        try
        {
            if (_gameLoaded)
            {
                exports.UnloadGame();
                _gameLoaded = false;
            }
            if (!exports.LoadGame(candidate.GameInfo))
            {
                if (previousContent is null || !exports.LoadGame(previousContent.GameInfo))
                    throw CartridgeExceptions.RollbackFailed();
                _gameLoaded = true;
                throw CartridgeExceptions.ReplacementFailed();
            }
            _gameLoaded = true;
            _content = candidate;
            _cartridge = prepared;
            transferred = true;
            previousContent?.Dispose();
            _supportsSaveStates = StateFunctions.IsAvailable(exports);
            MediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
        }
        finally
        {
            if (!transferred) candidate.Dispose();
        }
    }

    private void ReplacePreparedContent(MediaConfiguration media)
    {
        var configuration = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var prepared = _mediaAdapter.PrepareContent(configuration, media, _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), _info) ?? throw new NotSupportedException(ErrorMessages.ContentLoadFailed);
        LoadedContent? candidate = null;
        var transferred = false;
        try
        {
            candidate = ContentFunctions.Create(prepared.RuntimePath, prepared.NeedsFullPath, _info.Extensions);
            var exports = RequireExports();
            var previousContent = _content;
            var previousPreparedContent = _preparedContent;
            if (_gameLoaded)
            {
                exports.UnloadGame();
                _gameLoaded = false;
            }
            if (!exports.LoadGame(candidate.GameInfo))
            {
                if (previousContent is not null && exports.LoadGame(previousContent.GameInfo))
                    _gameLoaded = true;
                throw _mediaAdapter.ContentLoadException(ErrorMessages.ContentLoadFailed);
            }
            _gameLoaded = true;
            _content = candidate;
            _preparedContent = prepared;
            _cartridge = null;
            transferred = true;
            try { previousContent?.Dispose(); }
            finally { _mediaAdapter.CleanupPreparedContent(previousPreparedContent); }
            _supportsSaveStates = StateFunctions.IsAvailable(exports);
            if (prepared.ActivityPaths is { } activityPaths) RequireCallbacks().TrackOpticalMedia(activityPaths);
            MediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
        }
        finally
        {
            if (!transferred)
            {
                try { candidate?.Dispose(); }
                finally { _mediaAdapter.CleanupPreparedContent(prepared); }
            }
        }
    }

    private ExternalCoreExports RequireExports() => _exports ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);

    private ExternalHostCallbacks RequireCallbacks() => _callbacks ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);

    private void DisposeNativeResources()
    {
        var exports = _exports;
        var callbacks = _callbacks;
        var library = _library;
        var content = _content;
        var preparedContent = _preparedContent;
        var gameLoaded = _gameLoaded;
        var nativeInitialized = _nativeInitialized;
        _exports = null;
        _callbacks = null;
        _library = null;
        _content = null;
        _preparedContent = null;
        _gameLoaded = false;
        _supportsSaveStates = false;
        _nativeInitialized = false;
        try
        {
            CommonCoreHostFunctions.MachineExecution.Cleanup(exports, gameLoaded, nativeInitialized, () => callbacks?.Dispose(), () => library?.Dispose());
        }
        finally
        {
            GC.KeepAlive(callbacks);
            try { content?.Dispose(); }
            finally
            {
                try { _mediaAdapter.CleanupPreparedContent(preparedContent); }
                finally
                {
                    _cartridge = null;
                    _mountedMedia.Clear();
                    _sessionMedia.Clear();
                    _configuration = null;
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeNativeResources();
    }
}
