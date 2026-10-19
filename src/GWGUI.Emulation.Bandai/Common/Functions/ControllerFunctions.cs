namespace GWGUI.Emulation.Bandai.Common.Functions;

internal static class ControllerFunctions
{
    internal static IReadOnlyList<PeripheralCategory> Peripherals(MachineModel model) => [PeripheralCategory.None, PeripheralCategory.Automatic, ConfigurationFunctions.DefaultPeripheral(model)];

    // These devices expose digital buttons. No analog scaling is applied to their native input.
    internal static EmulationInputSnapshot ApplyDeadZones(EmulationInputSnapshot input, IReadOnlyList<ControllerBinding>? bindings) =>
        bindings?.Any(binding => binding.Port == ControllerConstants.FirstPort && binding.Peripheral == PeripheralCategory.None) == true ? input with { Controllers = [EmulationControllerState.Empty] } : EmulationInputMappingFunctions.Freeze(input);
}
