using GWGUI.Emulation.Sharp.Emulators.X1.Constants;
using GWGUI.Emulation.Sharp.Emulators.X1.Dictionaries;

namespace GWGUI.Emulation.Sharp.Emulators.X1.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Defaults(MachineModel model)
    {
        var result = OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
        result[OptionConstants.Romtype] = model switch { MachineModel.X1 => RuntimeOptionConstants.BaseRom, MachineModel.X1Turbo => RuntimeOptionConstants.TurboRom, MachineModel.X1TurboZ => RuntimeOptionConstants.TurboZRom, _ => throw new ArgumentOutOfRangeException(nameof(model)) };
        return result;
    }

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var defaults = Defaults(configuration.Model);
        var result = new Dictionary<string, string>(defaults, StringComparer.Ordinal);
        foreach (var option in configuration.Options) if (result.ContainsKey(option.Key)) result[option.Key] = option.Value;
        result[OptionConstants.Romtype] = defaults[OptionConstants.Romtype];
        result[OptionConstants.KeyMode] = RuntimeOptionConstants.KeyboardMode;
        return result;
    }
}
