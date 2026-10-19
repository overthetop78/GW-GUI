using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Functions;

internal static class ExternalCoreLifecycleFunctions
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
