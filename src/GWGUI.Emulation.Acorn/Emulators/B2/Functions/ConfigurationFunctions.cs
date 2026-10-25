using GWGUI.Emulation.Acorn.Emulators.B2.Constants;
using GWGUI.Emulation.Acorn.Emulators.B2.Dictionaries;

namespace GWGUI.Emulation.Acorn.Emulators.B2.Functions;

internal static class ConfigurationOptionFunctions
{
    internal static IReadOnlyDictionary<string, string> Normalize(MachineModel model, IReadOnlyDictionary<string, string> options)
    {
        var result = new Dictionary<string, string>(options, StringComparer.Ordinal);
        if (model == MachineModel.BbcMicroModelB)
        {
            var native = options.GetValueOrDefault(OptionConstants.Model, B2ModelFunctions.DefaultValue(model));
            var controller = options.GetValueOrDefault(OptionConstants.DiscController, DiscController(native));
            var memory = options.GetValueOrDefault(OptionConstants.DiscMemory, DiscMemory(native));
            result[OptionConstants.Model] = controller == OptionConstants.ChallengerController ? memory switch
            {
                OptionConstants.Challenger256 => OptionConstants.ModelBOpusChallenger256K,
                OptionConstants.Challenger512 => OptionConstants.ModelBOpusChallenger512K,
                _ => throw new ArgumentException(ErrorMessages.OptionValueInvalidFormat, nameof(options))
            } : "B/" + controller;
        }
        result.Remove(OptionConstants.DiscController);
        result.Remove(OptionConstants.DiscMemory);
        return result;
    }

    internal static string DiscController(string native) => native is OptionConstants.ModelBOpusChallenger256K or OptionConstants.ModelBOpusChallenger512K
        ? OptionConstants.ChallengerController : native.StartsWith("B/", StringComparison.Ordinal) ? native[2..] : native;

    internal static string DiscMemory(string native) => native == OptionConstants.ModelBOpusChallenger512K ? OptionConstants.Challenger512 : OptionConstants.Challenger256;

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
