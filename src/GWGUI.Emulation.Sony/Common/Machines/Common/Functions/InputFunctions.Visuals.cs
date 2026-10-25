namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(PeripheralCategory type, Model model) => type == PeripheralCategory.DanceMat
        ? [EmulationControllerVisualIds.SonyDanceMat, EmulationControllerVisualIds.SonyKonamiDdr] : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2 ? type switch
        {
            PeripheralCategory.Joystick => model.Id == ModelConstants.PlayStation2
                ? [EmulationControllerVisualIds.SonyDualShock2, EmulationControllerVisualIds.SonyDualShock1, EmulationControllerVisualIds.SonyPlayStationController, EmulationControllerVisualIds.SonyDualAnalog, EmulationControllerVisualIds.SonyAnalogJoystick, EmulationControllerVisualIds.SonyAsciiSpecializedPad, EmulationControllerVisualIds.SonyAsciiEnhancedPad, EmulationControllerVisualIds.SonyHoriFightingCommander2Way, EmulationControllerVisualIds.SonyHoriFightingCommander10B, EmulationControllerVisualIds.SonyLogitechCordless, EmulationControllerVisualIds.SonyLogitechCordlessAction, EmulationControllerVisualIds.SonySegaSaturnPs2]
                : [EmulationControllerVisualIds.SonyPlayStationController, EmulationControllerVisualIds.SonyAsciiSpecializedPad, EmulationControllerVisualIds.SonyAsciiEnhancedPad, EmulationControllerVisualIds.SonyHoriFightingCommander2Way, EmulationControllerVisualIds.SonyHoriFightingCommander10B, EmulationControllerVisualIds.SonyLogitechCordless, EmulationControllerVisualIds.SonySegaSaturnPs2],
            PeripheralCategory.DualShock => [EmulationControllerVisualIds.SonyDualShock1],
            PeripheralCategory.AnalogController => [EmulationControllerVisualIds.SonyDualAnalog],
            PeripheralCategory.AnalogJoystick => [EmulationControllerVisualIds.SonyAnalogJoystick],
            PeripheralCategory.Mouse => [EmulationControllerVisualIds.SonyPlayStationMouse],
            PeripheralCategory.GunCon2 => [EmulationControllerVisualIds.SonyGunCon2],
            PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble => [EmulationControllerVisualIds.SonyNeGcon],
            PeripheralCategory.GunCon => [EmulationControllerVisualIds.SonyGunCon],
            PeripheralCategory.Justifier => [EmulationControllerVisualIds.SonyJustifier],
            _ => null
        } : type is not (PeripheralCategory.Joystick or PeripheralCategory.DualShock)
            ? null : model.Id switch
        {
            ModelConstants.PlayStation3 => [EmulationControllerVisualIds.SonyDualShock3],
            ModelConstants.PocketStation => [EmulationControllerVisualIds.SonyPocketStation],
            ModelConstants.Psp => [EmulationControllerVisualIds.SonyPsp1000, EmulationControllerVisualIds.SonyPsp2000, EmulationControllerVisualIds.SonyPsp3000, EmulationControllerVisualIds.SonyPspGo],
            ModelConstants.PlayStation4 => [EmulationControllerVisualIds.SonyDualShock4],
            ModelConstants.PlayStation5 => [EmulationControllerVisualIds.SonyDualSense],
            _ => null
        };

    private static string? DefaultVisualId(PeripheralCategory type, Model model) => CompatibleVisualIds(type, model)?.FirstOrDefault();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(PeripheralCategory type, Model model) =>
            SpecializedVisualCommands(type, model) is { } specialized ? specialized
            : SpecialControllerVisualCommands(type, model) is { } specialController ? specialController
            : RemoteAndFishingVisualCommands(type, model) is { } remoteAndFishing ? remoteAndFishing
            : TrainAndJetVisualCommands(type, model) is { } trainAndJet ? trainAndJet
            : RarePeripheralVisualCommands(type) is { } rarePeripheral ? rarePeripheral
            : InstrumentVisualCommands(type) is { } instrument ? instrument
            : MusicVisualCommands(type) is { } music ? music
            : type == PeripheralCategory.BuzzBuzzers ? BuzzVisualCommands()
            : type == PeripheralCategory.Seamic ? SeamicVisualCommands()
            : type == PeripheralCategory.VolumeController ? VolumeVisualCommands()
            : type == PeripheralCategory.Guitar ? GuitarVisualCommands()
            : type == PeripheralCategory.SerialWheel ? SerialWheelVisualCommands()
            : type == PeripheralCategory.UsbWheel ? WheelVisualCommands()
            : type == PeripheralCategory.ArcadeStick ? ArcadeVisualCommands(model)
            : type == PeripheralCategory.NetPlayController ? new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(PeripheralCategory.Joystick, model)!)
            {
                [EmulationControllerVisualControl.Guide] = "NetPlay.Vibration"
            }
            : type == PeripheralCategory.JogCon ? new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(PeripheralCategory.Joystick, model)!)
            {
                [EmulationControllerVisualControl.AnalogMode] = "JogCon.Mode",
                [EmulationControllerVisualControl.Steering] = "JogCon.Left"
            }
            : type == PeripheralCategory.DanceMat ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select
            } : type is PeripheralCategory.Mouse or PeripheralCategory.SerialMouse ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.MouseLeftButton] = nameof(MouseAction.LeftButton),
                [EmulationControllerVisualControl.MouseRightButton] = nameof(MouseAction.RightButton)
            } : type == PeripheralCategory.GunCon2 ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select
            } : type is PeripheralCategory.GunCon or PeripheralCategory.Justifier ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
            } : type is PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.R2,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.L2,
                [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
                [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
                [EmulationControllerVisualControl.Steering] = InputSettingsFunctionsConstants.LeftStickLeft,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
            } : type is not (PeripheralCategory.Joystick or PeripheralCategory.DualShock or PeripheralCategory.ArcadeStick
                or PeripheralCategory.AnalogController or PeripheralCategory.AnalogJoystick)
            ? null : model.Id == ModelConstants.PocketStation ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.A
            }
            : model.Id == ModelConstants.Psp
            ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
                [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
                [EmulationControllerVisualControl.StickUp] = InputSettingsFunctionsConstants.PspStickUp,
                [EmulationControllerVisualControl.StickDown] = InputSettingsFunctionsConstants.PspStickDown,
                [EmulationControllerVisualControl.StickLeft] = InputSettingsFunctionsConstants.PspStickLeft,
                [EmulationControllerVisualControl.StickRight] = InputSettingsFunctionsConstants.PspStickRight
            }
            : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2
                or ModelConstants.PlayStation3
            ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
                [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
                [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
                [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.LeftShoulder] = EmulationControllerCommandIds.L,
                [EmulationControllerVisualControl.RightShoulder] = EmulationControllerCommandIds.R,
                [EmulationControllerVisualControl.LeftTrigger] = EmulationControllerCommandIds.L2,
                [EmulationControllerVisualControl.RightTrigger] = EmulationControllerCommandIds.R2,
                [EmulationControllerVisualControl.LeftStick] = EmulationControllerCommandIds.L3,
                [EmulationControllerVisualControl.RightStick] = EmulationControllerCommandIds.R3,
                [EmulationControllerVisualControl.AnalogMode] = InputSettingsFunctionsConstants.Analog,
                [EmulationControllerVisualControl.VibrationMode] = "Logitech.Vibration",
                [EmulationControllerVisualControl.Slow] = "Slow",
                [EmulationControllerVisualControl.StickUp] = InputSettingsFunctionsConstants.LeftStickUp,
                [EmulationControllerVisualControl.StickDown] = InputSettingsFunctionsConstants.LeftStickDown,
                [EmulationControllerVisualControl.StickLeft] = InputSettingsFunctionsConstants.LeftStickLeft,
                [EmulationControllerVisualControl.StickRight] = InputSettingsFunctionsConstants.LeftStickRight,
                [EmulationControllerVisualControl.RightStickUp] = InputSettingsFunctionsConstants.RightStickUp,
                [EmulationControllerVisualControl.RightStickDown] = InputSettingsFunctionsConstants.RightStickDown,
                [EmulationControllerVisualControl.RightStickLeft] = InputSettingsFunctionsConstants.RightStickLeft,
                [EmulationControllerVisualControl.RightStickRight] = InputSettingsFunctionsConstants.RightStickRight
            }
        : new Dictionary<EmulationControllerVisualControl, string>
        {
            [EmulationControllerVisualControl.DirectionUp] = EmulationControllerCommandIds.Up,
            [EmulationControllerVisualControl.DirectionDown] = EmulationControllerCommandIds.Down,
            [EmulationControllerVisualControl.DirectionLeft] = EmulationControllerCommandIds.Left,
            [EmulationControllerVisualControl.DirectionRight] = EmulationControllerCommandIds.Right,
            [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
            [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A
        };

    private static string ControllerResourceKey(PeripheralCategory type, Model model) =>
        type == PeripheralCategory.Joystick ? model.Id switch
        {
            ModelConstants.PlayStation => InputSettingsFunctionsConstants.ResourcePlayStationController,
            ModelConstants.PlayStation2 => InputSettingsFunctionsConstants.ResourceDualShock2,
            ModelConstants.PlayStation3 => InputSettingsFunctionsConstants.ResourceDualShock3,
            ModelConstants.PocketStation => Machines.PocketStation.Constants.InputConstants.IntegratedControllerLabel,
            ModelConstants.Psp => InputSettingsFunctionsConstants.ResourcePspIntegrated,
            ModelConstants.PlayStation4 => InputSettingsFunctionsConstants.ResourceDualShock4,
            ModelConstants.PlayStation5 => InputSettingsFunctionsConstants.ResourceDualSense,
            _ => InputSettingsFunctionsConstants.ResourceControllerJoystick
        } : type switch
    {
        PeripheralCategory.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        PeripheralCategory.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        PeripheralCategory.DanceMat => "Emulation.Sony.Controller.DanceMat",
        PeripheralCategory.DualShock => InputSettingsFunctionsConstants.ResourceDualShock,
        PeripheralCategory.AnalogController => InputSettingsFunctionsConstants.ResourceDualAnalog,
        PeripheralCategory.AnalogJoystick => InputSettingsFunctionsConstants.ResourceAnalogJoystick,
        PeripheralCategory.GunCon => InputSettingsFunctionsConstants.ResourceGunCon,
        PeripheralCategory.GunCon2 => InputSettingsFunctionsConstants.ResourceGunCon2,
        PeripheralCategory.Justifier => InputSettingsFunctionsConstants.ResourceJustifier,
        PeripheralCategory.Mouse => InputSettingsFunctionsConstants.ResourcePlayStationMouse,
        PeripheralCategory.Keyboard => InputSettingsFunctionsConstants.ResourceControllerKeyboard,
        PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble or PeripheralCategory.KeyboardAndMouse
            => string.Empty,
        PeripheralCategory.SerialMouse => InputSettingsFunctionsConstants.ResourcePlayStationMouse,
        _ => "Emulation.Sony.Controller." + type
    };
}
