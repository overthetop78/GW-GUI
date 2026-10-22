using CommonCoreHostFunctions = GWGUI.Emulation.Nintendo.Common.Functions.CoreHostFunctions;
namespace GWGUI.Emulation.Nintendo.Common.Machines.Common.Exceptions;

internal static class MachineExceptions
{
    internal static EmulationMessageException SavedStateInvalid(Exception? innerException = null) => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, innerException);

    internal static EmulationMessageException SavedStateIncompatible(Exception? innerException = null) => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateIncompatible, innerException);

    internal static EmulationMessageException MediaNotFound(Exception? innerException = null) => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound, innerException);

    internal static EmulationMessageException MachineNotRunning(Exception? innerException = null) => CommonCoreHostFunctions.Messages.CreateError(EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, innerException);


}
