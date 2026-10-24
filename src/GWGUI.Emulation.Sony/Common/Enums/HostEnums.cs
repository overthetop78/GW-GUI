namespace GWGUI.Emulation.Sony.Common.Enums;

internal enum HostCommand : byte
{
    SaveMediaChanges = CoreHostConstants.SaveMediaChangesCommand,
    HasUnsavedMediaChanges = CoreHostConstants.HasUnsavedMediaChangesCommand,

    Initialize = 1, RunFrame, HardReset, Stop, InsertMedia, EjectMedia,
    SaveState, LoadState, SetOption, SelectDisk, Dispose, SoftReset = CoreHostConstants.SoftResetCommand, SetControllerPortDevice = CoreHostConstants.SetControllerPortDeviceCommand,
    /// <summary>Consulte les images disponibles et leur sélection sans modifier le média.</summary>
    GetDiskStatus = CoreHostConstants.GetDiskStatusCommand
}



/// <summary>Statut précédant les données d’une réponse du protocole d’hôte.</summary>
internal enum HostResponseStatus : byte
{
    /// <summary>L’opération a réussi ; la suite contient le résultat.</summary>
    Success = CoreHostConstants.SuccessResponse,
    /// <summary>L’opération a échoué ; la suite contient le diagnostic.</summary>
    Failure = CoreHostConstants.FailureResponse
}