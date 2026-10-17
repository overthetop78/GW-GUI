namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static readonly string[] DvdRemoteFunctions =
    [
        "Audio", "Shuffle", "Angle", "Program", "Subtitle", "Repeat", "SlowBackward", "SlowForward",
        "Clear", "Time", "Previous", "Next", "RepeatAB", "Display", "Title", "Menu", "Return", "Enter"
    ];

    private static IReadOnlyList<InputBindingDefinition> DvdRemoteDefinitions() =>
    [
        .. PlayStationButtonDefinitions(),
        .. Enumerable.Range(0, 10).Select(key => new InputBindingDefinition("Dvd.Key" + key, string.Empty, string.Empty, key.ToString())),
        .. DvdRemoteFunctions.Select(command => new InputBindingDefinition("Dvd." + command, "Emulation.Sony.Input.Remote." + command, string.Empty)),
        new("Dvd.Play", "Emulation.Cassette.Command.Play", string.Empty),
        new("Dvd.Pause", "Common.Pause", string.Empty),
        new("Dvd.Stop", "Common.Stop", string.Empty),
        new("Dvd.Rewind", "Emulation.Cassette.Command.Rewind", string.Empty),
        new("Dvd.FastForward", "Emulation.Cassette.Command.FastForward", string.Empty)
    ];

    private static IReadOnlyList<InputBindingDefinition> FishingDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.Up
            or EmulationControllerCommandIds.Down or EmulationControllerCommandIds.Left or EmulationControllerCommandIds.Right
            or EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)),
        Definition(InputSettingsFunctionsConstants.LeftStickUp, InputSettingsFunctionsConstants.LeftStickUpLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickDown, InputSettingsFunctionsConstants.LeftStickDownLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickLeft, InputSettingsFunctionsConstants.LeftStickLeftLabel, string.Empty),
        Definition(InputSettingsFunctionsConstants.LeftStickRight, InputSettingsFunctionsConstants.LeftStickRightLabel, string.Empty),
        new("Fishing.Center", "Emulation.Sony.Input.Fishing.Center", string.Empty),
        new("Fishing.Reel", "Emulation.Sony.Input.Fishing.Reel", string.Empty),
        new("Fishing.Cast", "Emulation.Sony.Input.Fishing.Cast", string.Empty)
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? RemoteAndFishingVisualCommands(ControllerType type, Model model)
    {
        if (type is not (ControllerType.DvdRemote or ControllerType.FishingController)) return null;
        var commands = new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(ControllerType.Joystick, model)!);
        if (type == ControllerType.FishingController)
        {
            commands[EmulationControllerVisualControl.LeftStick] = "Fishing.Center";
            commands[EmulationControllerVisualControl.RotaryRight] = "Fishing.Reel";
            return commands;
        }
        foreach (var key in Enumerable.Range(0, 10))
            commands[Enum.Parse<EmulationControllerVisualControl>("Key" + key)] = "Dvd.Key" + key;
        foreach (var command in DvdRemoteFunctions)
            commands[Enum.Parse<EmulationControllerVisualControl>(command)] = "Dvd." + command;
        commands[EmulationControllerVisualControl.PlayPause] = "Dvd.Play";
        commands[EmulationControllerVisualControl.Pause] = "Dvd.Pause";
        commands[EmulationControllerVisualControl.Stop] = "Dvd.Stop";
        commands[EmulationControllerVisualControl.Rewind] = "Dvd.Rewind";
        commands[EmulationControllerVisualControl.FastForward] = "Dvd.FastForward";
        return commands;
    }
}
