namespace GWGUI.Emulation.Sony.Common.Interfaces;

internal interface IEmulatorControllerAdapter
{
    IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration);
    IReadOnlyList<ControllerType> GetControllerTypes(MachineConfiguration configuration, int port) => GetControllerTypes(configuration);
    int GetControllerPortCount(MachineConfiguration configuration) => ModelCatalog.Get(configuration.Model).ControllerPortCount;
    string? GetControllerPortDisplayName(MachineConfiguration configuration, int port) => null;
    IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, ControllerType type, int port) => null;
}
