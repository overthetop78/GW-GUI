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
            [InputSnapshotFunctionsConstants.X1] = EmulationInputMappingConstants.UpIndex,
            [InputSnapshotFunctionsConstants.X2] = EmulationInputMappingConstants.RightIndex,
            [InputSnapshotFunctionsConstants.X3] = EmulationInputMappingConstants.DownIndex,
            [InputSnapshotFunctionsConstants.X4] = EmulationInputMappingConstants.LeftIndex,
            [InputSnapshotFunctionsConstants.Y1] = EmulationInputMappingConstants.R2Index,
            [InputSnapshotFunctionsConstants.Y2] = EmulationInputMappingConstants.RIndex,
            [InputSnapshotFunctionsConstants.Y3] = EmulationInputMappingConstants.L2Index,
            [InputSnapshotFunctionsConstants.Y4] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.A] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.B] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.Start] = EmulationInputMappingConstants.StartIndex,
            [InputSnapshotFunctionsConstants.Rotate] = EmulationInputMappingConstants.SelectIndex
        };

    private static readonly IReadOnlyDictionary<string, int> PlaydiaButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationInputMappingConstants.Up] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.Down] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.Left] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.Right] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.A] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.B] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.Start] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Select] = EmulationInputMappingConstants.SelectIndex
        };

    private static readonly IReadOnlyDictionary<string, int> TamagotchiButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationInputMappingConstants.Select] = EmulationInputMappingConstants.YIndex,
            [InputSnapshotFunctionsConstants.Execute] = EmulationInputMappingConstants.BIndex,
            [InputSnapshotFunctionsConstants.Cancel] = EmulationInputMappingConstants.AIndex
        };
}
