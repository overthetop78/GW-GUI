namespace GWGUI.Emulation.Bandai.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static IReadOnlyDictionary<string, int> ForModel(MachineModel model) => model switch
    {
        MachineModel.WonderSwan or MachineModel.WonderSwanColor or MachineModel.SwanCrystal => WonderSwanButtons,
        MachineModel.Playdia => PlaydiaButtons,
        MachineModel.TamagotchiP1 => TamagotchiButtons,
        _ => throw new ArgumentOutOfRangeException(nameof(model))
    };

    private static readonly IReadOnlyDictionary<string, int> WonderSwanButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.X1] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Up),
            [InputSnapshotFunctionsConstants.X2] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Right),
            [InputSnapshotFunctionsConstants.X3] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Down),
            [InputSnapshotFunctionsConstants.X4] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Left),
            [InputSnapshotFunctionsConstants.Y1] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.R2),
            [InputSnapshotFunctionsConstants.Y2] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.R),
            [InputSnapshotFunctionsConstants.Y3] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.L2),
            [InputSnapshotFunctionsConstants.Y4] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.L),
            [EmulationControllerCommandIds.A] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.A),
            [EmulationControllerCommandIds.B] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.B),
            [EmulationControllerCommandIds.Start] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Start),
            [InputSnapshotFunctionsConstants.Rotate] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Select)
        };

    private static readonly IReadOnlyDictionary<string, int> PlaydiaButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationControllerCommandIds.Up] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Up),
            [EmulationControllerCommandIds.Down] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Down),
            [EmulationControllerCommandIds.Left] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Left),
            [EmulationControllerCommandIds.Right] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Right),
            [EmulationControllerCommandIds.A] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.A),
            [EmulationControllerCommandIds.B] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.B),
            [EmulationControllerCommandIds.Start] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Start),
            [EmulationControllerCommandIds.Select] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Select)
        };

    private static readonly IReadOnlyDictionary<string, int> TamagotchiButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationControllerCommandIds.Select] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.Y),
            [InputSnapshotFunctionsConstants.Execute] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.B),
            [InputSnapshotFunctionsConstants.Cancel] = EmulationSdk.InputProcessing.ControllerButtonIndex(EmulationControllerCommandIds.A)
        };
}
