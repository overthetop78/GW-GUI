using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;
using Pce = GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;
using Quasi = GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;
using Neko = GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;
using Kai = GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

namespace GWGUI.Emulation.Nec.Emulators.Common.Interop.Functions;

internal static class CoreSettingsFunctions
{
    internal static IReadOnlyList<CoreOptionDefinition> Definitions(string emulatorId) => emulatorId switch
    {
        Pce.CoreConstants.Id => Pce.OptionConstants.All,
        Quasi.CoreConstants.Id => Quasi.OptionConstants.All,
        Neko.CoreConstants.Id => Neko.OptionConstants.All,
        Kai.CoreConstants.Id => Kai.OptionConstants.All,
        _ => []
    };

    internal static bool HasDefinitions(string emulatorId) => Definitions(emulatorId).Count != 0;

    internal static IReadOnlyList<EmulationSettingsField> Fields(MachineConfiguration configuration,
        EmulationMachineTab tab, string blockId) => Definitions(configuration.EmulatorId)
        .Where(option => option.Tab == tab && AppliesTo(option, configuration.Model)).Select(option => new EmulationSettingsField(
            option.Key, tab, blockId, option.LabelResourceKey, EmulationSettingsEditor.Selection,
            configuration.Options?.GetValueOrDefault(option.Key) ?? Default(configuration, option),
            option.Choices.Select(choice => new EmulationSettingsChoice(choice.Value,
                choice.IsInvariant ? string.Empty : choice.Label,
                choice.IsInvariant ? choice.Label : null)).ToArray())).ToArray();

    internal static MachineConfiguration Configure(MachineConfiguration configuration)
    {
        var options = new Dictionary<string, string>(configuration.Options
            ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        foreach (var option in Definitions(configuration.EmulatorId))
        {
            var fallback = Default(configuration, option);
            var selected = AppliesTo(option, configuration.Model)
                ? options.GetValueOrDefault(option.Key) ?? fallback : fallback;
            options[option.Key] = option.Choices.Any(choice => choice.Value == selected) ? selected : fallback;
        }
        return configuration with { Options = options };
    }

    private static bool AppliesTo(CoreOptionDefinition option, string machineId) =>
        option.MachineIds is null || option.MachineIds.Contains(machineId);

    private static string Default(MachineConfiguration configuration, CoreOptionDefinition option) =>
        configuration.EmulatorId == Quasi.CoreConstants.Id && option.Key == Quasi.ModelConstants.BasicModeOption
            ? Quasi.ModelConstants.BasicModeFor(configuration.Model) : option.DefaultValue;
}
