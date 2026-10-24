namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Enums;

internal enum HostCommand : byte
{
    SaveMediaChanges = CoreHostConstants.SaveMediaChangesCommand,
    HasUnsavedMediaChanges = CoreHostConstants.HasUnsavedMediaChangesCommand,

    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose, SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand,
    SoftReset = CoreHostConstants.SoftResetCommand,
    /// <summary>Consulte les images disponibles et leur sélection sans modifier le média.</summary>
    GetDiskStatus = CoreHostConstants.GetDiskStatusCommand
}


