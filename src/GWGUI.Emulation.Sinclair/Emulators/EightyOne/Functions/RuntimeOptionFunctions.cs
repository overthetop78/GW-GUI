using GWGUI.Emulation.Sinclair.Emulators.EightyOne.Constants;
using GWGUI.Emulation.Sinclair.Emulators.EightyOne.Dictionaries;

namespace GWGUI.Emulation.Sinclair.Emulators.EightyOne.Functions;

internal static class RuntimeOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Defaults(MachineModel model)
    {
        if (model != MachineModel.Zx81) throw new ArgumentOutOfRangeException(nameof(model));
        return OptionCatalog.Options.ToDictionary(option => option.Key, option => option.DefaultValue, StringComparer.Ordinal);
    }

    internal static bool IsAvailable(MachineConfiguration configuration, string key) => configuration.Model == MachineModel.Zx81 && OptionCatalog.Options.Any(option => option.Key == key);

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        var result = new Dictionary<string, string>(Defaults(configuration.Model), StringComparer.Ordinal);
        foreach (var option in OptionCatalog.Options.Where(option => option.IsVisible)) if (configuration.Options.TryGetValue(option.Key, out var value)) result[option.Key] = value;
        foreach (var option in OptionCatalog.Options.Where(option => option.Key.StartsWith(RuntimeOptionConstants.JoypadOptionPrefix, StringComparison.Ordinal))) result[option.Key] = RuntimeOptionConstants.Automatic;
        return result;
    }
}
