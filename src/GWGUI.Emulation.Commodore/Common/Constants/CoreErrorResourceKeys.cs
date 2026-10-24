namespace GWGUI.Emulation.Commodore.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Clé de la ressource ArchiveMissingLibrary ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ArchiveMissingLibrary = "Emulation.Error.PUAE.ArchiveMissingLibrary";
    /// <summary>Clé de la ressource ContentRefused ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ContentRefused = "Emulation.Error.PUAE.ContentRefused";
    /// <summary>Clé de la ressource CoreNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotFound = "Emulation.Error.PUAE.CoreNotFound";
    /// <summary>Clé de la ressource CoreNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotInitialized = "Emulation.Error.PUAE.CoreNotInitialized";
    /// <summary>Clé de la ressource CoreNotLoaded ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotLoaded = "Emulation.Error.PUAE.CoreNotLoaded";
    /// <summary>Clé de la ressource CorePathNotAbsolute ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CorePathNotAbsolute = "Emulation.Error.PUAE.CorePathNotAbsolute";
    /// <summary>Clé de la ressource DiskControlIncomplete ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DiskControlIncomplete = "Emulation.Error.PUAE.DiskControlIncomplete";
    /// <summary>Clé de la ressource DiskControlUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DiskControlUnavailable = "Emulation.Error.PUAE.DiskControlUnavailable";
    /// <summary>Clé de la ressource DownloadedCoreInvalidPe ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreInvalidPe = "Emulation.Error.PUAE.DownloadedCoreInvalidPe";
    /// <summary>Clé de la ressource DownloadedCoreNotPe ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreNotPe = "Emulation.Error.PUAE.DownloadedCoreNotPe";
    /// <summary>Clé de la ressource DownloadedCoreWrongArchitecture ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreWrongArchitecture = "Emulation.Error.PUAE.DownloadedCoreWrongArchitecture";
    /// <summary>Clé de la ressource FullContentPathsRequired ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string FullContentPathsRequired = "Emulation.Error.PUAE.FullContentPathsRequired";
    /// <summary>Clé de la ressource HostConfigurationInvalid ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostConfigurationInvalid = "Emulation.Error.PUAE.HostConfigurationInvalid";
    /// <summary>Clé de la ressource HostExecutableNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostExecutableNotFound = "Emulation.Error.PUAE.HostExecutableNotFound";
    /// <summary>Clé de la ressource HostNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostNotInitialized = "Emulation.Error.PUAE.HostNotInitialized";
    /// <summary>Clé de la ressource HostResponseUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostResponseUnavailable = "Emulation.Error.PUAE.HostResponseUnavailable";
    /// <summary>Clé de la ressource InvalidOptionValue ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidOptionValue = "Emulation.Error.PUAE.InvalidOptionValue";
    /// <summary>Clé de la ressource InvalidResponseLength ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidResponseLength = "Emulation.Error.PUAE.InvalidResponseLength";
    /// <summary>Clé de la ressource InvalidStateSize ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidStateSize = "Emulation.Error.PUAE.InvalidStateSize";
    /// <summary>Clé de la ressource LibraryIdentityMismatch ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string LibraryIdentityMismatch = "Emulation.Error.PUAE.LibraryIdentityMismatch";
    /// <summary>Clé de la ressource MediaEjectFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaEjectFailed = "Emulation.Error.PUAE.MediaEjectFailed";
    /// <summary>Clé de la ressource MediaInsertFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaInsertFailed = "Emulation.Error.PUAE.MediaInsertFailed";
    /// <summary>Clé de la ressource MediaNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaNotFound = "Emulation.Error.PUAE.MediaNotFound";
    /// <summary>Clé de la ressource MediaRefused ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaRefused = "Emulation.Error.PUAE.MediaRefused";
    /// <summary>Clé de la ressource MediaSelectionFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaSelectionFailed = "Emulation.Error.PUAE.MediaSelectionFailed";
    /// <summary>Clé de la ressource MediaSlotCreationFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaSlotCreationFailed = "Emulation.Error.PUAE.MediaSlotCreationFailed";
    /// <summary>Clé de la ressource ProcessAlreadyInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessAlreadyInitialized = "Emulation.Error.PUAE.ProcessAlreadyInitialized";
    /// <summary>Clé de la ressource ProcessCommunicationFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessCommunicationFailed = "Emulation.Error.PUAE.ProcessCommunicationFailed";
    /// <summary>Clé de la ressource ProcessNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessNotInitialized = "Emulation.Error.PUAE.ProcessNotInitialized";
    /// <summary>Clé de la ressource ProcessStartFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessStartFailed = "Emulation.Error.PUAE.ProcessStartFailed";
    /// <summary>Clé de la ressource ProcessTimeout ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessTimeout = "Emulation.Error.PUAE.ProcessTimeout";
    /// <summary>Clé de la ressource ProcessUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessUnavailable = "Emulation.Error.PUAE.ProcessUnavailable";
    /// <summary>Clé de la ressource RequestedDiskSelectionFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string RequestedDiskSelectionFailed = "Emulation.Error.PUAE.RequestedDiskSelectionFailed";
    /// <summary>Clé de la ressource RequestedMediaInsertFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string RequestedMediaInsertFailed = "Emulation.Error.PUAE.RequestedMediaInsertFailed";
    /// <summary>Clé de la ressource StartWithoutMediaUnsupported ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StartWithoutMediaUnsupported = "Emulation.Error.PUAE.StartWithoutMediaUnsupported";
    /// <summary>Clé de la ressource StateEmpty ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateEmpty = "Emulation.Error.PUAE.StateEmpty";
    /// <summary>Clé de la ressource StateRestoreFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateRestoreFailed = "Emulation.Error.PUAE.StateRestoreFailed";
    /// <summary>Clé de la ressource StateSaveFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateSaveFailed = "Emulation.Error.PUAE.StateSaveFailed";
    /// <summary>Clé de la ressource UnknownCoreOption ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownCoreOption = "Emulation.Error.PUAE.UnknownCoreOption";
    /// <summary>Clé de la ressource UnknownHostCommand ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownHostCommand = "Emulation.Error.PUAE.UnknownHostCommand";
    /// <summary>Clé de la ressource UnsupportedApiVersion ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedApiVersion = "Emulation.Error.PUAE.UnsupportedApiVersion";
    /// <summary>Clé de la ressource UnsupportedContentExtension ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedContentExtension = "Emulation.Error.PUAE.UnsupportedContentExtension";
    /// <summary>Clé de la ressource UnsupportedController ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedController = "Emulation.Error.PUAE.UnsupportedController";
    /// <summary>Clé de la ressource UnsupportedHardDiskExtension ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedHardDiskExtension = "Emulation.Error.PUAE.UnsupportedHardDiskExtension";
    /// <summary>Clé de la ressource UnsupportedPixelFormat ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedPixelFormat = "Emulation.Error.PUAE.UnsupportedPixelFormat";
    /// <summary>Clé de la ressource VideoBufferUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string VideoBufferUnavailable = "Emulation.Error.PUAE.VideoBufferUnavailable";
}
