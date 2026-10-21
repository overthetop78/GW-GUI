namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Identifiants invariants des éditeurs propres aux profils d’exemple.</summary>
internal static class SettingsConstants
{
    /// <summary>Choix explicite de l’impulsion de démarrage automatique d’une cassette insérée.</summary>
    internal const string CassetteAutoBoot = "example_cassette_auto_boot";
    /// <summary>Identité du champ de processeur fictif.</summary>
    internal const string Cpu = "example_cpu";
    /// <summary>Identité du champ de fréquence du processeur, en hertz.</summary>
    internal const string CpuFrequency = "example_cpu_frequency";
    /// <summary>Identité du champ de capacité RAM, en octets.</summary>
    internal const string MainMemory = "example_main_memory";
    /// <summary>Identité du champ de firmware facultatif.</summary>
    internal const string Firmware = "example_firmware";
    /// <summary>Identité des réglages de vidéo.</summary>
    internal const string Video = "example_video";
    /// <summary>Identité de l’autorisation de sortie audio.</summary>
    internal const string AudioEnabled = "example_audio_enabled";
    /// <summary>Clé de sélection de la sortie physique fournie par l’hôte.</summary>
    internal const string AudioOutput = "gwgui_example_audio_output";
    /// <summary>Clé de latence demandée à la sortie audio, en millisecondes.</summary>
    internal const string AudioLatency = "gwgui_example_audio_latency";
    /// <summary>Identité du champ de répertoire partagé.</summary>
    internal const string SharedFolder = "example_shared_folder";
    /// <summary>Identité du répertoire initial de disquettes.</summary>
    internal const string FloppyFolder = "example_floppy_folder";
    /// <summary>Identité du répertoire des sauvegardes d’état.</summary>
    internal const string StateFolder = "example_state_folder";
    /// <summary>Identité du répertoire des captures d’image.</summary>
    internal const string CaptureFolder = "example_capture_folder";
    /// <summary>Valeur qui laisse l’hôte choisir sa sortie audio par défaut.</summary>
    internal const string DefaultOutput = "default";
}
