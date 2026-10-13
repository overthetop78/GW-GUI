using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Dictionaries;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Functions;

internal static class ConfigurationOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Defaults(MachineModel model) => B2ModelFunctions.GetModels(model).Count == 0 ? new Dictionary<string, string>()
        : new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [OptionConstants.Model] = B2ModelFunctions.DefaultValue(model),
            [OptionConstants.AutoBoot] = OptionConstants.Enabled
        };

    internal static void Validate(MachineConfiguration configuration)
    {
        if (configuration.Options.TryGetValue(OptionConstants.Model, out var selectedModel) && !B2ModelFunctions.IsCompatible(configuration.Model, selectedModel))
            throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        if (configuration.Options.TryGetValue(OptionConstants.AutoBoot, out var autoBoot) && autoBoot is not (OptionConstants.Enabled or OptionConstants.Disabled))
            throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
        foreach (var option in B2OptionCatalog.JoypadOptions)
            if (configuration.Options.TryGetValue(option, out var key) && !B2OptionCatalog.KeyboardValues.Contains(key, StringComparer.Ordinal))
                throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(configuration));
    }
}
