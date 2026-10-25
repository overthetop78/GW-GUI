namespace GWGUI.Emulation.Sony.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Clé de la ressource ArchiveMissingLibrary ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ArchiveMissingLibrary = "Emulation.Error.Core.ArchiveMissingLibrary";
    /// <summary>Clé de la ressource ContentRefused ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ContentRefused = "Emulation.Error.Core.ContentRefused";
    /// <summary>Clé de la ressource CoreNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotFound = "Emulation.Error.Core.CoreNotFound";
    /// <summary>Clé de la ressource CoreNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotInitialized = "Emulation.Error.Core.CoreNotInitialized";
    /// <summary>Clé de la ressource CoreNotLoaded ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotLoaded = "Emulation.Error.Core.CoreNotLoaded";
    /// <summary>Clé de la ressource CorePathNotAbsolute ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CorePathNotAbsolute = "Emulation.Error.Core.CorePathNotAbsolute";
    /// <summary>Clé de la ressource DiskControlIncomplete ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DiskControlIncomplete = "Emulation.Error.Core.DiskControlIncomplete";
    /// <summary>Clé de la ressource DiskControlUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DiskControlUnavailable = "Emulation.Error.Core.DiskControlUnavailable";
    /// <summary>Clé de la ressource DownloadedCoreInvalidPe ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreInvalidPe = "Emulation.Error.Core.DownloadedCoreInvalidPe";
    /// <summary>Clé de la ressource DownloadedCoreNotPe ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreNotPe = "Emulation.Error.Core.DownloadedCoreNotPe";
    /// <summary>Clé de la ressource DownloadedCoreWrongArchitecture ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string DownloadedCoreWrongArchitecture = "Emulation.Error.Core.DownloadedCoreWrongArchitecture";
    /// <summary>Clé de la ressource HostConfigurationInvalid ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostConfigurationInvalid = "Emulation.Error.Core.HostConfigurationInvalid";
    /// <summary>Clé de la ressource HostExecutableNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostExecutableNotFound = "Emulation.Error.Core.HostExecutableNotFound";
    /// <summary>Clé de la ressource HostNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostNotInitialized = "Emulation.Error.Core.HostNotInitialized";
    /// <summary>Clé de la ressource HostResponseUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostResponseUnavailable = "Emulation.Error.Core.HostResponseUnavailable";
    /// <summary>Clé de la ressource InvalidOptionValue ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidOptionValue = "Emulation.Error.Core.InvalidOptionValue";
    /// <summary>Clé de la ressource InvalidResponseLength ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidResponseLength = "Emulation.Error.Core.InvalidResponseLength";
    /// <summary>Clé de la ressource InvalidStateSize ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidStateSize = "Emulation.Error.Core.InvalidStateSize";
    /// <summary>Clé de la ressource LibraryIdentityMismatch ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string LibraryIdentityMismatch = "Emulation.Error.Core.LibraryIdentityMismatch";
    /// <summary>Clé de la ressource MediaEjectFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaEjectFailed = "Emulation.Error.Core.MediaEjectFailed";
    /// <summary>Clé de la ressource MediaInsertFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaInsertFailed = "Emulation.Error.Core.MediaInsertFailed";
    /// <summary>Clé de la ressource MediaNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaNotFound = "Emulation.Error.Core.MediaNotFound";
    /// <summary>Clé de la ressource MediaRefused ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaRefused = "Emulation.Error.Core.MediaRefused";
    /// <summary>Clé de la ressource MediaSelectionFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaSelectionFailed = "Emulation.Error.Core.MediaSelectionFailed";
    /// <summary>Clé de la ressource MediaSlotCreationFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string MediaSlotCreationFailed = "Emulation.Error.Core.MediaSlotCreationFailed";
    /// <summary>Clé de la ressource ProcessAlreadyInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessAlreadyInitialized = "Emulation.Error.Core.ProcessAlreadyInitialized";
    /// <summary>Clé de la ressource ProcessCommunicationFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessCommunicationFailed = "Emulation.Error.Core.ProcessCommunicationFailed";
    /// <summary>Clé de la ressource ProcessNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessNotInitialized = "Emulation.Error.Core.ProcessNotInitialized";
    /// <summary>Clé de la ressource ProcessStartFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessStartFailed = "Emulation.Error.Core.ProcessStartFailed";
    /// <summary>Clé de la ressource ProcessTimeout ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessTimeout = "Emulation.Error.Core.ProcessTimeout";
    /// <summary>Clé de la ressource ProcessUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ProcessUnavailable = "Emulation.Error.Core.ProcessUnavailable";
    /// <summary>Clé de la ressource RequestedDiskSelectionFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string RequestedDiskSelectionFailed = "Emulation.Error.Core.RequestedDiskSelectionFailed";
    /// <summary>Clé de la ressource RequestedMediaInsertFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string RequestedMediaInsertFailed = "Emulation.Error.Core.RequestedMediaInsertFailed";
    /// <summary>Clé de la ressource ResourceName ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string ResourceName = "GWGUI.Emulation.Sony.Resources.Error";
    /// <summary>Clé de la ressource StartWithoutMediaUnsupported ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StartWithoutMediaUnsupported = "Emulation.Error.Core.StartWithoutMediaUnsupported";
    /// <summary>Clé de la ressource StateEmpty ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateEmpty = "Emulation.Error.Core.StateEmpty";
    /// <summary>Clé de la ressource StateRestoreFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateRestoreFailed = "Emulation.Error.Core.StateRestoreFailed";
    /// <summary>Clé de la ressource StateSaveFailed ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string StateSaveFailed = "Emulation.Error.Core.StateSaveFailed";
    /// <summary>Clé de la ressource UnknownCoreOption ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownCoreOption = "Emulation.Error.Core.UnknownCoreOption";
    /// <summary>Clé de la ressource UnknownHostCommand ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownHostCommand = "Emulation.Error.Core.UnknownHostCommand";
    /// <summary>Clé de la ressource UnsupportedApiVersion ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedApiVersion = "Emulation.Error.Core.UnsupportedApiVersion";
    /// <summary>Clé de la ressource UnsupportedContentExtension ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedContentExtension = "Emulation.Error.Core.UnsupportedContentExtension";
    /// <summary>Clé de la ressource UnsupportedPixelFormat ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnsupportedPixelFormat = "Emulation.Error.Core.UnsupportedPixelFormat";
    /// <summary>Clé de la ressource VideoBufferUnavailable ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string VideoBufferUnavailable = "Emulation.Error.Core.VideoBufferUnavailable";
}
