using GWGUI.Emulation.Amiga.Emulators.PUAE.Constants;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Functions;

internal static class PuaeOptionFunctions
{
    internal static MachineConfiguration ToNative(MachineConfiguration configuration) =>
        configuration with { Options = Translate(configuration.Options) };

    private static IReadOnlyDictionary<string, string>? Translate(
        IReadOnlyDictionary<string, string>? options)
    {
        if (options is null) return null;
        var translated = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var option in options)
        {
            var key = option.Key.StartsWith(PuaeOptionConstants.GenericPrefix, StringComparison.Ordinal)
                ? PuaeOptionConstants.NativePrefix + option.Key[PuaeOptionConstants.GenericPrefix.Length..]
                : option.Key;
            translated[key] = option.Value;
        }
        return translated;
    }
}
