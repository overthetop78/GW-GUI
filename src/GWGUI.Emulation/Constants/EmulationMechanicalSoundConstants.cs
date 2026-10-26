using M = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.Emulation.Constants;

/// <summary>Identifiants du service média relayés au SDK ; les définitions et profils appartiennent à MediaEngine.</summary>
public static class EmulationMechanicalSoundConstants
{
    /// <summary>Identifiant de la catégorie des lecteurs de bande.</summary>
    public const string Cassette = M.Cassette;
    /// <summary>Identifiant de la catégorie des lecteurs de disquettes.</summary>
    public const string Floppy = M.Floppy;
    /// <summary>Identifiant de la catégorie des disques durs.</summary>
    public const string HardDisk = M.HardDisk;
    /// <summary>Identifiant de la catégorie des lecteurs optiques.</summary>
    public const string Optical = M.Optical;
    /// <summary>Identifiant de l’absence d’opération mécanique.</summary>
    public const string None = M.None;
    /// <summary>Identifiant de la lecture d’une bande.</summary>
    public const string Play = M.Play;
    /// <summary>Identifiant de l’enregistrement sur bande.</summary>
    public const string Record = M.Record;
    /// <summary>Identifiant de l’avance rapide d’une bande.</summary>
    public const string FastForward = M.FastForward;
    /// <summary>Identifiant du rembobinage d’une bande.</summary>
    public const string Rewind = M.Rewind;
    /// <summary>Identifiant de l’arrêt du mécanisme.</summary>
    public const string Stop = M.Stop;
    /// <summary>Identifiant de la commande de pause du magnétophone.</summary>
    public const string Pause = M.Pause;
    /// <summary>Identifiant de l’éjection du support.</summary>
    public const string Eject = M.Eject;
    /// <summary>Identifiant de la rotation continue du moteur.</summary>
    public const string Motor = M.Motor;
    /// <summary>Identifiant de l’activité mécanique de lecture.</summary>
    public const string Read = M.Read;
    /// <summary>Identifiant de l’activité mécanique d’écriture.</summary>
    public const string Write = M.Write;
    /// <summary>Identifiant du déplacement des têtes.</summary>
    public const string Seek = M.Seek;
    /// <summary>Identifiant de l’insertion du support.</summary>
    public const string Insert = M.Insert;
    /// <summary>Identifiant du profil synthétique standard de lecteur de bande.</summary>
    public const string Standard = M.Standard;
    /// <summary>Identifiant du profil de disquette 5,25 pouces, associé aux enregistrements Panasonic JU-475-5.</summary>
    public const string Floppy525 = M.Floppy525;
    /// <summary>Identifiant du profil de disquette 3,5 pouces, associé aux enregistrements Alps Electric DF354H148F.</summary>
    public const string Floppy35 = M.Floppy35;
    /// <summary>Identifiant du profil Winchester, associé aux enregistrements Miniscribe 8425.</summary>
    public const string Winchester = M.Winchester;
    /// <summary>Identifiant du profil de disque dur 3,5 pouces, associé aux enregistrements Seagate ST3120A.</summary>
    public const string HardDisk35 = M.HardDisk35;
    /// <summary>Identifiant du profil générique de disque dur 2,5 pouces.</summary>
    public const string HardDisk25 = M.HardDisk25;
    /// <summary>Identifiant du profil de lecteur optique à tiroir.</summary>
    public const string OpticalTray = M.OpticalTray;
    /// <summary>Identifiant du profil générique de lecteur optique à chargement par fente.</summary>
    public const string OpticalSlot = M.OpticalSlot;
    /// <summary>Volume mécanique initial, en pourcentage.</summary>
    public const int DefaultVolumePercent = M.DefaultVolumePercent;

    /// <summary>Valeur maximale du réglage de volume mécanique, en pourcentage.</summary>
    public const int MaximumVolumePercent = M.MaximumVolumePercent;
    /// <summary>Clé d’option persistante du volume des bruitages mécaniques.</summary>
    public const string VolumeOption = M.VolumeOption;

    /// <summary>Clé de traduction du libellé du volume mécanique.</summary>
    public const string VolumeResource = M.VolumeResource;
    /// <summary>Nombre maximal d’événements dans la file générique de bruitages mécaniques.</summary>
    public const int EventQueueCapacity = M.EventQueueCapacity;
    /// <summary>Nombre maximal d’événements dans une file de bruitages de disque dur.</summary>
    public const int HardDiskEventQueueCapacity = M.HardDiskEventQueueCapacity;
    /// <summary>Cadence de référence des déplacements de disquette, en événements par seconde.</summary>
    public const int FloppySeekEventsPerSecond = M.FloppySeekEventsPerSecond;
    /// <summary>Durée de référence d’une action de disquette, en centièmes de seconde.</summary>
    public const int FloppyActionHundredths = M.FloppyActionHundredths;
    /// <summary>Nombre de centièmes dans une seconde.</summary>
    public const int HundredthsPerSecond = M.HundredthsPerSecond;
    /// <summary>Cadence de référence des déplacements de disque dur, en événements par seconde.</summary>
    public const int HardDiskSeekEventsPerSecond = M.HardDiskSeekEventsPerSecond;
}
