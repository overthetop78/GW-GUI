namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static readonly IReadOnlyDictionary<string, int> ButtonIndexes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.B] = InputSnapshotFunctionsConstants.BButtonIndex,
            [InputSnapshotFunctionsConstants.Y] = InputSnapshotFunctionsConstants.YButtonIndex,
            [InputSnapshotFunctionsConstants.Select] = InputSnapshotFunctionsConstants.SelectButtonIndex,
            [InputSnapshotFunctionsConstants.Start] = InputSnapshotFunctionsConstants.StartButtonIndex,
            [InputSnapshotFunctionsConstants.Up] = InputSnapshotFunctionsConstants.UpButtonIndex,
            [InputSnapshotFunctionsConstants.Down] = InputSnapshotFunctionsConstants.DownButtonIndex,
            [InputSnapshotFunctionsConstants.Left] = InputSnapshotFunctionsConstants.LeftButtonIndex,
            [InputSnapshotFunctionsConstants.Right] = InputSnapshotFunctionsConstants.RightButtonIndex,
            [InputSnapshotFunctionsConstants.A] = InputSnapshotFunctionsConstants.AButtonIndex,
            [InputSnapshotFunctionsConstants.X] = InputSnapshotFunctionsConstants.XButtonIndex,
            [InputSnapshotFunctionsConstants.L] = InputSnapshotFunctionsConstants.LButtonIndex,
            [InputSnapshotFunctionsConstants.R] = InputSnapshotFunctionsConstants.RButtonIndex,
            [InputSnapshotFunctionsConstants.L2] = InputSnapshotFunctionsConstants.L2ButtonIndex,
            [InputSnapshotFunctionsConstants.R2] = InputSnapshotFunctionsConstants.R2ButtonIndex,
            [InputSnapshotFunctionsConstants.L3] = InputSnapshotFunctionsConstants.L3ButtonIndex,
            [InputSnapshotFunctionsConstants.R3] = InputSnapshotFunctionsConstants.R3ButtonIndex
        };

    internal static readonly IReadOnlyDictionary<string, MouseAction> DefaultMouseMappings =
        new Dictionary<string, MouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.MouseLeft] = MouseAction.LeftButton,
            [InputSnapshotFunctionsConstants.MouseRight] = MouseAction.RightButton,
            [InputSnapshotFunctionsConstants.MouseMiddle] = MouseAction.MiddleButton
        };
}
