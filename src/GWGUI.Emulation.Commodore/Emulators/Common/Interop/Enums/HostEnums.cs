namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Enums;

internal enum HostCommand : byte
{
    SaveMediaChanges = CoreHostConstants.SaveMediaChangesCommand,
    HasUnsavedMediaChanges = CoreHostConstants.HasUnsavedMediaChangesCommand,

    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose,
    /// <summary>Exécute la réinitialisation logicielle par le contrat commun.</summary>
    SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand,
    SoftReset = CoreHostConstants.SoftResetCommand,
    /// <summary>Consulte les images disponibles et leur sélection sans modifier le média.</summary>
    GetDiskStatus = CoreHostConstants.GetDiskStatusCommand
}
