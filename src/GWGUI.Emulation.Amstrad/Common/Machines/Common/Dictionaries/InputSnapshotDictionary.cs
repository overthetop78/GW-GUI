namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static readonly IReadOnlyDictionary<string, int> ButtonIndexes =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.B] = 0,
            [InputSnapshotFunctionsConstants.Y] = 1,
            [InputSnapshotFunctionsConstants.Select] = 2,
            [InputSnapshotFunctionsConstants.Start] = 3,
            [InputSnapshotFunctionsConstants.Up] = 4,
            [InputSnapshotFunctionsConstants.Down] = 5,
            [InputSnapshotFunctionsConstants.Left] = 6,
            [InputSnapshotFunctionsConstants.Right] = 7,
            [InputSnapshotFunctionsConstants.A] = 8,
            [InputSnapshotFunctionsConstants.X] = 9,
            [InputSnapshotFunctionsConstants.L] = 10,
            [InputSnapshotFunctionsConstants.R] = 11,
            [InputSnapshotFunctionsConstants.L2] = 12,
            [InputSnapshotFunctionsConstants.R2] = 13,
            [InputSnapshotFunctionsConstants.L3] = 14,
            [InputSnapshotFunctionsConstants.R3] = 15
        };

    internal static readonly IReadOnlyDictionary<string, MouseAction> DefaultMouseMappings =
        new Dictionary<string, MouseAction>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.MouseLeft] = MouseAction.LeftButton,
            [InputSnapshotFunctionsConstants.MouseRight] = MouseAction.RightButton,
            [InputSnapshotFunctionsConstants.MouseMiddle] = MouseAction.MiddleButton
        };
}
