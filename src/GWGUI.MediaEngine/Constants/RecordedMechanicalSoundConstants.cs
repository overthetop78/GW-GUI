namespace GWGUI.MediaEngine.Constants;

/// <summary>Ressources et niveaux du lecteur d’enregistrements mécaniques.</summary>
internal static class RecordedMechanicalSoundConstants
{
    /// <summary>Nom du dossier racine des fichiers externes de l’application.</summary>
    internal const string AssetsDirectory = "Assets";

    /// <summary>Nom du sous-dossier contenant la banque de bruitages mécaniques.</summary>
    internal const string BankDirectory = "MechanicalAudio";

    /// <summary>Nom du fichier JSON décrivant les profils et leurs enregistrements.</summary>
    internal const string CatalogFileName = "catalog.json";
    /// <summary>Identifiant de l’opération de démarrage du moteur.</summary>
    internal const string Start = "Start";
    /// <summary>Profil Alps Electric DF354H148F utilisé en l’absence de son spécifique de disquette.</summary>
    internal const string DefaultFloppyProfile = MechanicalSoundConstants.Floppy35;
    /// <summary>Profil Seagate ST3120A utilisé en l’absence de son spécifique de disque dur.</summary>
    internal const string DefaultHardDiskProfile = MechanicalSoundConstants.HardDisk35;
    /// <summary>Profil du lecteur optique à tiroir disponible, utilisé en l’absence de son spécifique.</summary>
    internal const string DefaultOpticalProfile = MechanicalSoundConstants.OpticalTray;
    /// <summary>Valeur maximale du réglage de volume, en pourcentage.</summary>
    internal const double MaximumVolumePercent = 100;

    /// <summary>Valeur initiale du réglage de volume, en pourcentage.</summary>
    internal const double DefaultVolumePercent = 50;
    /// <summary>Amplitude positive maximale du PCM signé sur 16 bits, utilisée pour convertir les échantillons flottants.</summary>
    internal const double PcmScale = 32767;
    /// <summary>Gain de chaque canal lors du mélange stéréo vers mono.</summary>
    internal const double StereoChannelGain = 0.5;
    /// <summary>Nombre d’échantillons flottants du tampon de décodage audio.</summary>
    internal const int ReadBufferLength = 8192;
}
