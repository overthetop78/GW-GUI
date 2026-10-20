namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Exceptions;

internal static class MachineExceptions
{
    internal static EmulationMessageException SavedStateInvalid(Exception? innerException = null) => EmulationErrorService.CreateError(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, innerException);

    internal static EmulationMessageException SavedStateIncompatible(Exception? innerException = null) => EmulationErrorService.CreateError(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateIncompatible, innerException);

    internal static EmulationMessageException MediaNotFound(Exception? innerException = null) => EmulationErrorService.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound, innerException);

    internal static EmulationMessageException MachineNotRunning(Exception? innerException = null) => EmulationErrorService.CreateError(EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, innerException);


}
