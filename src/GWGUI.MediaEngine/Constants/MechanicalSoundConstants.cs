namespace GWGUI.MediaEngine.Constants;

/// <summary>Identifiants et paramètres communs de synthèse mécanique ; aucune dépendance à un émulateur.</summary>
public static class MechanicalSoundConstants
{
    /// <summary>Identifiant de la catégorie des lecteurs de bande.</summary>
    public const string Cassette = "Cassette";

    /// <summary>Identifiant de la catégorie des lecteurs de disquettes.</summary>
    public const string Floppy = "Floppy";

    /// <summary>Identifiant de la catégorie des disques durs.</summary>
    public const string HardDisk = "HardDisk";

    /// <summary>Identifiant de la catégorie des lecteurs optiques.</summary>
    public const string Optical = "Optical";
    /// <summary>Identifiant de l’absence d’opération mécanique.</summary>
    public const string None = "None";

    /// <summary>Identifiant de la lecture d’une bande.</summary>
    public const string Play = "Play";

    /// <summary>Identifiant de l’enregistrement sur bande.</summary>
    public const string Record = "Record";

    /// <summary>Identifiant de l’avance rapide d’une bande.</summary>
    public const string FastForward = "FastForward";

    /// <summary>Identifiant du rembobinage d’une bande.</summary>
    public const string Rewind = "Rewind";

    /// <summary>Identifiant de l’arrêt du mécanisme.</summary>
    public const string Stop = "Stop";

    /// <summary>Identifiant de l’éjection du support.</summary>
    public const string Eject = "Eject";

    /// <summary>Identifiant de la rotation continue du moteur.</summary>
    public const string Motor = "Motor";

    /// <summary>Identifiant de l’activité mécanique de lecture.</summary>
    public const string Read = "Read";

    /// <summary>Identifiant de l’activité mécanique d’écriture.</summary>
    public const string Write = "Write";

    /// <summary>Identifiant du déplacement des têtes.</summary>
    public const string Seek = "Seek";

    /// <summary>Identifiant de l’insertion du support.</summary>
    public const string Insert = "Insert";
    /// <summary>Identifiant du profil synthétique standard de lecteur de bande.</summary>
    public const string Standard = "Standard";

    /// <summary>Identifiant du profil de disquette 5,25 pouces, associé aux enregistrements Panasonic JU-475-5.</summary>
    public const string Floppy525 = "Floppy525";

    /// <summary>Identifiant du profil de disquette 3,5 pouces, associé aux enregistrements Alps Electric DF354H148F.</summary>
    public const string Floppy35 = "Floppy35";

    /// <summary>Identifiant du profil Winchester, associé aux enregistrements Miniscribe 8425.</summary>
    public const string Winchester = "Winchester";

    /// <summary>Identifiant du profil de disque dur 3,5 pouces, associé aux enregistrements Seagate ST3120A.</summary>
    public const string HardDisk35 = "HardDisk35";

    /// <summary>Identifiant du profil générique de disque dur 2,5 pouces.</summary>
    public const string HardDisk25 = "HardDisk25";

    /// <summary>Identifiant du profil de lecteur optique à tiroir.</summary>
    public const string OpticalTray = "OpticalTray";

    /// <summary>Identifiant du profil générique de lecteur optique à chargement par fente.</summary>
    public const string OpticalSlot = "OpticalSlot";
    /// <summary>Libellé invariant du format physique de 5,25 pouces.</summary>
    internal const string Label525 = "5.25″";

    /// <summary>Libellé invariant du format physique de 3,5 pouces.</summary>
    internal const string Label35 = "3.5″";

    /// <summary>Libellé invariant du format physique de 2,5 pouces.</summary>
    internal const string Label25 = "2.5″";
    /// <summary>Multiplicateur du générateur déterministe de bruit de synthèse.</summary>
    internal const long NoiseMultiplier = 1664525;

    /// <summary>Incrément du générateur déterministe de bruit de synthèse.</summary>
    internal const long NoiseIncrement = 1013904223;
    /// <summary>Nombre de bits décalés pour extraire le bruit pseudo-aléatoire.</summary>
    internal const int NoiseShift = 16;
    /// <summary>Diviseur appliqué aux bits extraits pour mettre à l’échelle le bruit de synthèse.</summary>
    internal const double NoiseDivisor = 32768.0;
    /// <summary>Fréquence de la composante tonale synthétique d’avance rapide, en hertz.</summary>
    internal const double CassetteForwardFrequency = 190;

    /// <summary>Fréquence de la composante tonale synthétique de rembobinage, en hertz.</summary>
    internal const double CassetteRewindFrequency = 165;
    /// <summary>Durée du transitoire synthétique d’arrêt de bande, en secondes.</summary>
    internal const double CassetteStopSeconds = 0.05;

    /// <summary>Durée du transitoire synthétique d’éjection de bande, en secondes.</summary>
    internal const double CassetteEjectSeconds = 0.12;

    /// <summary>Amplitude de référence du transitoire synthétique de bande, en unités PCM.</summary>
    internal const double CassetteTransientLevel = 2200;

    /// <summary>Coefficient de bruit du transitoire synthétique de bande.</summary>
    internal const double CassetteTransientNoise = 0.3;
    /// <summary>Durée de référence d’un déplacement synthétique des têtes de disquette, en secondes.</summary>
    internal const double FloppySeekSeconds = 0.025;

    /// <summary>Durée de référence des autres transitoires synthétiques de disquette, en secondes.</summary>
    internal const double FloppyActionSeconds = 0.12;

    /// <summary>Amplitude du bruit transitoire synthétique de disquette, en unités PCM.</summary>
    internal const double FloppyTransientLevel = 1600;

    /// <summary>Fréquence de la composante tonale transitoire synthétique de disquette, en hertz.</summary>
    internal const double FloppyTransientFrequency = 950;

    /// <summary>Amplitude de la composante tonale transitoire synthétique de disquette, en unités PCM.</summary>
    internal const double FloppyTransientTone = 500;
    /// <summary>Durée de référence d’un déplacement synthétique des têtes de disque dur, en secondes.</summary>
    internal const double HardDiskSeekSeconds = 0.04;

    /// <summary>Durée de référence de l’enveloppe synthétique d’arrêt de disque dur, en secondes.</summary>
    internal const double HardDiskStopSeconds = 0.15;

    /// <summary>Amplitude du bruit synthétique de déplacement des têtes de disque dur, en unités PCM.</summary>
    internal const double HardDiskTransientLevel = 1200;

    /// <summary>Fréquence de la composante tonale synthétique de déplacement du disque dur, en hertz.</summary>
    internal const double HardDiskTransientFrequency = 650;

    /// <summary>Amplitude de la composante tonale synthétique de déplacement du disque dur, en unités PCM.</summary>
    internal const double HardDiskTransientTone = 450;
    /// <summary>Durée de référence d’un déplacement synthétique du lecteur optique, en secondes.</summary>
    internal const double OpticalSeekSeconds = 0.06;

    /// <summary>Durée de référence des autres transitoires synthétiques du lecteur optique à tiroir, en secondes.</summary>
    internal const double OpticalActionSeconds = 0.45;

    /// <summary>Durée de référence des autres transitoires synthétiques du lecteur optique à fente, en secondes.</summary>
    internal const double OpticalSlotActionSeconds = 0.7;

    /// <summary>Amplitude du bruit transitoire synthétique du lecteur optique, en unités PCM.</summary>
    internal const double OpticalTransientLevel = 850;

    /// <summary>Fréquence de la composante tonale transitoire synthétique du lecteur optique, en hertz.</summary>
    internal const double OpticalTransientFrequency = 480;

    /// <summary>Amplitude de la composante tonale transitoire synthétique du lecteur optique, en unités PCM.</summary>
    internal const double OpticalTransientTone = 350;
    /// <summary>Volume mécanique initial, en pourcentage.</summary>
    public const int DefaultVolumePercent = 50;

    /// <summary>Valeur maximale du réglage de volume mécanique, en pourcentage.</summary>
    public const int MaximumVolumePercent = 100;
    /// <summary>Clé d’option persistante du volume des bruitages mécaniques.</summary>
    public const string VolumeOption = "soundVolume";

    /// <summary>Clé de traduction du libellé du volume mécanique.</summary>
    public const string VolumeResource = "Emulation.Storage.Sound.Volume";
    /// <summary>Nombre maximal d’événements dans la file générique de bruitages mécaniques.</summary>
    public const int EventQueueCapacity = 32;

    /// <summary>Nombre maximal d’événements dans une file de bruitages de disque dur.</summary>
    public const int HardDiskEventQueueCapacity = 16;
    /// <summary>Cadence de référence des déplacements de disquette, en événements par seconde.</summary>
    public const int FloppySeekEventsPerSecond = 40;

    /// <summary>Durée de référence d’une action de disquette, en centièmes de seconde.</summary>
    public const int FloppyActionHundredths = 12;

    /// <summary>Nombre de centièmes dans une seconde.</summary>
    public const int HundredthsPerSecond = 100;

    /// <summary>Cadence de référence des déplacements de disque dur, en événements par seconde.</summary>
    public const int HardDiskSeekEventsPerSecond = 25;
}
