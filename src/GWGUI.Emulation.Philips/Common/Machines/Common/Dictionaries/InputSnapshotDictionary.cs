namespace GWGUI.Emulation.Philips.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyList<InputActionDefinition> Actions(MachineModel model, Emulator core, PeripheralCategory peripheral) => new Engine().Adapter(core) is IEmulatorInputMapper mapper ? mapper.DescribeInput(model, peripheral) : [];
}
