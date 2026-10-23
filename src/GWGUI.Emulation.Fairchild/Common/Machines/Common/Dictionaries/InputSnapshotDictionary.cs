namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineConfiguration configuration, int port) => new Engine().Adapter(configuration) is IEmulatorControllerAdapter mapper ? mapper.DescribeInput(configuration, port, PeripheralCategory.Standard) : [];
}
