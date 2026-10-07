using GWGUI.Emulation.Commodore.Emulators.Common.Interop.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Functions;

internal static class UaeOptionFunctions
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
            var key = option.Key.StartsWith(UaeOptionConstants.GenericPrefix, StringComparison.Ordinal)
                ? UaeOptionConstants.NativePrefix + option.Key[UaeOptionConstants.GenericPrefix.Length..]
                : option.Key;
            translated[key] = option.Value;
        }
        return translated;
    }
}
