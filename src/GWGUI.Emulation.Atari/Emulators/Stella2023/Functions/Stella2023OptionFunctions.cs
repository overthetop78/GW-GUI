using GWGUI.Emulation.Atari.Emulators.Stella2023.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Stella2023.Functions;

internal static class Stella2023OptionFunctions
{
    internal static IReadOnlyDictionary<string, string> ToNative(
        IReadOnlyDictionary<string, string> options)
    {
        var translated = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (translated.Remove(CartridgeConstants.RegionOptionKey, out var value))
            translated[Stella2023OptionConstants.Region] = value;
        return translated;
    }
}
