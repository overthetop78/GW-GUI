using CommonCoreHostFunctions = GWGUI.Emulation.Apple.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;
namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

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
            CommonCoreHostFunctions.MachineExecution.Cleanup(exports, gameLoaded, initialized, () => callbacks?.Dispose(), () => library?.Dispose());
        }
        finally
        {
            try { content?.Dispose(); }
            finally
            {
                _sessionMedia.Clear();
                _initialMedia = null;
                _configuration = null;
                _sessionDirectory = null;
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
