namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineModel model, Emulator core, int port) => new Engine().Adapter(core) is IEmulatorControllerAdapter mapper ? mapper.DescribeInput(model, port) : [];
}
