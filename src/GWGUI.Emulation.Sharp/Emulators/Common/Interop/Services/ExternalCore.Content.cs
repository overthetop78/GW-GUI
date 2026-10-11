namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private ExternalCoreExports RequireExports() => _exports ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
    private ExternalHostCallbacks RequireCallbacks() => _callbacks ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);

    private void DisposeNativeResources()
    {
        var exports = _exports;
        var callbacks = _callbacks;
        var library = _library;
        var content = _content;
        var initialized = _nativeInitialized;
        var gameLoaded = _gameLoaded;
        _exports = null;
        _callbacks = null;
        _library = null;
        _content = null;
        _nativeInitialized = false;
        _gameLoaded = false;
        _supportsSaveStates = false;
        try
        {
            CoreLifecycleFunctions.Cleanup(exports, gameLoaded, initialized, () => callbacks?.Dispose(), () => library?.Dispose());
        }
        finally
        {
            try { content?.Dispose(); }
            finally
            {
                _configuration = null;
                _sessionDirectory = null;
                _saveDirectory = null;
                Region = null;
            }
        }
    }

    public void Stop() => DisposeNativeResources();
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        DisposeNativeResources();
    }
}
