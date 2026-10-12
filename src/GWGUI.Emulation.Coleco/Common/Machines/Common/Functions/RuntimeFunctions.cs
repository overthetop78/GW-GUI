using System.Diagnostics;

namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Functions;

internal static class RuntimeFunctions
{
    internal static int RegionValue(EmulationVideoStandard? region) => region is null ? RuntimeConstants.MissingRegionValue : (int)region.Value;

    internal static EmulationVideoStandard? ReadRegion(int value) => value == RuntimeConstants.MissingRegionValue || value is not ((int)EmulationVideoStandard.Ntsc) and not ((int)EmulationVideoStandard.Pal) ? null : (EmulationVideoStandard)value;

    internal static HostProcessState ProcessState(Process? process, bool connectionFailed, bool disposed)
    {
        if (connectionFailed) return HostProcessState.Faulted;
        if (process is null) return HostProcessState.NotStarted;
        if (disposed) return HostProcessState.Exited;
        try { return process.HasExited ? HostProcessState.Exited : HostProcessState.Running; }
        catch (InvalidOperationException) { return HostProcessState.Exited; }
    }

    internal static RuntimeStatus Status(MachineConfiguration configuration, IEmulatorCore core)
    {
        var frame = core.LatestVideoFrame;
        var geometry = frame is null ? null : new RuntimeGeometry(frame.Width, frame.Height, frame.Pitch, frame.AspectRatio);
        return new RuntimeStatus(configuration.Model, core.Region, core.FramesPerSecond, core.SampleRate,
            geometry, core.CoreName, new Dictionary<EmulationMediaSlot, bool>(),
            new Dictionary<int, bool>(core.LedStates), core.BufferedAudioFrames, core.AudioOverrunCount,
            core.AudioUnderrunCount, core.HostProcessState, core.HostProcessId);
    }
}

internal static class MessageFunctions
{
    internal static Exception Translate(Exception error, MachineConfiguration configuration)
    {
        var context = new EmulationMachineMessageContext(configuration.MachineId);
        if (error is not EmulationException coreError)
            return EmulationErrorService.Translate(error, EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, context);
        if (coreError.IsLocalized)
            return EmulationErrorService.TranslateLocalized(error, Category(coreError.Category), context);
        if (coreError.Code == ErrorCode.ContentRequired)
            return new EmulationMessageException(new EmulationMessage(EmulationMessageCategory.Media,
                EmulationMessageCode.RequiredMediaMissing, EmulationMessageSeverity.Error,
                EmulationMessageTarget.Dialog,
                new EmulationRequiredMachineMediaMessageContext(configuration.MachineId, [EmulationMediaCategory.CartridgeSlot])), error);
        return EmulationErrorService.Translate(error, Category(coreError.Category), MessageCode(coreError.Code), context);
    }

    private static EmulationMessageCategory Category(ErrorCategory category) => category switch
    {
        ErrorCategory.Core or ErrorCategory.Host => EmulationMessageCategory.Emulator,
        ErrorCategory.Firmware => EmulationMessageCategory.Firmware,
        ErrorCategory.Content => EmulationMessageCategory.Media,
        ErrorCategory.State => EmulationMessageCategory.SavedState,
        _ => EmulationMessageCategory.Machine
    };

    private static EmulationMessageCode MessageCode(ErrorCode code) => code switch
    {
        ErrorCode.CoreNotFound => EmulationMessageCode.EmulatorNotInstalled,
        ErrorCode.CoreRejected => EmulationMessageCode.EmulatorRejected,
        ErrorCode.FirmwareMissing => EmulationMessageCode.FirmwareMissing,
        ErrorCode.FirmwareInvalid => EmulationMessageCode.FirmwareIncompatible,
        ErrorCode.ContentNotFound => EmulationMessageCode.MediaNotFound,
        ErrorCode.ContentUnsupported => EmulationMessageCode.MediaUnsupported,
        ErrorCode.OptionInvalid => EmulationMessageCode.OptionInvalid,
        ErrorCode.HostProtocolFailure => EmulationMessageCode.HostCommunicationFailed,
        ErrorCode.StateInvalid => EmulationMessageCode.SavedStateInvalid,
        ErrorCode.StateIncompatible => EmulationMessageCode.SavedStateIncompatible,
        _ => EmulationMessageCode.MachineStartFailed
    };
}
