namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorControllerAdapter
{
    IReadOnlyList<ControllerType> GetPeripherals(MachineConfiguration configuration);
    IReadOnlyList<ControllerType> GetPeripherals(MachineConfiguration configuration, int port) => GetPeripherals(configuration);
    IReadOnlyList<ControllerType> GetUsbPeripherals(MachineConfiguration configuration, int port) => [ControllerType.None];
    int GetControllerPortCount(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).ControllerPortCount;
    string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => null;
    int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port) => null;
    IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, ControllerType type, int port) => null;
}
