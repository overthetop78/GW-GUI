namespace GWGUI.Emulation.Acorn.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationControllerCommandIds.Fire1] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.A),
            [EmulationControllerCommandIds.B] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.B),
            [EmulationControllerCommandIds.Y] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Y),
            [EmulationControllerCommandIds.Select] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Select),
            [EmulationControllerCommandIds.Start] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Start),
            [EmulationControllerCommandIds.Up] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Up),
            [EmulationControllerCommandIds.Down] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Down),
            [EmulationControllerCommandIds.Left] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Left),
            [EmulationControllerCommandIds.Right] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Right),
            [EmulationControllerCommandIds.A] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.A),
            [EmulationControllerCommandIds.X] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.X),
            [EmulationControllerCommandIds.L] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.L),
            [EmulationControllerCommandIds.R] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.R),
            [EmulationControllerCommandIds.L2] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.L2),
            [EmulationControllerCommandIds.R2] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.R2),
            [EmulationControllerCommandIds.L3] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.L3),
            [EmulationControllerCommandIds.R3] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.R3)
        };
}
