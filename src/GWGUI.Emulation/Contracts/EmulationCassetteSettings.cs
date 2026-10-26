using C = GWGUI.Emulation.Constants.EmulationTapeSettingsConstants;
using System.Globalization;

namespace GWGUI.Emulation.Contracts;

/// <summary>Paramètres réutilisables du lecteur cassette, enregistrés dans ses options de périphérique.</summary>
/// <param name="BaudRate">Zéro suit le matériel ; une autre valeur est un débit de symboles, pas une vitesse de bande.</param>
/// <param name="Mode">Réel : horloge et mécanique ; accéléré : matériel sans attente hôte ; direct : fichiers sans démodulation.</param>
/// <param name="SpeedPercent">Vitesse de bande en pourcentage ; modifie ensemble durée et hauteur du signal lu.</param>
/// <param name="WindMultiplier">Rapport de déplacement mécanique en avance ou retour rapides.</param>
/// <param name="ReadLatencyMilliseconds">Temps d'établissement du moteur avant la lecture.</param>
/// <param name="WriteLatencyMilliseconds">Temps d'établissement du moteur avant l'enregistrement.</param>
/// <param name="MechanicalSound">Active les sons mécaniques uniquement en mode réel.</param>
/// <param name="WriteProtected">Interdit l'enregistrement indépendamment de la protection de l'image montée.</param>
/// <param name="MechanicalVolumePercent">Volume des seuls bruitages mécaniques, de zéro à cent pour cent.</param>
public sealed record EmulationCassetteSettings(int BaudRate = 0, EmulationCassettePlaybackMode Mode = EmulationCassettePlaybackMode.RealTime, int SpeedPercent = 100, int WindMultiplier = 20, int ReadLatencyMilliseconds = 0, int WriteLatencyMilliseconds = 0, bool MechanicalSound = true, bool WriteProtected = false, int MechanicalVolumePercent = GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants.DefaultVolumePercent)
{
    public const string Prefix = "cassette.";
    /// <summary>Relit et valide les options communes ; les débits acceptés sont déclarés par la machine.</summary>
    public static EmulationCassetteSettings Read(IReadOnlyDictionary<string, string> options)
    {
        int Number(string name, int fallback) => options.TryGetValue(Prefix + name, out var text) ? int.Parse(text, CultureInfo.InvariantCulture) : fallback;
        bool Flag(string name, bool fallback) => options.TryGetValue(Prefix + name, out var text) ? bool.Parse(text) : fallback;
        var mode = options.TryGetValue(Prefix + "mode", out var value) ? Enum.Parse<EmulationCassettePlaybackMode>(value) : EmulationCassettePlaybackMode.RealTime;
        var result = new EmulationCassetteSettings(Number("baud", 0), mode, Number("speed", 100), Number("wind", 20), Number("readLatency", 0), Number("writeLatency", 0), Flag("sound", true), Flag("protected", false), EmulationMechanicalSoundSettings.ReadVolume(options, Prefix + GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants.VolumeOption));
        if (!Enum.IsDefined(mode) || result.BaudRate < 0 || result.SpeedPercent is < 25 or > 400 || result.WindMultiplier is < 1 or > 100 || result.ReadLatencyMilliseconds is < 0 or > 10000 || result.WriteLatencyMilliseconds is < 0 or > 10000) throw new ArgumentOutOfRangeException(nameof(options));
        return result;
    }

    /// <summary>Champs communs affichés par le dialogue de stockage existant, sans logique propre à une marque dans App.</summary>
    public static IReadOnlyList<EmulationSettingsField> Describe(IReadOnlyDictionary<string, string> options, IReadOnlyList<int> baudRates)
    {
        var settings = Read(options);
        EmulationSettingsField Choice(string id, string label, string current, IReadOnlyList<EmulationSettingsChoice> choices) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Settings." + label, EmulationSettingsEditor.Selection, current, choices, ExplanationResourceKey: id == "baud" ? C.AutomaticHelp : null, UseRadioButtons: true);
        EmulationSettingsField Range(string id, string label, int current, IReadOnlyList<int> presets, int step, string unit) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Settings." + label, EmulationSettingsEditor.Slider, current.ToString(CultureInfo.InvariantCulture), Choices: presets.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, value.ToString(CultureInfo.InvariantCulture))).ToArray(), NumericRange: new(presets[0], presets[^1], step, unit));
        EmulationSettingsField Toggle(string id, string label, bool current) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Settings." + label, EmulationSettingsEditor.Toggle, current.ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString);
        return
        [
            Choice("baud", "Baud", settings.BaudRate.ToString(CultureInfo.InvariantCulture), new[] { new EmulationSettingsChoice("0", "Emulation.Cassette.Settings.Automatic") }.Concat(baudRates.Select(rate => new EmulationSettingsChoice(rate.ToString(CultureInfo.InvariantCulture), string.Empty, rate.ToString(CultureInfo.InvariantCulture)))).ToArray()),
            Choice("mode", "Mode", settings.Mode.ToString(), Enum.GetValues<EmulationCassettePlaybackMode>().Select(mode => new EmulationSettingsChoice(mode.ToString(), "Emulation.Cassette.Settings.Mode." + mode)).ToArray()),
            Range("speed", "Speed", settings.SpeedPercent, C.TapeSpeeds, C.TapeSpeedStep, "%"),
            Range("wind", "Wind", settings.WindMultiplier, C.WindingSpeeds, C.WindingSpeedStep, "×"),
            Range("readLatency", "ReadLatency", settings.ReadLatencyMilliseconds, C.MotorDelays, C.MotorDelayStep, "ms"),
            Range("writeLatency", "WriteLatency", settings.WriteLatencyMilliseconds, C.MotorDelays, C.MotorDelayStep, "ms"),
            EmulationMechanicalSoundSettings.DescribeVolume(Prefix + GWGUI.Emulation.Constants.EmulationMechanicalSoundConstants.VolumeOption, "Tape", settings.MechanicalVolumePercent),
            Toggle("sound", "Sound", settings.MechanicalSound), Toggle("protected", "Protected", settings.WriteProtected),
            .. EmulationCassettePcmSettings.Describe(options)
        ];
    }
    /// <summary>Groupes proposés au module, qui peut filtrer ou compléter librement les champs avant de les transmettre.</summary>
    public static IReadOnlyList<EmulationSettingsBlock> DescribeBlocks(IReadOnlyDictionary<string, string> options, IReadOnlyList<int> baudRates)
    {
        var fields = Describe(options, baudRates);
        return
        [
            new(C.PlaybackGroup, EmulationMachineTab.Storage, C.PlaybackGroup, fields.Where(field => field.Id == C.Baud || field.Id == C.Mode || field.Id == C.Protected).ToArray(), Columns: 2, IsCompact: true),
            new(C.MechanicsGroup, EmulationMachineTab.Storage, C.MechanicsGroup, fields.Where(field => field.NumericRange is not null || field.Id == C.Sound).ToArray(), IsCompact: true, VisibleWhenFieldId: C.Mode, VisibleWhenValue: EmulationCassettePlaybackMode.RealTime.ToString()),
            new(C.RawGroup, EmulationMachineTab.Storage, C.RawGroup, fields.Where(field => field.Id.StartsWith(EmulationCassettePcmSettings.Prefix, StringComparison.Ordinal)).ToArray(), IsCompact: true)
        ];
    }
}
