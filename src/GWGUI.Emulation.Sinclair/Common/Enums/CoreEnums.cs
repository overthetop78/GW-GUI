namespace GWGUI.Emulation.Sinclair.Common.Enums;

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
    GetDiskStatus = CoreHostConstants.GetDiskStatusCommand,
    SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand,
    /// <summary>Exécute la réinitialisation logicielle par le contrat commun.</summary>
    SoftReset = CoreHostConstants.SoftResetCommand
}

public enum HostProcessState { InProcess, NotStarted, Running, Exited, Faulted }

internal enum HostResponseStatus : byte
{
    Success = CoreHostConstants.SuccessResponse,
    Failure = CoreHostConstants.FailureResponse
}
