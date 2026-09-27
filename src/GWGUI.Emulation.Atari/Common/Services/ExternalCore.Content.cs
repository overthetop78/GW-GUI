using System.Runtime.InteropServices;
using System.Security.Cryptography;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class ExternalCore : IEmulatorCore
{
private void ReplaceCartridge(MediaConfiguration media)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var exports = RequireExports();
        var cartridgeExtensions = _mediaAdapter.CartridgeExtensions ??
            throw CartridgeExceptions.UnsupportedCore();
        var prepared = CartridgeFunctions.Prepare(configuration, media, Emulator, cartridgeExtensions,
            _info.NeedsFullPath, _info.Extensions);
        CartridgeFunctions.ValidateNoUnsupportedMetadata(media);
        foreach (var option in _mediaAdapter.PrepareOptions(
                     CartridgeFunctions.GetMediaOptions(media, _mediaAdapter.SupportsCartridgeRegion)))
            RequireCallbacks().SetOption(option.Key, option.Value);
        var candidate = ContentFunctions.Create(prepared.RuntimePath,
            prepared.NeedsFullPath, _info.Extensions);
        var previousContent = _content;
        if (_gameLoaded)
        {
            exports.UnloadGame();
            _gameLoaded = false;
        }

        if (!exports.LoadGame(candidate.GameInfo))
        {
            candidate.Dispose();
            if (previousContent is null || !exports.LoadGame(previousContent.GameInfo))
                throw CartridgeExceptions.RollbackFailed();
            _gameLoaded = true;
            throw CartridgeExceptions.ReplacementFailed();
        }

        _gameLoaded = true;
        _supportsSaveStates = StateFunctions.IsAvailable(exports);
        previousContent?.Dispose();
        _content = candidate;
        _cartridge = prepared;
        MediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
    }

    private void ReplacePreparedContent(MediaConfiguration media)
    {
        var configuration = _configuration ??
            throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        var prepared = _mediaAdapter.PrepareContent(configuration, media,
            _sessionDirectory ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized), _info)
            ?? throw new NotSupportedException(ErrorMessages.ContentLoadFailed);
        var candidate = ContentFunctions.Create(prepared.RuntimePath,
            prepared.NeedsFullPath, _info.Extensions);
        var exports = RequireExports();
        var previousContent = _content;
        if (_gameLoaded)
        {
            exports.UnloadGame();
            _gameLoaded = false;
        }
        if (!exports.LoadGame(candidate.GameInfo))
        {
            candidate.Dispose();
            if (previousContent is not null && exports.LoadGame(previousContent.GameInfo))
                _gameLoaded = true;
            throw _mediaAdapter.ContentLoadException(ErrorMessages.ContentLoadFailed);
        }
        _gameLoaded = true;
        _supportsSaveStates = StateFunctions.IsAvailable(exports);
        previousContent?.Dispose();
        _content = candidate;
        _preparedContent = prepared;
        if (prepared.ActivityPaths is { } activityPaths) RequireCallbacks().TrackOpticalMedia(activityPaths);
        _cartridge = null;
        MediaRuntimeFunctions.Register(_mountedMedia, media with { IsInserted = true });
    }

    private ExternalCoreExports RequireExports() => _exports ??
        throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);

    private ExternalHostCallbacks RequireCallbacks() => _callbacks ??
        throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);

    private void DisposeNativeResources()
    {
        var exports = _exports;
        var callbacks = _callbacks;
        var library = _library;
        CoreLifecycleFunctions.Cleanup(exports, _gameLoaded, _nativeInitialized,
            () => callbacks?.Dispose(), () => library?.Dispose());
        _gameLoaded = false;
        _supportsSaveStates = false;
        _nativeInitialized = false;
        _content?.Dispose();
        _content = null;
        _mediaAdapter.CleanupPreparedContent(_preparedContent);
        _preparedContent = null;
        _cartridge = null;
        _mountedMedia.Clear();
        _sessionMedia.Clear();
        _configuration = null;
        _callbacks = null;
        _exports = null;
        _library = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeNativeResources();
    }
}
