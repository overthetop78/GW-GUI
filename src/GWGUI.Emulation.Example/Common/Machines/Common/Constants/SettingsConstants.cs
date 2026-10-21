namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Identifiants de réglages, clés de libellés et unités utilisés pour décrire la configuration.</summary>
/// <remarks>Les identifiants techniques relient les éditeurs aux valeurs appliquées par Common.
/// Les clés de ressources servent à obtenir les libellés dans la langue de l’hôte.
/// Les deux usages restent distincts : changer la traduction ne change pas l’identité du réglage enregistré.</remarks>
internal static class SettingsConstants
{
    /// <summary>Identifiant du réglage décrivant le processeur du modèle.</summary>
    internal const string Cpu = "example_cpu";
    /// <summary>Identifiant du réglage décrivant la fréquence du processeur.</summary>
    internal const string CpuFrequency = "example_cpu_frequency";
    /// <summary>Identifiant du réglage décrivant la capacité de mémoire principale.</summary>
    internal const string MainMemory = "example_main_memory";
    /// <summary>Identifiant du réglage présentant le firmware de la machine.</summary>
    internal const string Firmware = "example_firmware";
    /// <summary>Identifiant du réglage présentant les capacités vidéo du modèle.</summary>
    internal const string Video = "example_video";
    /// <summary>Identifiant de l’activation de la sortie audio.</summary>
    internal const string AudioEnabled = "example_audio_enabled";
    /// <summary>Identifiant du choix de périphérique de sortie audio transmis aux services d’exécution.</summary>
    internal const string AudioOutput = "gwgui_example_audio_output";
    /// <summary>Identifiant du réglage de latence audio, dont les valeurs sont exprimées en millisecondes.</summary>
    internal const string AudioLatency = "gwgui_example_audio_latency";
    /// <summary>Identifiant du dossier partagé configuré pour la machine.</summary>
    internal const string SharedFolder = "example_shared_folder";
    /// <summary>Identifiant du dossier de travail des images de disquettes.</summary>
    internal const string FloppyFolder = "example_floppy_folder";
    /// <summary>Identifiant du dossier des sauvegardes d’état.</summary>
    internal const string StateFolder = "example_state_folder";
    /// <summary>Identifiant du dossier des captures de la machine.</summary>
    internal const string CaptureFolder = "example_capture_folder";
    /// <summary>Valeur technique demandant la sortie audio par défaut plutôt qu’un périphérique nommé.</summary>
    internal const string DefaultOutput = "default";
    /// <summary>Clé du libellé du contrôleur de disques.</summary>
    internal const string ResourceDiscController = "Emulation.Example.Storage.DiscController";
    /// <summary>Clé du libellé du contrôleur vidéo.</summary>
    internal const string ResourceVideoController = "Emulation.Example.Video.Controller";
    /// <summary>Clé du libellé décrivant la production des pixels.</summary>
    internal const string ResourceVideoPixels = "Emulation.Example.Video.Pixels";
    /// <summary>Clé du libellé de la capacité de télétexte.</summary>
    internal const string ResourceTeletext = "Emulation.Example.Video.Teletext";
    /// <summary>Clé du libellé de l’option de démarrage automatique.</summary>
    internal const string ResourceAutoBoot = "Emulation.Example.Option.AutoBoot";
    /// <summary>Clé du libellé indiquant un firmware fourni avec le cœur.</summary>
    internal const string ResourceEmbeddedFirmware = "Emulation.Example.Firmware.Embedded";
    /// <summary>Clé du libellé commun de modèle de processeur.</summary>
    internal const string ResourceCpu = "Emulation.Cpu.Model";
    /// <summary>Préfixe des clés désignant les différents processeurs dans les descriptions du module.</summary>
    internal const string CpuResourcePrefix = "Emulation.Example.Cpu.";
    /// <summary>Clé du libellé de fréquence d’origine du processeur.</summary>
    internal const string ResourceFrequency = "Emulation.Cpu.SpeedOriginal";
    /// <summary>Clé du libellé de mémoire principale.</summary>
    internal const string ResourceMemory = "Emulation.Memory.Main";
    /// <summary>Clé du libellé des réglages d’affichage.</summary>
    internal const string ResourceVideo = "Emulation.Video.Settings.Display";
    /// <summary>Clé du libellé du firmware système.</summary>
    internal const string ResourceFirmware = "Emulation.Firmware.Rom.System";
    /// <summary>Clé du libellé de l’onglet général.</summary>
    internal const string ResourceGeneral = "Emulation.Tab.General";
    /// <summary>Clé du libellé du groupe audio.</summary>
    internal const string ResourceAudio = "Emulation.Audio";
    /// <summary>Clé du libellé de l’activation audio.</summary>
    internal const string ResourceAudioEnabled = "Emulation.Audio.Enabled";
    /// <summary>Clé du libellé de la sortie audio.</summary>
    internal const string ResourceAudioOutput = "Emulation.Audio.Output";
    /// <summary>Clé du libellé de la latence audio.</summary>
    internal const string ResourceAudioLatency = "Emulation.Audio.Latency";
    /// <summary>Clé du libellé utilisé pour le volume audio.</summary>
    internal const string ResourceAudioVolume = "Explorer.Volume";
    /// <summary>Clé du libellé du choix de sortie audio par défaut.</summary>
    internal const string ResourceDefaultOutput = "Emulation.Audio.DefaultOutput";
    /// <summary>Nombre d’octets dans un kibioctet, utilisé pour convertir les capacités de mémoire.</summary>
    internal const int Kibibyte = 1024;
}
