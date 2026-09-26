namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

internal static class CoreLifecycleFunctions
{
    internal static void Load(ExternalCoreExports exports, ExternalHostCallbacks callbacks,
        MachineConfiguration configuration, nint gameInfo)
    {
        if (gameInfo == nint.Zero && !callbacks.SupportsNoGame)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentRequired,
                ErrorMessages.ContentRequired);
        if (!exports.LoadGame(gameInfo))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported,
                ErrorMessages.ContentLoadFailed);
        callbacks.ConfigureInput(configuration.Input);
        ControllerPortFunctions.Configure(exports, callbacks, configuration);
        exports.GetSystemAvInfo(out var avInfo);
        callbacks.ApplySystemAvInfo(avInfo);
    }

    internal static void Cleanup(ExternalCoreExports? exports, bool gameLoaded, bool initialized,
        Action disposeCallbacks, Action disposeLibrary)
    {
        if (gameLoaded)
            exports?.UnloadGame();
        if (initialized)
            exports?.Deinitialize();
        disposeCallbacks();
        disposeLibrary();
    }
}
