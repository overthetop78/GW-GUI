namespace GWGUI.Emulation.Atari.Emulators.Libretro.Enums;

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
    SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand
}

public enum HostProcessState { InProcess, NotStarted, Running, Exited, Faulted }

internal enum HostResponseStatus : byte
{
    Success = CoreHostConstants.SuccessResponse,
    Failure = CoreHostConstants.FailureResponse
}
