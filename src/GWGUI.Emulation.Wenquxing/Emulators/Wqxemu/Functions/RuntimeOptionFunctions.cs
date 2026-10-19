using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants;
using GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Dictionaries;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Functions;

internal static class RuntimeOptionFunctions
{
    internal static EmulationLegacyOptionHost<CoreOption> CreateCallbackHost(IReadOnlyDictionary<string, string> values) =>
        new(values,
        static (key, name, defaultValue, current, choices) => new CoreOption(key, name, null, null, defaultValue, current, choices.Select(value => new CoreOptionValue(value, value)).ToArray(), IsVisible: key != GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Constants.RuntimeOptionConstants.Model),
        static (option, value) => option.Values.Any(choice => choice.Value == value),
        static (option, value) => option with { CurrentValue = value });

    internal static IReadOnlyDictionary<string, string> Prepare(MachineConfiguration configuration)
    {
        ConfigurationFunctions.Validate(configuration);
        return new Dictionary<string, string>(configuration.Options, StringComparer.Ordinal) { [RuntimeOptionConstants.Model] = ModelDictionary.NativeValues[configuration.Model].ToUpperInvariant() };
    }
}