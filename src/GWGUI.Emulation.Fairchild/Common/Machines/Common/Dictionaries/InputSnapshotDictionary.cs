namespace GWGUI.Emulation.Fairchild.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineModel model, Emulator core, int port) =>
        new Engine().Adapter(core) is IEmulatorInputMapper mapper ? mapper.DescribeInput(model, port) : [];
}
