namespace GWGUI.Emulation.Fairchild.Common.Enums;

internal enum HostCommand : byte
{
    Initialize = CoreHostConstants.InitializeCommand,
    RunFrame = CoreHostConstants.RunFrameCommand,
    HardReset = CoreHostConstants.HardResetCommand,
    Stop = CoreHostConstants.StopCommand,
    InsertMedia = CoreHostConstants.InsertMediaCommand,
    EjectMedia = CoreHostConstants.EjectMediaCommand,
    SaveState = CoreHostConstants.SaveStateCommand,
    LoadState = CoreHostConstants.LoadStateCommand,
    SetOption = CoreHostConstants.SetOptionCommand,
    SelectDisk = CoreHostConstants.SelectDiskCommand,
    Dispose = CoreHostConstants.DisposeCommand,
    SaveMediaChanges = CoreHostConstants.SaveMediaChangesCommand,
    GetDiskStatus = CoreHostConstants.GetDiskStatusCommand,
    HasUnsavedMediaChanges = CoreHostConstants.HasUnsavedMediaChangesCommand,
    SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand,
    /// <summary>Exécute la réinitialisation logicielle par le contrat commun.</summary>
    SoftReset = CoreHostConstants.SoftResetCommand
}


internal enum HostResponseStatus : byte
{
    Success = CoreHostConstants.SuccessResponse,
    Failure = CoreHostConstants.FailureResponse
}

public enum ErrorCategory
{
    Core,
    Firmware,
    Content,
    Option,
    Host,
    State
}

public enum ErrorCode
{
    CoreNotFound,
    CoreRejected,
    FirmwareMissing,
    FirmwareInvalid,
    ContentNotFound,
    ContentRequired,
    ContentUnsupported,
    OptionInvalid,
    HostProtocolFailure,
    StateInvalid,
    StateIncompatible
}
