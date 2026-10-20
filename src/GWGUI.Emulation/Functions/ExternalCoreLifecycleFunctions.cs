using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Functions;

internal static class ExternalCoreLifecycleFunctions
{

    internal static async ValueTask WaitForThreadAsync(Task? loop, Thread? thread)
    {
        try
        {
            if (loop is not null) await loop.ConfigureAwait(false);
        }
        finally { thread?.Join(); }
    }

    internal static bool TryDisposeCore(IDisposable core, Action<Exception> reportFailure)
    {
        try
        {
            core.Dispose();
            return true;
        }
        catch (Exception error)
        {
            reportFailure(error);
            return false;
        }
    }

    internal static bool IsSupportedStateSize(nuint size, nuint maximumStateSize)
        => size > nuint.Zero && size <= maximumStateSize;

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
