namespace GWGUI.Emulation.Philips.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineModel model, Emulator core, int port, PeripheralCategory peripheral) => new Engine().Adapter(core) is IEmulatorControllerAdapter mapper ? mapper.DescribeInput(model, port, peripheral) : [];
}
