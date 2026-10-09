namespace GWGUI.Emulation.Sony.Common.Machines.PocketStation.Functions;

internal static class ControllerFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> Definitions { get; } =
    [
        new(EmulationControllerCommandIds.Up,
            InputSettingsFunctionsConstants.ResourceControllerActionUp, string.Empty),
        new(EmulationControllerCommandIds.Down,
            InputSettingsFunctionsConstants.ResourceControllerActionDown, string.Empty),
        new(EmulationControllerCommandIds.Left,
            InputSettingsFunctionsConstants.ResourceControllerActionLeft, string.Empty),
        new(EmulationControllerCommandIds.Right,
            InputSettingsFunctionsConstants.ResourceControllerActionRight, string.Empty),
        new(EmulationControllerCommandIds.A,
            InputSettingsFunctionsConstants.ResourceControllerActionFire1, string.Empty)
    ];
}
