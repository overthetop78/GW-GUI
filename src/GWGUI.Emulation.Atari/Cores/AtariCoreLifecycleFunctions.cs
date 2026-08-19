namespace GWGUI.Emulation.Atari.Cores;

internal static class AtariCoreLifecycleFunctions
{
    internal static void Load(AtariExternalCoreExports exports, AtariExternalHostCallbacks callbacks,
        AtariMachineConfiguration configuration, nint gameInfo)
    {
        if (gameInfo == nint.Zero && !callbacks.SupportsNoGame)
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentRequired,
                AtariErrorMessages.ContentRequired);
        if (!exports.LoadGame(gameInfo))
            throw new AtariEmulationException(AtariErrorKind.Content, AtariErrorCode.ContentUnsupported,
                AtariErrorMessages.ContentLoadFailed);
        callbacks.ConfigureInput(configuration.Input);
        AtariControllerPortFunctions.Configure(exports, callbacks, configuration);
        exports.GetSystemAvInfo(out var avInfo);
        callbacks.ApplySystemAvInfo(avInfo);
    }

    internal static void Cleanup(AtariExternalCoreExports? exports, bool gameLoaded, bool initialized,
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
