using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Factories;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Constants;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Functions;
using GWGUI.Emulation.Sony.Emulators.PokketStation.Dictionaries;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Functions;
using PocketInputs = GWGUI.Emulation.Sony.Common.Machines.PocketStation.Functions.ControllerFunctions;

namespace GWGUI.Emulation.Sony.Emulators.PokketStation.Factories;

internal sealed class PokketStationMachineFactory : CoreMachineFactory
{
    protected override CoreDefinition CoreDefinition => CoreConstants.Definition;

    public override int GetControllerPortCount(MachineConfiguration configuration) => LinkFunctions.IsLinked(configuration) ? LinkConstants.UnitCount : 1;

    public override IReadOnlyList<EmulationMediaDevice> GetStorageDevices(MachineConfiguration configuration) =>
    [new(new EmulationMediaSlot(EmulationMediaCategory.MemoryCard, 1), EmulationMediaType.MemoryCard,
        GWGUI.Emulation.Sony.Common.Machines.PocketStation.Constants.MediaConstants.FlashImageExtensions,
        RequiresMachineRecreation: true, DisplayLabel: LinkConstants.SecondaryCardLabel)];

    public override IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, ControllerType type, int port)
    {
        if (type == ControllerType.None) return [];
        return LinkFunctions.IsLinked(configuration) && port == 0
            ? [.. PocketInputs.Definitions,
                new(EmulationControllerCommandIds.L, LinkConstants.SelectUnitALabel, string.Empty),
                new(EmulationControllerCommandIds.R, LinkConstants.SelectUnitBLabel, string.Empty)]
            : PocketInputs.Definitions;
    }

    public override IReadOnlyList<EmulationSettingsBlock> DescribeSettings(MachineConfiguration configuration, EmulatorManagementContext? context = null)
    {
        if (!LinkFunctions.IsLinked(configuration)) return [];
        var identity = OptionCatalog.All.Single(option => option.Key == LinkConstants.IdentityKey);
        var field = new EmulationSettingsField(identity.Key, EmulationMachineTab.General, LinkConstants.Group,
            identity.Name, EmulationSettingsEditor.Selection, configuration.Options?.GetValueOrDefault(identity.Key) ?? identity.DefaultValue,
            Choices: identity.Values.Select(value => new EmulationSettingsChoice(value.Value, string.Empty, InvariantDisplayValue: value.Label)).ToArray(),
            ExplanationResourceKey: LinkConstants.Help,
            RequiresRestart: true);
        return [.. CoreSettingsFunctions.Blocks(OptionCatalog.All.Where(option => option.Key != identity.Key).ToArray(), configuration),
            new(LinkConstants.Group, EmulationMachineTab.General, LinkConstants.GroupLabel, [field])];
    }
}
