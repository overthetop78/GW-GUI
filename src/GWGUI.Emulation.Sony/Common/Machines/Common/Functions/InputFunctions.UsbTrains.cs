namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IEnumerable<InputBindingDefinition> TrainDirectionDefinitions() => PlayStationButtonDefinitions()
        .Where(definition => definition.Id is EmulationControllerCommandIds.Up or EmulationControllerCommandIds.Down
            or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right);

    private static InputBindingDefinition TrainHornPedalDefinition() =>
        new("Train.HornPedal", "Emulation.Sony.Input.Train.HornPedal", string.Empty);

    private static IReadOnlyList<InputBindingDefinition> UsbTrainDefinitions() =>
    [
        .. TrainTwoHandleDefinitions(),
        .. TrainDirectionDefinitions(),
        new("Train.D", string.Empty, string.Empty, "D"),
        TrainHornPedalDefinition()
    ];

    private static IReadOnlyList<InputBindingDefinition> RyojohenDefinitions() =>
    [
        .. TrainTwoHandleDefinitions().Where(definition => definition.Id is not ("Train.A" or "Train.B" or "Train.C")),
        .. TrainDirectionDefinitions(),
        new("Train.Horn", "Emulation.Sony.Input.Train.Horn", string.Empty),
        new("Train.Announce", "Emulation.Sony.Input.Train.Announce", string.Empty),
        new("Train.Camera", "Emulation.Sony.Input.Train.Camera", string.Empty),
        new("Train.LeftDoor", "Emulation.Sony.Input.Train.LeftDoor", string.Empty),
        new("Train.RightDoor", "Emulation.Sony.Input.Train.RightDoor", string.Empty),
        TrainHornPedalDefinition()
    ];

    private static IReadOnlyList<InputBindingDefinition> TrainMasconDefinitions(bool multi) =>
    [
        .. TrainOneHandleDefinitions().Select(definition => definition.Id == "Train.A"
            ? new InputBindingDefinition("Train.A", "Emulation.Sony.Input.Train.ASoft", string.Empty) : definition),
        .. TrainDirectionDefinitions(),
        new("Train.AHard", "Emulation.Sony.Input.Train.AHard", string.Empty),
        new("Train.Ats", string.Empty, string.Empty, "ATS"),
        multi ? new("Train.D", string.Empty, string.Empty, "D") : new("Train.Close", "Emulation.Sony.Input.Train.Close", string.Empty),
        new("Train.ReverserForward", "Emulation.Sony.Input.Train.ReverserForward", string.Empty),
        new("Train.ReverserNeutral", "Emulation.Sony.Input.Train.ReverserNeutral", string.Empty),
        new("Train.ReverserBackward", "Emulation.Sony.Input.Train.ReverserBackward", string.Empty)
    ];
}
