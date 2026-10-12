using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;

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
        var subsystem = _subsystem;
        var initialized = _nativeInitialized;
        var gameLoaded = _gameLoaded;
        _exports = null;
        _callbacks = null;
        _library = null;
        _content = null;
        _subsystem = null;
        _nativeInitialized = false;
        _gameLoaded = false;
        _supportsSaveStates = false;
        try
        {
            try
            {
                if (gameLoaded && _batteryMemoryReady) WriteBatteryMemory(exports!);
            }
            finally
            {
                CoreLifecycleFunctions.Cleanup(exports, gameLoaded, initialized, () => callbacks?.Dispose(), () => library?.Dispose());
            }
        }
        finally
        {
            try { content?.Dispose(); }
            finally
            {
                try { subsystem?.Dispose(); }
                finally
                {
                    _batteryMemoryReady = false;
                    _batterySavePath = null;
                    _configuration = null;
                    _sessionDirectory = null;
                    _saveDirectory = null;
                    Region = null;
                }
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
