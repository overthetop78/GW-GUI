namespace GWGUI.Emulation.Coleco.Common.Machines.Common.Dictionaries;

internal static class ControllerProtocolDictionary
{
    internal static readonly IReadOnlyList<int> GearKeypad = [1, 9, 10, 11, 12, 13, 14, 15];
    internal static readonly IReadOnlyList<int> BlueKeypad = [9, 1, 11, 10, 13, 12, 15, 14];
    internal static readonly IReadOnlyDictionary<int, int> JollyKeypad = new Dictionary<int, int> { [1] = 0, [2] = 8, [3] = 1, [4] = 9, [5] = 0, [6] = 8, [7] = 1, [8] = 9, [9] = 2, [0] = 2 };
    internal const int JollyKeypadFirstModifier = 12, JollyKeypadSecondModifier = 13;
    internal static int Button(Emulator core, string action) => action switch
    {
        ControllerActionConstants.Up => 4, ControllerActionConstants.Down => 5,
        ControllerActionConstants.Left => 6, ControllerActionConstants.Right => 7,
        ControllerActionConstants.FireLeft => 0, ControllerActionConstants.FireRight => 8,
        ControllerActionConstants.Purple when core == Emulator.JollyCV => 1,
        ControllerActionConstants.Blue when core == Emulator.JollyCV => 9,
        ControllerActionConstants.Star => core == Emulator.GearColeco ? 3 : 2,
        ControllerActionConstants.Pound => core == Emulator.GearColeco ? 2 : 3,
        ControllerActionConstants.SpinnerLeft when core == Emulator.JollyCV => 11,
        ControllerActionConstants.SpinnerRight when core == Emulator.JollyCV => 10,
        _ => InputMappingConstants.NoButton
    };
}
