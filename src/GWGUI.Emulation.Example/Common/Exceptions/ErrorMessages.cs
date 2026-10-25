namespace GWGUI.Emulation.Example.Common.Exceptions;

/// <summary>Résolution des messages de validation, de contenu, de cœur et de sauvegarde du module.</summary>
/// <remarks>Chaque propriété décrit une situation technique ; plusieurs situations partagent un même texte
/// traduit de catégorie. Les détails comme le chemin et la valeur attendue restent dans le contexte de l’erreur.
/// La résolution se fait à chaque accès avec la culture d’interface courante, sans figer un texte lors du chargement.</remarks>
internal static class ErrorMessages
{
    /// <summary>Message associé à un répertoire d’installation non renseigné.</summary>
    internal static string EmptyInstallationRoot => CoreNotFound();
    /// <summary>Message associé à une version de cœur non renseignée.</summary>
    internal static string EmptyVersion => CoreNotFound();
    /// <summary>Message associé à une identité de cœur déclarée plusieurs fois.</summary>
    internal static string DuplicateCore => CoreRejected();
    /// <summary>Message associé à une association de modèle déclarée plusieurs fois.</summary>
    internal static string DuplicateModel => CoreRejected();
    /// <summary>Message associé à un modèle absent du catalogue d’adaptateurs.</summary>
    internal static string MissingModel => CoreNotFound();
    /// <summary>Message associé à une identité de firmware non reconnue.</summary>
    internal static string UnknownFirmware => FirmwareInvalid();
    /// <summary>Message associé à un firmware obligatoire non sélectionné.</summary>
    internal static string RequiredFirmwareMissing => FirmwareMissing();
    /// <summary>Message associé à un fichier de firmware absent.</summary>
    internal static string FirmwareFileMissing => FirmwareMissing();
    /// <summary>Message associé à un fichier de firmware impossible à lire.</summary>
    internal static string FirmwareFileUnreadable => FirmwareInvalid();
    /// <summary>Message associé à un firmware pouvant correspondre à plusieurs identités.</summary>
    internal static string FirmwareIdentityAmbiguous => FirmwareInvalid();
    /// <summary>Message associé à un candidat de firmware qui ne peut pas être sélectionné.</summary>
    internal static string FirmwareCannotBeSelected => FirmwareInvalid();
    /// <summary>Message associé à une identité de modèle matériel déclarée plusieurs fois.</summary>
    internal static string DuplicateHardwareModelDefinition => OptionInvalid();
    /// <summary>Message associé à un modèle matériel absent du catalogue.</summary>
    internal static string UnknownHardwareModel => OptionInvalid();
    /// <summary>Message associé à une déclaration de compatibilité présente plusieurs fois.</summary>
    internal static string DuplicateCompatibilityDefinition => OptionInvalid();
    /// <summary>Message associé à un modèle inconnu dans une déclaration de compatibilité.</summary>
    internal static string UnknownCompatibilityModel => OptionInvalid();
    /// <summary>Message associé à une capacité indisponible dépourvue d’explication.</summary>
    internal static string MissingUnavailableExplanation => OptionInvalid();
    /// <summary>Message associé à un nombre de contrôleurs incompatible avec la déclaration du modèle.</summary>
    internal static string InvalidCompatibilityControllerCount => OptionInvalid();
    /// <summary>Message associé à une version de schéma de configuration non prise en charge.</summary>
    internal static string UnsupportedSchema => OptionInvalid();
    /// <summary>Message associé à un chemin de firmware vide.</summary>
    internal static string EmptyFirmwarePath => FirmwareMissing();
    /// <summary>Message associé à plusieurs firmwares déclarés pour un même emplacement.</summary>
    internal static string DuplicateFirmware => FirmwareInvalid();
    /// <summary>Message associé à un firmware incompatible avec le modèle ou son emplacement.</summary>
    internal static string IncompatibleFirmware => FirmwareInvalid();
    /// <summary>Message associé à un chemin de média vide.</summary>
    internal static string EmptyMediaPath => ContentNotFound();
    /// <summary>Message associé à plusieurs montages déclarés pour le même lecteur.</summary>
    internal static string DuplicateMediaSlot => ContentUnsupported();
    /// <summary>Message associé à un média incompatible avec le lecteur ou le modèle.</summary>
    internal static string IncompatibleMedia => ContentUnsupported();
    /// <summary>Message associé à un indice de port de contrôleur hors limites.</summary>
    internal static string InvalidControllerPort => OptionInvalid();
    /// <summary>Message associé à une catégorie de périphérique non acceptée sur le port.</summary>
    internal static string UnsupportedControllerDevice => OptionInvalid();
    /// <summary>Message associé à plusieurs associations déclarées pour un même port.</summary>
    internal static string DuplicateControllerPort => OptionInvalid();
    /// <summary>Message associé à une zone morte de contrôleur hors de la plage admise.</summary>
    internal static string InvalidControllerDeadZone => OptionInvalid();
    /// <summary>Message associé à un chemin relatif de cœur lorsqu’un chemin absolu est requis.</summary>
    internal static string CorePathMustBeAbsolute => CoreNotFound();
    /// <summary>Message associé à une bibliothèque ou un exécutable de cœur absent.</summary>
    internal static string CoreFileMissing => CoreNotFound();
    /// <summary>Message associé à une fonction native requise absente des exports du cœur.</summary>
    internal static string CoreExportMissing => CoreRejected();
    /// <summary>Message associé à une version d’API native non acceptée.</summary>
    internal static string CoreApiVersionUnsupported => CoreRejected();
    /// <summary>Message associé à une identité de cœur différente de celle attendue.</summary>
    internal static string CoreIdentityMismatch => CoreRejected();
    /// <summary>Message associé à un fichier de contenu absent.</summary>
    internal static string ContentFileMissing => ContentNotFound();
    /// <summary>Message associé à une extension de contenu non prise en charge.</summary>
    internal static string ContentExtensionUnsupported => ContentUnsupported();
    /// <summary>Texte utilisé lors du formatage d’un diagnostic de valeur d’option invalide.</summary>
    internal static string OptionValueInvalidFormat => OptionInvalid();
    /// <summary>Message associé à une seconde initialisation du même cœur.</summary>
    internal static string CoreAlreadyInitialized => CoreRejected();
    /// <summary>Message associé à une opération effectuée avant initialisation du cœur.</summary>
    internal static string CoreNotInitialized => CoreRejected();
    /// <summary>Message associé à un refus du contenu lors de son chargement par le cœur.</summary>
    internal static string ContentLoadFailed => ContentUnsupported();
    /// <summary>Message associé à l’absence de contenu lorsque le cœur exige un média au démarrage.</summary>
    internal static string ContentRequired => ContentUnsupported();
    /// <summary>Message associé à un échange de média non pris en charge pendant la session.</summary>
    internal static string DynamicMediaUnsupported => ContentUnsupported();
    /// <summary>Message associé à une extension de stockage non admise par le lecteur.</summary>
    internal static string StorageExtensionInvalid => ContentUnsupported();
    /// <summary>Message associé à une sauvegarde d’état dont le format ou les données sont invalides.</summary>
    internal static string StateInvalid => StateInvalidText();
    /// <summary>Message associé à une taille de sauvegarde d’état invalide.</summary>
    internal static string StateSizeInvalid => StateInvalidText();
    /// <summary>Message associé à une demande d’état lorsque le cœur ne fournit pas cette capacité.</summary>
    internal static string StateUnavailable => StateInvalidText();
    /// <summary>Message associé à un échec de production ou d’écriture d’une sauvegarde d’état.</summary>
    internal static string StateSaveFailed => StateInvalidText();
    /// <summary>Message associé à un échec de restauration d’une sauvegarde d’état.</summary>
    internal static string StateLoadFailed => StateInvalidText();
    /// <summary>Message associé à une identité de sauvegarde incompatible avec la machine courante.</summary>
    internal static string StateIncompatible => ExceptionText.Get(CoreErrorResourceKeys.StateIncompatible);
    /// <summary>Message associé à une commande incompatible avec l’état courant de la machine.</summary>
    internal static string MachineInvalidState => ExceptionText.Get(CoreErrorResourceKeys.Unexpected);
    /// <summary>Message associé à une commande qui ne peut plus être exécutée après l’arrêt de la session.</summary>
    internal static string MachineStopped => ExceptionText.Get(CoreErrorResourceKeys.Unexpected);

    /// <summary>Résout le message de catégorie indiquant l’absence d’un firmware.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string FirmwareMissing() => ExceptionText.Get(CoreErrorResourceKeys.FirmwareMissing);
    /// <summary>Résout le message de catégorie indiquant un firmware invalide.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string FirmwareInvalid() => ExceptionText.Get(CoreErrorResourceKeys.FirmwareInvalid);
    /// <summary>Résout le message de catégorie indiquant un contenu introuvable.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string ContentNotFound() => ExceptionText.Get(CoreErrorResourceKeys.ContentNotFound);
    /// <summary>Résout le message de catégorie indiquant un contenu non pris en charge.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string ContentUnsupported() => ExceptionText.Get(CoreErrorResourceKeys.ContentUnsupported);
    /// <summary>Résout le message de catégorie indiquant un réglage invalide.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string OptionInvalid() => ExceptionText.Get(CoreErrorResourceKeys.OptionInvalid);
    /// <summary>Résout le message de catégorie indiquant un cœur introuvable.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string CoreNotFound() => ExceptionText.Get(CoreErrorResourceKeys.CoreNotFound);
    /// <summary>Résout le message de catégorie indiquant un cœur refusé.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string CoreRejected() => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    /// <summary>Résout le message de catégorie indiquant un état invalide.</summary>
    /// <returns>Texte résolu par le service de messages pour cette catégorie d&apos;erreur et la culture courante.</returns>
    private static string StateInvalidText() => ExceptionText.Get(CoreErrorResourceKeys.StateInvalid);

}

