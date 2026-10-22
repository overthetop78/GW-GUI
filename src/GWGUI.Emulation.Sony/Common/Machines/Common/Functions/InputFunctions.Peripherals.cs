namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static readonly string[] BuzzColors = ["Red", "Blue", "Orange", "Green", "Yellow"];

    private static IReadOnlyList<InputBindingDefinition> GripDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3
            or EmulationControllerCommandIds.A or EmulationControllerCommandIds.B or EmulationControllerCommandIds.X or EmulationControllerCommandIds.Y)),
        new("Grip.TriangleLeft", string.Empty, string.Empty, "△ ◀"),
        new("Grip.TriangleRight", string.Empty, string.Empty, "△ ▶"),
        new("Grip.SquareLeft", string.Empty, string.Empty, "□ ◀"),
        new("Grip.SquareRight", string.Empty, string.Empty, "□ ▶")
    ];

    private static IReadOnlyList<InputBindingDefinition> BiohazardDefinitions() => PlayStationButtonDefinitions()
        .Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3 or EmulationControllerCommandIds.Y)).ToArray();

    private static IReadOnlyList<InputBindingDefinition> GloveDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)),
        new("Glove.Reset", string.Empty, string.Empty, "RESET"),
        new("Glove.RealityQuest", string.Empty, string.Empty, "Reality Quest")
    ];

    private static IReadOnlyList<InputBindingDefinition> SphereDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3
            or EmulationControllerCommandIds.R or EmulationControllerCommandIds.R2)),
        .. new[] { "Tx", "Ty", "Tz", "Rx", "Ry", "Rz" }.SelectMany(axis => new InputBindingDefinition[]
        {
            new("Sphere." + axis + "Negative", "Emulation.Sony.Input.Sphere." + axis + "Negative", string.Empty)
                { OppositeDirectionId = "Sphere." + axis + "Positive" },
            new("Sphere." + axis + "Positive", "Emulation.Sony.Input.Sphere." + axis + "Positive", string.Empty)
                { OppositeDirectionId = "Sphere." + axis + "Negative" }
        }),
        .. Enumerable.Range(0, 4).Select(mode => new InputBindingDefinition("Sphere.Mode" + mode,
            string.Empty, string.Empty, "MODE " + mode))
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? SpecializedVisualCommands(PeripheralCategory type, Model model)
    {
        if (type is not (PeripheralCategory.Grip or PeripheralCategory.BiohazardPad or PeripheralCategory.SphereController or PeripheralCategory.Glove)) return null;
        var commands = new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(PeripheralCategory.Joystick, model)!);
        if (type == PeripheralCategory.Grip)
        {
            commands[EmulationControllerVisualControl.FaceX] = "Grip.TriangleLeft";
            commands[EmulationControllerVisualControl.FaceY] = "Grip.TriangleRight";
            commands[EmulationControllerVisualControl.FaceA] = "Grip.SquareLeft";
            commands[EmulationControllerVisualControl.FaceB] = "Grip.SquareRight";
        }
        if (type == PeripheralCategory.Glove)
        {
            commands[EmulationControllerVisualControl.Reset] = "Glove.Reset";
            commands[EmulationControllerVisualControl.Guide] = "Glove.RealityQuest";
        }
        if (type == PeripheralCategory.SphereController)
            for (var mode = 0; mode <= 3; mode++)
                commands[Enum.Parse<EmulationControllerVisualControl>("Key" + mode)] = "Sphere.Mode" + mode;
        return commands;
    }

    private static IReadOnlyList<InputBindingDefinition> BuzzDefinitions() => Enumerable.Range(1, 4)
        .SelectMany(player => BuzzColors.Select(color => new InputBindingDefinition(
            $"Buzz{player}.{color}", $"Emulation.Sony.Input.Buzz.{player}.{color}", string.Empty))).ToArray();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> BuzzVisualCommands() => Enumerable.Range(1, 4)
        .SelectMany(player => BuzzColors.Select(color => (Control: Enum.Parse<EmulationControllerVisualControl>($"Buzz{player}{color}"), Command: $"Buzz{player}.{color}")))
        .ToDictionary(item => item.Control, item => item.Command);

    private static IReadOnlyList<InputBindingDefinition> JogConDefinitions() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)),
        new("JogCon.Mode", "MODE", string.Empty, "MODE"),
        new("JogCon.Left", InputSettingsFunctionsConstants.ResourceSteeringLeft, string.Empty) { OppositeDirectionId = "JogCon.Right" },
        new("JogCon.Right", InputSettingsFunctionsConstants.ResourceSteeringRight, string.Empty) { OppositeDirectionId = "JogCon.Left" }
    ];

    private static IReadOnlyList<InputBindingDefinition> VolumeDefinitions() =>
    [
        new("Volume.A", "A", string.Empty, "A"),
        new("Volume.B", "B", string.Empty, "B"),
        new("Volume.Left", InputSettingsFunctionsConstants.ResourceSteeringLeft, string.Empty) { OppositeDirectionId = "Volume.Right" },
        new("Volume.Right", InputSettingsFunctionsConstants.ResourceSteeringRight, string.Empty) { OppositeDirectionId = "Volume.Left" }
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> VolumeVisualCommands() => new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.FaceA] = "Volume.A",
        [EmulationControllerVisualControl.FaceB] = "Volume.B",
        [EmulationControllerVisualControl.RotaryLeft] = "Volume.Left",
        [EmulationControllerVisualControl.RotaryRight] = "Volume.Right"
    };

    private static IReadOnlyList<InputBindingDefinition> GuitarDefinitions() =>
    [
        new("Guitar.Green", "Guitar.Green", string.Empty, "🟩"),
        new("Guitar.Red", "Guitar.Red", string.Empty, "🟥"),
        new("Guitar.Yellow", "Guitar.Yellow", string.Empty, "🟨"),
        new("Guitar.Blue", "Guitar.Blue", string.Empty, "🟦"),
        new("Guitar.Orange", "Guitar.Orange", string.Empty, "🟧"),
        new("Guitar.StrumUp", "Emulation.Sony.Input.Guitar.StrumUp", string.Empty),
        new("Guitar.StrumDown", "Emulation.Sony.Input.Guitar.StrumDown", string.Empty),
        new("Guitar.Whammy", "Emulation.Sony.Input.Guitar.Whammy", string.Empty),
        new("Guitar.Tilt", "Emulation.Sony.Input.Guitar.Tilt", string.Empty),
        Definition(EmulationControllerCommandIds.Start, InputSettingsFunctionsConstants.ControllerStart, string.Empty),
        Definition(EmulationControllerCommandIds.Select, InputSettingsFunctionsConstants.ControllerSelect, string.Empty)
    ];

    private static IReadOnlyDictionary<string, IReadOnlyList<InputBindingDefinition>>? PeripheralVisualDefinitions(PeripheralCategory type) => type switch
    {
        PeripheralCategory.SerialWheel => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyMadCatzWheel] = MadCatzWheelDefinitions(),
            [EmulationControllerVisualIds.SonyInteractV3] = InteractV3Definitions(),
            [EmulationControllerVisualIds.SonyFanatecSpeedster2] = Speedster2Definitions(),
            [EmulationControllerVisualIds.SonyFanatecSpeedster3] = Speedster3Definitions()
        },
        PeripheralCategory.UsbTrainController => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyDenshaUsb] = UsbTrainDefinitions(),
            [EmulationControllerVisualIds.SonyDenshaShinkansen] = UsbTrainDefinitions(),
            [EmulationControllerVisualIds.SonyDenshaRyojohen] = RyojohenDefinitions(),
            [EmulationControllerVisualIds.SonyTrainMascon] = TrainMasconDefinitions(false),
            [EmulationControllerVisualIds.SonyMultiTrain] = TrainMasconDefinitions(true)
        },
        PeripheralCategory.TrainController => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyDenshaController] = TrainTwoHandleDefinitions(),
            [EmulationControllerVisualIds.SonyDenshaOneHandle] = TrainOneHandleDefinitions(),
            [EmulationControllerVisualIds.SonyDenshaMamecon] = TrainTwoHandleDefinitions()
        },
        PeripheralCategory.Joystick => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyLogitechCordless] = LogitechCordlessDefinitions(),
            [EmulationControllerVisualIds.SonyLogitechCordlessAction] = LogitechCordlessDefinitions(),
            [EmulationControllerVisualIds.SonySegaSaturnPs2] = SegaSaturnPs2Definitions(),
            [EmulationControllerVisualIds.SonyAsciiSpecializedPad] = DigitalDefinitionsWithSlow(),
            [EmulationControllerVisualIds.SonyAsciiEnhancedPad] = DigitalDefinitionsWithSlow(),
            [EmulationControllerVisualIds.SonyHoriFightingCommander10B] = DigitalDefinitionsWithSlow(),
            [EmulationControllerVisualIds.SonyHoriFightingCommander2Way] = PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray()
        },
        PeripheralCategory.ArcadeStick => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyHoriFightingStick2] = HoriFightingStick2Definitions(),
            [EmulationControllerVisualIds.SonyHoriRealArcadePro] = HoriRealArcadeProDefinitions(),
            [EmulationControllerVisualIds.SonyAsciiArcadeStick] = DigitalDefinitionsWithSlow(),
            [EmulationControllerVisualIds.SonyAsciiFighterStickV] = DigitalDefinitionsWithSlow()
        },
        PeripheralCategory.Guitar => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyGuitarHeroSg] = GuitarDefinitions(),
            [EmulationControllerVisualIds.SonyKramerStriker] = GuitarDefinitions(),
            [EmulationControllerVisualIds.SonyGuitarFreaks] = GuitarFreaksDefinitions()
        },
        PeripheralCategory.UsbWheel => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyGtForce] = GtForceDefinitions(),
            [EmulationControllerVisualIds.SonyDrivingForce] = DrivingForceDefinitions(),
            [EmulationControllerVisualIds.SonyDrivingForcePro] = DrivingForceProDefinitions(),
            [EmulationControllerVisualIds.SonyDrivingForceGt] = DrivingForceGtDefinitions()
        },
        PeripheralCategory.DanceMat => new Dictionary<string, IReadOnlyList<InputBindingDefinition>>
        {
            [EmulationControllerVisualIds.SonyKonamiDdr] = KonamiDdrDefinitions()
        },
        _ => null
    };

    private static IReadOnlyList<InputBindingDefinition> DigitalDefinitionsWithSlow() =>
    [
        .. PlayStationButtonDefinitions().Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)),
        new("Slow", "SLOW", string.Empty, "SLOW")
    ];

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> GuitarVisualCommands() => new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.PrimaryAction] = "Guitar.Pick",
        [EmulationControllerVisualControl.FretGreen] = "Guitar.Green",
        [EmulationControllerVisualControl.FretRed] = "Guitar.Red",
        [EmulationControllerVisualControl.FretYellow] = "Guitar.Yellow",
        [EmulationControllerVisualControl.FretBlue] = "Guitar.Blue",
        [EmulationControllerVisualControl.FretOrange] = "Guitar.Orange",
        [EmulationControllerVisualControl.StrumUp] = "Guitar.StrumUp",
        [EmulationControllerVisualControl.StrumDown] = "Guitar.StrumDown",
        [EmulationControllerVisualControl.Whammy] = "Guitar.Whammy",
        [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
        [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select
    };
}
