namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Exceptions;

internal static class MachineExceptions
{
    internal static EmulationMessageException SavedStateInvalid(Exception? innerException = null) =>
        Error(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, innerException);

    internal static EmulationMessageException SavedStateIncompatible(Exception? innerException = null) =>
        Error(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateIncompatible,
            innerException);

    internal static EmulationMessageException MediaNotFound(Exception? innerException = null) =>
        Error(EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound, innerException);

    internal static EmulationMessageException MachineNotRunning(Exception? innerException = null) =>
        Error(EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, innerException);

    private static EmulationMessageException Error(EmulationMessageCategory category,
        EmulationMessageCode code, Exception? innerException) =>
        new(new EmulationMessage(category, code, EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog), innerException);
}
