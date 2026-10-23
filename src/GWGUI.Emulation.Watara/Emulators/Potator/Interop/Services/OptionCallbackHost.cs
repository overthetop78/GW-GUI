using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Watara.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Watara.Emulators.Potator.Interop.Services;

internal static class OptionCallbackHost
{
    /// <summary>Crée le propriétaire partagé avec les descriptions et validations des options du backend.</summary>
    internal static IEmulationVariableOptionHost<CoreOption> Create(IReadOnlyDictionary<string, string> values) => CommonCoreHostFunctions.NativeResources.CreateVariableOptionHost<CoreOption>(values,
        static (key, name, defaultValue, current, choices) => new CoreOption(key, name, null, null, defaultValue, current, choices.Select(value => new CoreOptionValue(value, value)).ToArray()),
        static (option, value) => option.Values.Any(choice => choice.Value == value),
        static (option, value) => option with { CurrentValue = value });
}
