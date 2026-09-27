using GWGUI.Emulation.Atari.Emulators.Stella.Constants;

namespace GWGUI.Emulation.Atari.Emulators.Stella.Functions;

internal static class StellaOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> ToNative(
        IReadOnlyDictionary<string, string> options)
    {
        var translated = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (translated.Remove(CartridgeConstants.RegionOptionKey, out var value))
            translated[StellaOptionConstants.Region] = value;
        return translated;
    }
}
