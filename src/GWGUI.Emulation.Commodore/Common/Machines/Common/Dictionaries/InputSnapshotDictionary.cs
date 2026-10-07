namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static readonly IReadOnlyDictionary<string, int> ButtonIndexes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.B] = (int)ControllerButton.B,
            [InputSnapshotFunctionsConstants.Y] = (int)ControllerButton.Y,
            [InputSnapshotFunctionsConstants.Select] = (int)ControllerButton.Select,
            [InputSnapshotFunctionsConstants.Start] = (int)ControllerButton.Start,
            [InputSnapshotFunctionsConstants.Up] = (int)ControllerButton.Up,
            [InputSnapshotFunctionsConstants.Down] = (int)ControllerButton.Down,
            [InputSnapshotFunctionsConstants.Left] = (int)ControllerButton.Left,
            [InputSnapshotFunctionsConstants.Right] = (int)ControllerButton.Right,
            [InputSnapshotFunctionsConstants.A] = (int)ControllerButton.A,
            [InputSnapshotFunctionsConstants.X] = (int)ControllerButton.X,
            [InputSnapshotFunctionsConstants.L] = (int)ControllerButton.L,
            [InputSnapshotFunctionsConstants.R] = (int)ControllerButton.R,
            [InputSnapshotFunctionsConstants.L2] = (int)ControllerButton.L2,
            [InputSnapshotFunctionsConstants.R2] = (int)ControllerButton.R2,
            [InputSnapshotFunctionsConstants.L3] = (int)ControllerButton.L3,
            [InputSnapshotFunctionsConstants.R3] = (int)ControllerButton.R3
        };

    internal static readonly IReadOnlyDictionary<string, MouseAction> DefaultMouseMappings =
        new Dictionary<string, MouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.MouseLeft] = MouseAction.LeftButton,
            [InputSnapshotFunctionsConstants.MouseRight] = MouseAction.RightButton,
            [InputSnapshotFunctionsConstants.MouseMiddle] = MouseAction.MiddleButton
        };
}
