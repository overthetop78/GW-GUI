using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Contracts;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;
using GWGUI.Emulation.Amiga.Emulators.PUAE.Services;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;

internal static class ProcessCoreConstants
{
    internal const string TheAmigaCoreProcessIsAlreadyInitialized = "The Amiga core process is already initialized.";
    internal const string TheGWGUIExecutableUsedToHostTheAmigaCoreWasNotFound = "The GW GUI executable used to host the Amiga core was not found.";
    internal const string CoreHost = "--amiga-core-host";
    internal const string TheAmigaCoreHostProcessCouldNotBeStarted = "The Amiga core host process could not be started.";
    internal const string TheSharedAmigaVideoBufferIsUnavailable = "The shared Amiga video buffer is unavailable.";
    internal const string TheAmigaCoreProcessIsNoLongerAvailable = "The Amiga core process is no longer available.";
    internal const string TheAmigaCoreProcessIsNotInitialized = "The Amiga core process is not initialized.";
    internal const string TheAmigaCoreProcessDidNotAnswerWithin30SecondsAndWasStopped = "The Amiga core process did not answer within 30 seconds and was stopped.";
    internal const string TheAmigaHostResponseIsUnavailable = "The Amiga host response is unavailable.";
}
