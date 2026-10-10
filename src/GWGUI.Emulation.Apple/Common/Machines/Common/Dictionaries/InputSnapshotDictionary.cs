namespace GWGUI.Emulation.Apple.Common.Machines.Common.Dictionaries;

internal static class InputSnapshotDictionary
{
    internal static readonly IReadOnlyDictionary<string, int> CommonButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [InputSnapshotFunctionsConstants.Fire1] = EmulationInputMappingConstants.AIndex,
            [EmulationControllerCommandIds.Fire2] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.B] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.Y] = EmulationInputMappingConstants.YIndex,
            [EmulationInputMappingConstants.Select] = EmulationInputMappingConstants.SelectIndex,
            [EmulationInputMappingConstants.Start] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Up] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.Down] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.Left] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.Right] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.A] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.X] = EmulationInputMappingConstants.XIndex,
            [EmulationInputMappingConstants.L] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.R] = EmulationInputMappingConstants.RIndex,
            [EmulationInputMappingConstants.L2] = EmulationInputMappingConstants.L2Index,
            [EmulationInputMappingConstants.R2] = EmulationInputMappingConstants.R2Index,
            [EmulationInputMappingConstants.L3] = EmulationInputMappingConstants.L3Index,
            [EmulationInputMappingConstants.R3] = EmulationInputMappingConstants.R3Index
        };
}
