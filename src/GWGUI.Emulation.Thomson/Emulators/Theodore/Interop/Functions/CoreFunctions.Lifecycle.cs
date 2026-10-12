using GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Contracts;

namespace GWGUI.Emulation.Thomson.Emulators.Theodore.Interop.Functions;

internal static class CoreLifecycleFunctions
{
    internal static void Cleanup(ExternalCoreExports? exports, bool gameLoaded, bool initialized, Action disposeCallbacks, Action disposeLibrary)
    {
        try
        {
            if (gameLoaded) exports?.UnloadGame();
        }
        finally
        {
            try
            {
                if (initialized) exports?.Deinitialize();
            }
            finally
            {
                try { disposeCallbacks(); }
                finally { disposeLibrary(); }
            }
        }
    }
}
