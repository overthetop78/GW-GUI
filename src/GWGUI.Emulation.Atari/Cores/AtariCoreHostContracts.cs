namespace GWGUI.Emulation.Atari.Cores;

internal enum AtariHostCommand : byte
{
    Initialize = AtariCoreHostConstants.InitializeCommand,
    RunFrame = AtariCoreHostConstants.RunFrameCommand,
    HardReset = AtariCoreHostConstants.HardResetCommand,
    Stop = AtariCoreHostConstants.StopCommand,
    InsertMedia = AtariCoreHostConstants.InsertMediaCommand,
    EjectMedia = AtariCoreHostConstants.EjectMediaCommand,
    SaveState = AtariCoreHostConstants.SaveStateCommand,
    LoadState = AtariCoreHostConstants.LoadStateCommand,
    SetOption = AtariCoreHostConstants.SetOptionCommand,
    SelectDisk = AtariCoreHostConstants.SelectDiskCommand,
    Dispose = AtariCoreHostConstants.DisposeCommand,
    SaveMediaChanges = AtariCoreHostConstants.SaveMediaChangesCommand,
    GetDiskStatus = AtariCoreHostConstants.GetDiskStatusCommand,
    HasUnsavedMediaChanges = AtariCoreHostConstants.HasUnsavedMediaChangesCommand,
    SetControllerPortDevice = AtariCoreHostConstants.SetControllerPortDeviceCommand
}

internal enum AtariHostResponseStatus : byte
{
    Success = AtariCoreHostConstants.SuccessResponse,
    Failure = AtariCoreHostConstants.FailureResponse
}

internal sealed record AtariHostError(
    string Type,
    string Message,
    AtariErrorKind? Kind,
    AtariErrorCode? Code,
    IReadOnlyDictionary<string, string> Context);
