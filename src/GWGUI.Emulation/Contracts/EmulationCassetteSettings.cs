using System.Globalization;

namespace GWGUI.Emulation.Contracts;

/// <summary>Modes de lecture distincts de la modulation série et de la vitesse mécanique du magnétophone.</summary>
public enum EmulationCassettePlaybackMode { RealTime, Accelerated, Direct }

/// <summary>Paramètres réutilisables du lecteur cassette, enregistrés dans ses options de périphérique.</summary>
/// <param name="BaudRate">Zéro suit le matériel ; une autre valeur est un débit de symboles, pas une vitesse de bande.</param>
/// <param name="Mode">Réel : horloge et mécanique ; accéléré : matériel sans attente hôte ; direct : fichiers sans démodulation.</param>
/// <param name="SpeedPercent">Vitesse de bande en pourcentage ; modifie ensemble durée et hauteur du signal lu.</param>
/// <param name="WindMultiplier">Rapport de déplacement mécanique en avance ou retour rapides.</param>
/// <param name="ReadLatencyMilliseconds">Temps d'établissement du moteur avant la lecture.</param>
/// <param name="WriteLatencyMilliseconds">Temps d'établissement du moteur avant l'enregistrement.</param>
/// <param name="MechanicalSound">Active les sons mécaniques uniquement en mode réel.</param>
/// <param name="WriteProtected">Interdit l'enregistrement indépendamment de la protection de l'image montée.</param>
public sealed record EmulationCassetteSettings(int BaudRate = 0, EmulationCassettePlaybackMode Mode = EmulationCassettePlaybackMode.RealTime, int SpeedPercent = 100, int WindMultiplier = 20, int ReadLatencyMilliseconds = 0, int WriteLatencyMilliseconds = 0, bool MechanicalSound = true, bool WriteProtected = false)
{
    public const string Prefix = "cassette.";
    /// <summary>Relit et valide les options communes ; les débits acceptés sont déclarés par la machine.</summary>
    public static EmulationCassetteSettings Read(IReadOnlyDictionary<string, string> options)
    {
        int Number(string name, int fallback) => options.TryGetValue(Prefix + name, out var text) ? int.Parse(text, CultureInfo.InvariantCulture) : fallback;
        bool Flag(string name, bool fallback) => options.TryGetValue(Prefix + name, out var text) ? bool.Parse(text) : fallback;
        var mode = options.TryGetValue(Prefix + "mode", out var value) ? Enum.Parse<EmulationCassettePlaybackMode>(value) : EmulationCassettePlaybackMode.RealTime;
        var result = new EmulationCassetteSettings(Number("baud", 0), mode, Number("speed", 100), Number("wind", 20), Number("readLatency", 0), Number("writeLatency", 0), Flag("sound", true), Flag("protected", false));
        if (!Enum.IsDefined(mode) || result.BaudRate < 0 || result.SpeedPercent is < 25 or > 400 || result.WindMultiplier is < 1 or > 100 || result.ReadLatencyMilliseconds is < 0 or > 10000 || result.WriteLatencyMilliseconds is < 0 or > 10000) throw new ArgumentOutOfRangeException(nameof(options));
        return result;
    }

    /// <summary>Champs communs affichés par le dialogue de stockage existant, sans logique propre à une marque dans App.</summary>
    public static IReadOnlyList<EmulationSettingsField> Describe(IReadOnlyDictionary<string, string> options, IReadOnlyList<int> baudRates)
    {
        var settings = Read(options);
        EmulationSettingsField Choice(string id, string label, string current, IReadOnlyList<EmulationSettingsChoice> choices) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Settings." + label, EmulationSettingsEditor.Selection, current, choices);
        EmulationSettingsField Range(string id, string label, int current, int minimum, int maximum, int step, string unit) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Settings." + label, EmulationSettingsEditor.Slider, current.ToString(CultureInfo.InvariantCulture), NumericRange: new(minimum, maximum, step, unit));
        EmulationSettingsField Toggle(string id, string label, bool current) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Settings." + label, EmulationSettingsEditor.Toggle, current.ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString);
        return
        [
            Choice("baud", "Baud", settings.BaudRate.ToString(CultureInfo.InvariantCulture), new[] { new EmulationSettingsChoice("0", "Emulation.Cassette.Settings.Automatic") }.Concat(baudRates.Select(rate => new EmulationSettingsChoice(rate.ToString(CultureInfo.InvariantCulture), string.Empty, rate.ToString(CultureInfo.InvariantCulture)))).ToArray()),
            Choice("mode", "Mode", settings.Mode.ToString(), Enum.GetValues<EmulationCassettePlaybackMode>().Select(mode => new EmulationSettingsChoice(mode.ToString(), "Emulation.Cassette.Settings.Mode." + mode)).ToArray()),
            Range("speed", "Speed", settings.SpeedPercent, 25, 400, 25, "%"),
            Range("wind", "Wind", settings.WindMultiplier, 1, 100, 1, "×"),
            Range("readLatency", "ReadLatency", settings.ReadLatencyMilliseconds, 0, 10000, 50, "ms"),
            Range("writeLatency", "WriteLatency", settings.WriteLatencyMilliseconds, 0, 10000, 50, "ms"),
            Toggle("sound", "Sound", settings.MechanicalSound), Toggle("protected", "Protected", settings.WriteProtected),
            .. EmulationCassettePcmSettings.Describe(options)
        ];
    }
}
