using System.Globalization;

namespace GWGUI.Emulation.Contracts;

/// <summary>Profil RAW commun du magnétophone ; ces paramètres sont requis parce que le fichier n'a aucun en-tête.</summary>
public sealed record EmulationCassettePcmSettings(int SampleRate = 48000, int Channels = 1, int BitsPerSample = 16, bool Signed = true, bool LittleEndian = true)
{
    public const string Prefix = "cassette.pcm.";
    public static EmulationCassettePcmSettings Read(IReadOnlyDictionary<string, string> options)
    {
        int Number(string name, int fallback) => options.TryGetValue(Prefix + name, out var value) ? int.Parse(value, CultureInfo.InvariantCulture) : fallback;
        bool Flag(string name, bool fallback) => options.TryGetValue(Prefix + name, out var value) ? bool.Parse(value) : fallback;
        var result = new EmulationCassettePcmSettings(Number("sampleRate", 48000), Number("channels", 1), Number("bitsPerSample", 16), Flag("signed", true), Flag("littleEndian", true));
        if (result.SampleRate is < 8000 or > 192000 || result.Channels is < 1 or > 2 || result.BitsPerSample is not (8 or 16 or 24 or 32)) throw new ArgumentOutOfRangeException(nameof(options));
        return result;
    }

    /// <summary>Paramètres transmis au service de média sans exposer les types de MediaEngine.</summary>
    public IReadOnlyDictionary<string, string> ToParameters() => new Dictionary<string, string>
    {
        ["sampleRate"] = SampleRate.ToString(CultureInfo.InvariantCulture), ["channels"] = Channels.ToString(CultureInfo.InvariantCulture),
        ["bitsPerSample"] = BitsPerSample.ToString(CultureInfo.InvariantCulture), ["signed"] = Signed.ToString(), ["littleEndian"] = LittleEndian.ToString()
    };

    public static IReadOnlyList<EmulationSettingsField> Describe(IReadOnlyDictionary<string, string> options)
    {
        var profile = Read(options);
        EmulationSettingsField Choice(string id, string label, int selected, params int[] values) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Pcm." + label, EmulationSettingsEditor.Selection, selected.ToString(CultureInfo.InvariantCulture), values.Select(value => new EmulationSettingsChoice(value.ToString(CultureInfo.InvariantCulture), string.Empty, value.ToString(CultureInfo.InvariantCulture))).ToArray());
        EmulationSettingsField Flag(string id, string label, bool value) => new(Prefix + id, EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Pcm." + label, EmulationSettingsEditor.Toggle, value.ToString(), EnabledValue: bool.TrueString, DisabledValue: bool.FalseString);
        return [Choice("sampleRate", "SampleRate", profile.SampleRate, 22050, 44100, 48000, 96000, 192000), new EmulationSettingsField(Prefix + "channels", EmulationMachineTab.Storage, "Cassette", "Emulation.Cassette.Pcm.Channels", EmulationSettingsEditor.Selection, profile.Channels.ToString(CultureInfo.InvariantCulture), [new("1", EmulationTapeSettingsConstants.MonoLabel), new("2", EmulationTapeSettingsConstants.StereoLabel)], UseRadioButtons: true), Choice("bitsPerSample", "Bits", profile.BitsPerSample, 8, 16, 24, 32), Flag("signed", "Signed", profile.Signed), Flag("littleEndian", "LittleEndian", profile.LittleEndian)];
    }
}
