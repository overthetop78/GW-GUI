namespace GWGUI.Emulation.Atari.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.Fire1] = 0,
            [InputSnapshotFunctionsConstants.Fire2] = 8,
            [InputSnapshotFunctionsConstants.Turbo] = 9,
            [InputSnapshotFunctionsConstants.Up] = 4,
            [InputSnapshotFunctionsConstants.Down] = 5,
            [InputSnapshotFunctionsConstants.Left] = 6,
            [InputSnapshotFunctionsConstants.Right] = 7,
            [InputSnapshotFunctionsConstants.A] = 8,
            [InputSnapshotFunctionsConstants.B] = 0,
            [InputSnapshotFunctionsConstants.C] = 1,
            [InputSnapshotFunctionsConstants.Pause] = 2,
            [InputSnapshotFunctionsConstants.Option] = 3,
            [InputSnapshotFunctionsConstants.Option1] = 10,
            [InputSnapshotFunctionsConstants.Option2] = 11,
            [InputSnapshotFunctionsConstants.Start] = 3,
            [InputSnapshotFunctionsConstants.Reset] = 9,
            [InputSnapshotFunctionsConstants.Key0] = 9,
            [InputSnapshotFunctionsConstants.Key1] = 10,
            [InputSnapshotFunctionsConstants.Key2] = 11,
            [InputSnapshotFunctionsConstants.Key3] = 12,
            [InputSnapshotFunctionsConstants.Key4] = 13,
            [InputSnapshotFunctionsConstants.Key5] = 14,
            [InputSnapshotFunctionsConstants.Key6] = 15,
            [InputSnapshotFunctionsConstants.Star] = 1,
            [InputSnapshotFunctionsConstants.Hash] = 8
        };
}
