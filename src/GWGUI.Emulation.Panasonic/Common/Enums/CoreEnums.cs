namespace GWGUI.Emulation.Panasonic.Common.Enums;

internal enum HostCommand : byte { Initialize = 1, RunFrame = 2, HardReset = 3, Stop = 4, InsertMedia = 5, EjectMedia = 6, SaveState = 7, LoadState = 8, SetOption = 9, SelectDisk = 10, Dispose = 11, GetDiskStatus = 13, SetControllerPortDevice = 15,
    /// <summary>Exécute la réinitialisation logicielle par le contrat commun.</summary>
    SoftReset = CoreHostConstants.SoftResetCommand
}
internal enum HostResponseStatus : byte { Success = 1, Failure = 2 }
