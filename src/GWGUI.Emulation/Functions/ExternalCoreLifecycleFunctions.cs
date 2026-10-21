using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Functions;

/// <summary>Opérations de cycle de vie communes aux modules d’émulation indépendants.</summary>
public static class ExternalCoreLifecycleFunctions
{
    internal static void DisposeCallbackResources(IDisposable? systemDirectory, IDisposable? contentDirectory,
        IDisposable? saveDirectory, IDisposable? assetsDirectory, IDisposable? virtualFileSystem,
        IDisposable? videoBuffers, IDisposable optionHost)
    {
        using (systemDirectory)
        using (contentDirectory)
        using (saveDirectory)
        using (assetsDirectory)
        using (virtualFileSystem)
        using (videoBuffers)
            optionHost.Dispose();
    }

    /// <summary>Démarre la boucle possédée par la machine et fournit les signaux de démarrage et de fin.</summary>
    /// <remarks>La boucle signale elle-même son démarrage. Son propriétaire doit demander l’arrêt puis attendre le thread avant de libérer ses ressources.</remarks>
    public static void StartThread(Action<CancellationToken> run, string name,
        CancellationToken cancellationToken, ref CancellationTokenSource? stop,
        out TaskCompletionSource started, out Task? loop, ref Thread? thread)
    {
        var previousStop = stop;
        var previousThread = thread;
        stop = null;
        started = null!;
        loop = null;
        thread = null;
        TaskCompletionSource? completed = null;
        var threadStarted = false;
        try
        {
            try { previousThread?.Join(); }
            finally { previousStop?.Dispose(); }
            var stopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            stop = stopSource;
            started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            completed = completion;
            thread = new Thread(() =>
            {
                try { run(stopSource.Token); }
                finally { completion.TrySetResult(); }
            })
            {
                IsBackground = true,
                Name = name
            };
            loop = completion.Task;
            thread.Start();
            threadStarted = true;
        }
        finally
        {
            if (!threadStarted)
            {
                thread = null;
                completed?.TrySetResult();
                try { stop?.Dispose(); }
                finally { stop = null; }
            }
        }
    }

    /// <summary>Exécute l’initialisation et la boucle du cœur, avec traitement de l’échec et nettoyage garanti.</summary>
    /// <remarks>Le nettoyage reçoit la réussite de l’initialisation et s’exécute aussi après une erreur.</remarks>
    public static void RunCore(Action initialize, Action run,
        Func<Exception, Action> selectFailureHandler, Action<bool> cleanup)
    {
        var initialized = false;
        Action? failureHandler = null;
        try
        {
            initialize();
            initialized = true;
            run();
        }
        catch (Exception error) when ((failureHandler = selectFailureHandler(error)) is not null)
        {
            failureHandler();
        }
        finally { cleanup(initialized); }
    }

    /// <summary>Attend l’arrêt puis libère, dans leur ordre, toutes les ressources confiées par la machine.</summary>
    /// <remarks>Chaque libération reste exécutée même si l’arrêt ou une libération précédente échoue.</remarks>
    public static async ValueTask DisposeMachineAsync(Func<ValueTask> stop,
        Action disposeFirst, Action disposeSecond, Action disposeThird, Action cleanup)
    {
        try { await stop().ConfigureAwait(false); }
        finally
        {
            try { disposeFirst(); }
            finally
            {
                try { disposeSecond(); }
                finally
                {
                    try { disposeThird(); }
                    finally { cleanup(); }
                }
            }
        }
    }

    /// <summary>Attend le démarrage et arrête la machine si cette attente échoue ou est annulée.</summary>
    public static async ValueTask WaitForStartOrStopAsync(Task started,
        CancellationToken cancellationToken, Func<ValueTask> stop)
    {
        var completedStart = false;
        try
        {
            await started.WaitAsync(cancellationToken).ConfigureAwait(false);
            completedStart = true;
        }
        finally
        {
            if (!completedStart) await stop().ConfigureAwait(false);
        }
    }

    /// <summary>Attend la fin de la boucle puis la terminaison effective de son thread, y compris après une erreur.</summary>
    public static async ValueTask WaitForThreadAsync(Task? loop, Thread? thread)
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
