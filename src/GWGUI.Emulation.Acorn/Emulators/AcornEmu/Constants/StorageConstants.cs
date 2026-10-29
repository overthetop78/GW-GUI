using S = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants;
using GWGUI.Emulation.Acorn.Emulators.AcornEmu.Dictionaries;

namespace GWGUI.Emulation.Acorn.Emulators.AcornEmu.Constants;

/// <summary>Identifiants persistants des lecteurs de l'émulateur interne et choix des dialogues partagés.</summary>
internal static class StorageConstants
{
    /// <summary>Clé d’option indiquant si le périphérique est connecté.</summary>
    internal const string ConnectedOption = "connected";
    /// <summary>Clé d’option du modèle physique de lecteur.</summary>
    internal const string ModelOption = "model";
    /// <summary>Clé d’option de la vitesse de transfert du lecteur.</summary>
    internal const string SpeedOption = "speed";
    /// <summary>Clé d’option de la protection de l’image contre l’écriture.</summary>
    internal const string ProtectedOption = "protected";
    /// <summary>Clé d’option de la redirection des écritures sur une copie de l’image.</summary>
    internal const string RedirectOption = "redirect";
    /// <summary>Clé d’option de la latence de lecture.</summary>
    internal const string ReadLatencyOption = "readLatency";
    /// <summary>Clé d’option de la latence d’écriture.</summary>
    internal const string WriteLatencyOption = "writeLatency";
    /// <summary>Clé d’option de l’interface du périphérique de stockage.</summary>
    internal const string InterfaceOption = "interface";
    /// <summary>Valeur persistante de la vitesse de transfert normale à 100 %.</summary>
    internal const string DefaultSpeed = "100";
    /// <summary>Valeur persistante désignant le transfert à vitesse maximale.</summary>
    internal const string MaximumSpeed = "0";
    /// <summary>Clé de traduction de l’activation des bruitages mécaniques.</summary>
    internal const string SoundResource = "Emulation.Cassette.Settings.Sound";
    /// <summary>Clé de traduction du choix du profil de bruitages.</summary>
    internal const string SoundProfileResource = "Emulation.Storage.Sound.Profile";
    /// <summary>Clé de traduction de l’interface de stockage.</summary>
    internal const string InterfaceResource = "Emulation.Storage.Device.Interface";
    /// <summary>Clé de traduction de la vitesse de transfert.</summary>
    internal const string SpeedResource = "Emulation.Storage.Disk.TransferSpeed";
    /// <summary>Clé de traduction de la latence de lecture.</summary>
    internal const string ReadLatencyResource = "Emulation.Storage.Disk.ReadLatency";
    /// <summary>Clé de traduction de la latence d’écriture.</summary>
    internal const string WriteLatencyResource = "Emulation.Storage.Disk.WriteLatency";
    /// <summary>Clé de traduction de la protection en écriture.</summary>
    internal const string ProtectionResource = "Emulation.Storage.Floppy.WriteProtection";
    /// <summary>Clé de traduction du choix de vitesse maximale.</summary>
    internal const string MaximumSpeedResource = "Emulation.Value.Maximum";
    /// <summary>Nom invariant du contrôleur de disquettes Intel 8271.</summary>
    internal const string Intel8271Display = "Intel 8271";
    /// <summary>Nom invariant de l’interface de disquettes Acorn WD1770.</summary>
    internal const string Wd1770Display = "Acorn WD1770";
    /// <summary>Nom invariant de l’interface SCSI.</summary>
    internal const string ScsiDisplay = "SCSI";
    /// <summary>Suffixe affiché après une valeur exprimée en pourcentage.</summary>
    internal const string PercentSuffix = " %";
    /// <summary>Suffixe affiché après une durée exprimée en millisecondes.</summary>
    internal const string MillisecondsSuffix = "ms";
    /// <summary>Vitesse de transfert initiale, en pourcentage de la vitesse normale.</summary>
    internal const int DefaultSpeedPercent = 100;

    /// <summary>Latence initiale des accès au disque dur, en millisecondes.</summary>
    internal const int DefaultHardDiskLatencyMilliseconds = 2;

    /// <summary>Latence maximale configurable, en millisecondes.</summary>
    internal const int MaximumLatencyMilliseconds = 10000;
    internal static IReadOnlyList<string> HardDiskOptions { get; } = Array.AsReadOnly<string>([ProtectedOption, ReadLatencyOption, WriteLatencyOption, SpeedOption, MechanicalSoundOption, SoundVolumeOption]);
    /// <summary>Préfixe des options persistantes des lecteurs de disquettes AcornEmu.</summary>
    internal const string FloppyPrefix = "acornemu.floppy.";

    /// <summary>Identifiant de configuration de la ROM du système de fichiers DFS.</summary>
    internal const string DfsFirmware = "acornemu.dfs";
    /// <summary>Clé persistante du choix du contrôleur de disquettes.</summary>
    internal const string ControllerOption = FloppyPrefix + "controller";

    /// <summary>Identifiant persistant du contrôleur Intel 8271.</summary>
    internal const string Intel8271 = "i8271";

    /// <summary>Identifiant persistant du contrôleur Acorn WD1770.</summary>
    internal const string AcornWd1770 = "acorn-wd1770";
    /// <summary>Identifiant du lecteur de disquettes double face à 80 pistes sélectionné initialement.</summary>
    internal const string DefaultModel = "ds80";

    /// <summary>Identifiant du lecteur simple face à 40 pistes par face en simple densité.</summary>
    internal const string Single40 = "ss40";

    /// <summary>Identifiant du lecteur double face à 40 pistes par face en simple densité.</summary>
    internal const string Double40 = "ds40";

    /// <summary>Identifiant du lecteur simple face à 80 pistes par face en simple densité.</summary>
    internal const string Single80 = "ss80";

    /// <summary>Identifiant du lecteur double face à 80 pistes par face en simple densité.</summary>
    internal const string Double80 = "ds80";
    /// <summary>Identifiant du lecteur simple face à 40 pistes par face en double densité.</summary>
    internal const string Single40Dd = "ss40-dd";

    /// <summary>Identifiant du lecteur double face à 40 pistes par face en double densité.</summary>
    internal const string Double40Dd = "ds40-dd";

    /// <summary>Identifiant du lecteur simple face à 80 pistes par face en double densité.</summary>
    internal const string Single80Dd = "ss80-dd";

    /// <summary>Identifiant du lecteur double face à 80 pistes par face en double densité.</summary>
    internal const string Double80Dd = "ds80-dd";
    /// <summary>Clé persistante d’activation des bruitages mécaniques du lecteur.</summary>
    internal const string MechanicalSoundOption = "mechanicalSound";
    /// <summary>Clé persistante du profil de bruitages mécaniques du lecteur.</summary>
    internal const string SoundProfileOption = "soundProfile";
    /// <summary>Clé persistante du volume des bruitages mécaniques du lecteur.</summary>
    internal const string SoundVolumeOption = S.VolumeOption;
    /// <summary>Nombre de lecteurs de disquettes raccordables.</summary>
    internal const int DriveCount = 2;

    /// <summary>Indice de l’indicateur d’activité du second lecteur de disquettes.</summary>
    internal const int SecondDriveLed = 4;
    /// <summary>Préfixe des options persistantes des disques durs AcornEmu.</summary>
    internal const string HardDiskPrefix = "acornemu.hdd.";

    /// <summary>Identifiant persistant de l’interface SCSI.</summary>
    internal const string ScsiInterface = "scsi";

    /// <summary>Identifiant MediaEngine du format d’image Acorn Winchester.</summary>
    internal const string HardDiskFormat = "hdd.acorn-winchester";
    /// <summary>Nombre d’unités de disque dur prises en charge par le contrôleur SCSI.</summary>
    internal const int HardDiskCount = AcornScsiConstants.UnitCount;

    /// <summary>Premier indice des indicateurs d’activité des disques durs.</summary>
    internal const int HardDiskLedStart = 8;
    /// <summary>Capacité maximale d’une image de disque dur, en octets.</summary>
    internal static long MaximumHardDiskBytes => WinchesterModelConstants.MaximumBytes;
    internal static IReadOnlyList<string> HardDiskExtensions => [MediaConstants.WinchesterImageExtension];
    internal static IReadOnlyList<string> Extensions => FloppyDriveModels.Available(false).SelectMany(model => model.AcceptedExtensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    internal static IReadOnlyList<string> DoubleDensityExtensions => FloppyDriveModels.All.SelectMany(model => model.AcceptedExtensions).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    internal static IReadOnlyList<string> Speeds { get; } = Array.AsReadOnly<string>(["100", "200", "400", "800", "0"]);
    internal static IReadOnlyList<FloppyDriveModelChoice> Models => FloppyDriveModels.All;
    internal static string Key(int drive, string name) => FloppyPrefix + drive + "." + name;
    internal static string HardDiskKey(int unit, string name) => HardDiskPrefix + unit + "." + name;
}
