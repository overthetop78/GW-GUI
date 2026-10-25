namespace GWGUI.Emulation.Amstrad.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Clé de la ressource ArchiveMissingLibrary ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ArchiveMissingLibrary = "Emulation.Error.Common.ArchiveMissingLibrary";
    /// <summary>Clé de la ressource ContentRefused ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ContentRefused = "Emulation.Error.Common.ContentRefused";
    /// <summary>Clé de la ressource CoreNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotFound = "Emulation.Error.Common.CoreNotFound";
    /// <summary>Clé de la ressource CoreNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotInitialized = "Emulation.Error.Common.CoreNotInitialized";
    /// <summary>Clé de la ressource CoreNotLoaded ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotLoaded = "Emulation.Error.Common.CoreNotLoaded";
    /// <summary>Clé de la ressource CorePathNotAbsolute ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CorePathNotAbsolute = "Emulation.Error.Common.CorePathNotAbsolute";
    /// <summary>Clé de la ressource DiskControlIncomplete ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DiskControlIncomplete = "Emulation.Error.Common.DiskControlIncomplete";
    /// <summary>Clé de la ressource DiskControlUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DiskControlUnavailable = "Emulation.Error.Common.DiskControlUnavailable";
    /// <summary>Clé de la ressource DownloadedCoreInvalidPe ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreInvalidPe = "Emulation.Error.Common.DownloadedCoreInvalidPe";
    /// <summary>Clé de la ressource DownloadedCoreNotPe ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreNotPe = "Emulation.Error.Common.DownloadedCoreNotPe";
    /// <summary>Clé de la ressource DownloadedCoreWrongArchitecture ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreWrongArchitecture = "Emulation.Error.Common.DownloadedCoreWrongArchitecture";
    /// <summary>Clé de la ressource HostConfigurationInvalid ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostConfigurationInvalid = "Emulation.Error.Common.HostConfigurationInvalid";
    /// <summary>Clé de la ressource HostExecutableNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostExecutableNotFound = "Emulation.Error.Common.HostExecutableNotFound";
    /// <summary>Clé de la ressource HostNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostNotInitialized = "Emulation.Error.Common.HostNotInitialized";
    /// <summary>Clé de la ressource HostResponseUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostResponseUnavailable = "Emulation.Error.Common.HostResponseUnavailable";
    /// <summary>Clé de la ressource InvalidOptionValue ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidOptionValue = "Emulation.Error.Common.InvalidOptionValue";
    /// <summary>Clé de la ressource InvalidResponseLength ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidResponseLength = "Emulation.Error.Common.InvalidResponseLength";
    /// <summary>Clé de la ressource InvalidStateSize ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidStateSize = "Emulation.Error.Common.InvalidStateSize";
    /// <summary>Clé de la ressource LibraryIdentityMismatch ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string LibraryIdentityMismatch = "Emulation.Error.Common.LibraryIdentityMismatch";
    /// <summary>Clé de la ressource MediaEjectFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaEjectFailed = "Emulation.Error.Common.MediaEjectFailed";
    /// <summary>Clé de la ressource MediaInsertFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaInsertFailed = "Emulation.Error.Common.MediaInsertFailed";
    /// <summary>Clé de la ressource MediaNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaNotFound = "Emulation.Error.Common.MediaNotFound";
    /// <summary>Clé de la ressource MediaRefused ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaRefused = "Emulation.Error.Common.MediaRefused";
    /// <summary>Clé de la ressource MediaSelectionFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaSelectionFailed = "Emulation.Error.Common.MediaSelectionFailed";
    /// <summary>Clé de la ressource MediaSlotCreationFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaSlotCreationFailed = "Emulation.Error.Common.MediaSlotCreationFailed";
    /// <summary>Clé de la ressource PlaylistLimitExceeded ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string PlaylistLimitExceeded = "Emulation.Error.Common.PlaylistLimitExceeded";
    /// <summary>Clé de la ressource ProcessAlreadyInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessAlreadyInitialized = "Emulation.Error.Common.ProcessAlreadyInitialized";
    /// <summary>Clé de la ressource ProcessCommunicationFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessCommunicationFailed = "Emulation.Error.Common.ProcessCommunicationFailed";
    /// <summary>Clé de la ressource ProcessNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessNotInitialized = "Emulation.Error.Common.ProcessNotInitialized";
    /// <summary>Clé de la ressource ProcessStartFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessStartFailed = "Emulation.Error.Common.ProcessStartFailed";
    /// <summary>Clé de la ressource ProcessTimeout ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessTimeout = "Emulation.Error.Common.ProcessTimeout";
    /// <summary>Clé de la ressource ProcessUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessUnavailable = "Emulation.Error.Common.ProcessUnavailable";
    /// <summary>Clé de la ressource RequestedDiskSelectionFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string RequestedDiskSelectionFailed = "Emulation.Error.Common.RequestedDiskSelectionFailed";
    /// <summary>Clé de la ressource RequestedMediaInsertFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string RequestedMediaInsertFailed = "Emulation.Error.Common.RequestedMediaInsertFailed";
    /// <summary>Clé de la ressource ResourceName ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ResourceName = "GWGUI.Emulation.Amstrad.Resources.Emulation";
    /// <summary>Clé de la ressource StartWithoutMediaUnsupported ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StartWithoutMediaUnsupported = "Emulation.Error.Common.StartWithoutMediaUnsupported";
    /// <summary>Clé de la ressource StateEmpty ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateEmpty = "Emulation.Error.Common.StateEmpty";
    /// <summary>Clé de la ressource StateRestoreFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateRestoreFailed = "Emulation.Error.Common.StateRestoreFailed";
    /// <summary>Clé de la ressource StateSaveFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateSaveFailed = "Emulation.Error.Common.StateSaveFailed";
    /// <summary>Clé de la ressource UnknownCoreOption ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownCoreOption = "Emulation.Error.Common.UnknownCoreOption";
    /// <summary>Clé de la ressource UnknownHostCommand ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownHostCommand = "Emulation.Error.Common.UnknownHostCommand";
    /// <summary>Clé de la ressource UnsupportedApiVersion ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedApiVersion = "Emulation.Error.Common.UnsupportedApiVersion";
    /// <summary>Clé de la ressource UnsupportedContentExtension ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedContentExtension = "Emulation.Error.Common.UnsupportedContentExtension";
    /// <summary>Clé de la ressource UnsupportedPixelFormat ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedPixelFormat = "Emulation.Error.Common.UnsupportedPixelFormat";
    /// <summary>Clé de la ressource VideoBufferUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string VideoBufferUnavailable = "Emulation.Error.Common.VideoBufferUnavailable";
}
