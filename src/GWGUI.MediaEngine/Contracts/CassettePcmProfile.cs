using System.Globalization;

namespace GWGUI.MediaEngine.Contracts;

/// <summary>Profil explicite d'un signal PCM sans en-tête ; les valeurs par défaut ne prétendent pas identifier sa source.</summary>
public sealed record CassettePcmProfile(int SampleRate = 48000, int Channels = 1, int BitsPerSample = 16, bool Signed = true, bool LittleEndian = true)
{
    public static CassettePcmProfile Read(IReadOnlyDictionary<string, string>? parameters)
    {
        int Number(string key, int fallback) => parameters?.TryGetValue(key, out var value) == true ? int.Parse(value, CultureInfo.InvariantCulture) : fallback;
        bool Flag(string key, bool fallback) => parameters?.TryGetValue(key, out var value) == true ? bool.Parse(value) : fallback;
        var result = new CassettePcmProfile(Number("sampleRate", 48000), Number("channels", 1), Number("bitsPerSample", 16), Flag("signed", true), Flag("littleEndian", true));
        if (result.SampleRate is < 8000 or > 192000 || result.Channels is < 1 or > 2 || result.BitsPerSample is not (8 or 16 or 24 or 32)) throw new ArgumentOutOfRangeException(nameof(parameters));
        return result;
    }
}
