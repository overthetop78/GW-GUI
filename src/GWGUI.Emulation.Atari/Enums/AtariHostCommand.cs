namespace GWGUI.Emulation.Atari.Enums;

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
