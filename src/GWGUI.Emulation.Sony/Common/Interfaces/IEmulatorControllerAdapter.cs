namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorControllerAdapter
{
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration);
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration, int port) => GetPeripherals(configuration);
    IReadOnlyList<PeripheralCategory> GetUsbPeripherals(MachineConfiguration configuration, int port) => [PeripheralCategory.None];
    int GetControllerPortCount(MachineConfiguration configuration);
    string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => null;
    int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => null;
    IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory type, int port) => null;
}
