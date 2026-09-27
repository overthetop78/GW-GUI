using GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Constants;

namespace GWGUI.Emulation.Atari.Emulators.VirtualJaguar.Functions;

internal static class VirtualJaguarOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> ToNative(
        IReadOnlyDictionary<string, string> options)
    {
        var translated = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (!translated.Remove(CartridgeConstants.RegionOptionKey, out var value)) return translated;
        translated[VirtualJaguarOptionConstants.Pal] = value switch
        {
            CartridgeConstants.AutomaticRegionValue or CartridgeConstants.NtscRegionValue =>
                CartridgeConstants.DisabledValue,
            CartridgeConstants.PalRegionValue => CartridgeConstants.EnabledValue,
            CartridgeConstants.SecamRegionValue => throw CartridgeExceptions.SecamUnsupported(),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options, null)
        };
        return translated;
    }
}
