using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Constants;

/// <summary>Champs et choix communs proposés aux modules pour la configuration d'un lecteur de bande.</summary>
public static class EmulationTapeSettingsConstants
{
    /// <summary>Identifiant du champ de débit de transmission de la bande, en bauds.</summary>
    public const string Baud = EmulationCassetteSettings.Prefix + "baud";

    /// <summary>Identifiant du champ de mode de chargement de la bande.</summary>
    public const string Mode = EmulationCassetteSettings.Prefix + "mode";

    /// <summary>Identifiant du champ d’activation des bruitages mécaniques du lecteur.</summary>
    public const string Sound = EmulationCassetteSettings.Prefix + "sound";

    /// <summary>Identifiant du champ de vitesse de défilement de la bande.</summary>
    public const string Speed = EmulationCassetteSettings.Prefix + "speed";

    /// <summary>Identifiant du champ de vitesse d’avance rapide et de rembobinage.</summary>
    public const string Wind = EmulationCassetteSettings.Prefix + "wind";

    /// <summary>Identifiant du champ de protection de l’image contre l’écriture.</summary>
    public const string Protected = EmulationCassetteSettings.Prefix + "protected";
    /// <summary>Clé de traduction du groupe des réglages de lecture.</summary>
    public const string PlaybackGroup = "Emulation.Cassette.Settings.Group.Playback";

    /// <summary>Clé de traduction du groupe des réglages mécaniques du lecteur.</summary>
    public const string MechanicsGroup = "Emulation.Cassette.Settings.Group.Mechanics";

    /// <summary>Clé de traduction du groupe décrivant le format audio RAW.</summary>
    public const string RawGroup = "Format.tape.raw";
    /// <summary>Clé de traduction du choix d’un canal audio mono.</summary>
    public const string MonoLabel = "Emulation.Cassette.Pcm.Mono";

    /// <summary>Clé de traduction du choix de deux canaux audio stéréo.</summary>
    public const string StereoLabel = "Emulation.Cassette.Pcm.Stereo";

    /// <summary>Clé de traduction de l’aide du débit de transmission automatique.</summary>
    public const string AutomaticHelp = "Emulation.Cassette.Settings.Automatic.Help";
    /// <summary>Pas du réglage de vitesse de bande, en points de pourcentage.</summary>
    public const int TapeSpeedStep = 25;

    /// <summary>Pas du multiplicateur de vitesse d’avance rapide et de rembobinage.</summary>
    public const int WindingSpeedStep = 1;

    /// <summary>Pas du réglage de délai moteur, en millisecondes.</summary>
    public const int MotorDelayStep = 50;
    public static IReadOnlyList<int> TapeSpeeds { get; } = Array.AsReadOnly<int>([25, 50, 75, 100, 150, 200, 300, 400]);
    public static IReadOnlyList<int> WindingSpeeds { get; } = Array.AsReadOnly<int>([1, 2, 5, 10, 20, 50, 100]);
    public static IReadOnlyList<int> MotorDelays { get; } = Array.AsReadOnly<int>([0, 50, 100, 250, 500, 1000, 2000, 5000, 10000]);
}
