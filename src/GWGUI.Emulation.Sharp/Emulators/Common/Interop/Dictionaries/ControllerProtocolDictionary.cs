namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Dictionaries;

internal sealed record NativeInputAction(ControllerCommand Command)
{
    internal string Id => Command.ToString();
    internal uint ButtonMask => 1u << (int)Command;
}

internal static class ControllerProtocolDictionary
{
    internal static IReadOnlyList<NativeInputAction> Actions(MachineModel model, PeripheralCategory peripheral) => ControllerCatalog.Actions(model, peripheral).Select(action => new NativeInputAction(action.Command)).ToArray();
}
