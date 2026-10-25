namespace GWGUI.Emulation.Philips.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineConfiguration configuration, int port, PeripheralCategory peripheral) => new Engine().Adapter(configuration) is IEmulatorControllerAdapter mapper ? mapper.DescribeInput(configuration, port, peripheral) : [];
}
