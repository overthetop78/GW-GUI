using System.Runtime.ExceptionServices;

namespace GWGUI.App.Functions.Rendering.Emulation;

internal static class EmulationVideoCleanupFunctions
{
    internal static void RunAll(params Action[] actions)
    {
        Exception? failure = null;
        foreach (var action in actions)
        {
            try { action(); }
            catch (Exception error) { failure ??= error; }
        }
        if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
    }

    internal static void DisposeAll(IEnumerable<IDisposable?> resources) => RunAll(resources.Select(resource => (Action)(() => resource?.Dispose())).ToArray());
}
