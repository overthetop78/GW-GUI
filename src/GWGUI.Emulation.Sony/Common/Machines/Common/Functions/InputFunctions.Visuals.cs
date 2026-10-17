namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    private static IReadOnlyList<string>? CompatibleVisualIds(ControllerType type, Model model) => type == ControllerType.DanceMat
        ? [EmulationControllerVisualIds.SonyDanceMat, EmulationControllerVisualIds.SonyKonamiDdr] : model.Id is ModelConstants.PlayStation or ModelConstants.PlayStation2 ? type switch
        {
            ControllerType.Joystick => model.Id == ModelConstants.PlayStation2
                ? [EmulationControllerVisualIds.SonyDualShock2, EmulationControllerVisualIds.SonyDualShock1, EmulationControllerVisualIds.SonyPlayStationController, EmulationControllerVisualIds.SonyDualAnalog, EmulationControllerVisualIds.SonyAnalogJoystick, EmulationControllerVisualIds.SonyAsciiSpecializedPad, EmulationControllerVisualIds.SonyAsciiEnhancedPad, EmulationControllerVisualIds.SonyHoriFightingCommander2Way, EmulationControllerVisualIds.SonyHoriFightingCommander10B, EmulationControllerVisualIds.SonyLogitechCordless, EmulationControllerVisualIds.SonyLogitechCordlessAction, EmulationControllerVisualIds.SonySegaSaturnPs2]
                : [EmulationControllerVisualIds.SonyPlayStationController, EmulationControllerVisualIds.SonyAsciiSpecializedPad, EmulationControllerVisualIds.SonyAsciiEnhancedPad, EmulationControllerVisualIds.SonyHoriFightingCommander2Way, EmulationControllerVisualIds.SonyHoriFightingCommander10B, EmulationControllerVisualIds.SonyLogitechCordless, EmulationControllerVisualIds.SonySegaSaturnPs2],
            ControllerType.DualShock => [EmulationControllerVisualIds.SonyDualShock1],
            ControllerType.AnalogController => [EmulationControllerVisualIds.SonyDualAnalog],
            ControllerType.AnalogJoystick => [EmulationControllerVisualIds.SonyAnalogJoystick],
            ControllerType.Mouse => [EmulationControllerVisualIds.SonyPlayStationMouse],
            ControllerType.GunCon2 => [EmulationControllerVisualIds.SonyGunCon2],
            ControllerType.NeGcon or ControllerType.NeGconRumble => [EmulationControllerVisualIds.SonyNeGcon],
            ControllerType.GunCon => [EmulationControllerVisualIds.SonyGunCon],
            ControllerType.Justifier => [EmulationControllerVisualIds.SonyJustifier],
            _ => null
        } : type == ControllerType.GunCon2 && model.Id == ModelConstants.PlayStation2
            ? [EmulationControllerVisualIds.SonyGunCon2]
            : type is not (ControllerType.Joystick or ControllerType.DualShock)
            ? null : model.Id switch
        {
            ModelConstants.PlayStation => type == ControllerType.DualShock ? [EmulationControllerVisualIds.SonyDualShock1] : [EmulationControllerVisualIds.SonyPlayStationController],
            ModelConstants.PlayStation2 => [EmulationControllerVisualIds.SonyDualShock2, EmulationControllerVisualIds.SonyDualShock1,
                EmulationControllerVisualIds.SonyPlayStationController, EmulationControllerVisualIds.SonyDualAnalog],
            ModelConstants.PlayStation3 => [EmulationControllerVisualIds.SonyDualShock3],
            ModelConstants.PocketStation => [EmulationControllerVisualIds.SonyPocketStation],
            ModelConstants.Psp => [EmulationControllerVisualIds.SonyPsp1000, EmulationControllerVisualIds.SonyPsp2000, EmulationControllerVisualIds.SonyPsp3000, EmulationControllerVisualIds.SonyPspGo],
            ModelConstants.PlayStation4 => [EmulationControllerVisualIds.SonyDualShock4],
            ModelConstants.PlayStation5 => [EmulationControllerVisualIds.SonyDualSense],
            _ => null
        };

    private static string? DefaultVisualId(ControllerType type, Model model) => CompatibleVisualIds(type, model)?.FirstOrDefault();

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>?
        VisualCommandIds(ControllerType type, Model model) =>
            SpecializedVisualCommands(type, model) is { } specialized ? specialized
            : SpecialControllerVisualCommands(type, model) is { } specialController ? specialController
            : RemoteAndFishingVisualCommands(type, model) is { } remoteAndFishing ? remoteAndFishing
            : TrainAndJetVisualCommands(type, model) is { } trainAndJet ? trainAndJet
            : RarePeripheralVisualCommands(type) is { } rarePeripheral ? rarePeripheral
            : InstrumentVisualCommands(type) is { } instrument ? instrument
            : MusicVisualCommands(type) is { } music ? music
            : type == ControllerType.BuzzBuzzers ? BuzzVisualCommands()
            : type == ControllerType.Seamic ? SeamicVisualCommands()
            : type == ControllerType.VolumeController ? VolumeVisualCommands()
            : type == ControllerType.Guitar ? GuitarVisualCommands()
            : type == ControllerType.SerialWheel ? SerialWheelVisualCommands()
            : type == ControllerType.UsbWheel ? WheelVisualCommands()
            : type == ControllerType.ArcadeStick ? ArcadeVisualCommands(model)
            : type == ControllerType.NetPlayController ? new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(ControllerType.Joystick, model)!)
            {
                [EmulationControllerVisualControl.Guide] = "NetPlay.Vibration"
            }
            : type == ControllerType.JogCon ? new Dictionary<EmulationControllerVisualControl, string>(VisualCommandIds(ControllerType.Joystick, model)!)
            {
                [EmulationControllerVisualControl.AnalogMode] = "JogCon.Mode",
                [EmulationControllerVisualControl.Steering] = "JogCon.Left"
            }
            : type == ControllerType.DanceMat ? new Dictionary<EmulationControllerVisualControl, string>
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
            } : type is ControllerType.Mouse or ControllerType.SerialMouse ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.MouseLeftButton] = nameof(MouseAction.LeftButton),
                [EmulationControllerVisualControl.MouseRightButton] = nameof(MouseAction.RightButton)
            } : type == ControllerType.GunCon2 ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.QuaternaryAction] = EmulationControllerCommandIds.Y,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start,
                [EmulationControllerVisualControl.Option] = EmulationControllerCommandIds.Select
            } : type is ControllerType.GunCon or ControllerType.Justifier ? new Dictionary<EmulationControllerVisualControl, string>
            {
                [EmulationControllerVisualControl.PrimaryAction] = EmulationControllerCommandIds.B,
                [EmulationControllerVisualControl.SecondaryAction] = EmulationControllerCommandIds.A,
                [EmulationControllerVisualControl.TertiaryAction] = EmulationControllerCommandIds.X,
                [EmulationControllerVisualControl.Start] = EmulationControllerCommandIds.Start
            } : type is ControllerType.NeGcon or ControllerType.NeGconRumble ? new Dictionary<EmulationControllerVisualControl, string>
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
            } : type is not (ControllerType.Joystick or ControllerType.DualShock or ControllerType.ArcadeStick
                or ControllerType.AnalogController or ControllerType.AnalogJoystick)
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

    private static string ControllerResourceKey(ControllerType type, Model model) =>
        type == ControllerType.Joystick ? model.Id switch
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
        ControllerType.Automatic => InputSettingsFunctionsConstants.ResourceControllerAutomatic,
        ControllerType.None => InputSettingsFunctionsConstants.ResourceControllerNone,
        ControllerType.DanceMat => "Emulation.Sony.Controller.DanceMat",
        ControllerType.DualShock => InputSettingsFunctionsConstants.ResourceDualShock,
        ControllerType.AnalogController => InputSettingsFunctionsConstants.ResourceDualAnalog,
        ControllerType.AnalogJoystick => InputSettingsFunctionsConstants.ResourceAnalogJoystick,
        ControllerType.GunCon => InputSettingsFunctionsConstants.ResourceGunCon,
        ControllerType.GunCon2 => InputSettingsFunctionsConstants.ResourceGunCon2,
        ControllerType.Justifier => InputSettingsFunctionsConstants.ResourceJustifier,
        ControllerType.Mouse => InputSettingsFunctionsConstants.ResourcePlayStationMouse,
        ControllerType.Keyboard => InputSettingsFunctionsConstants.ResourceControllerKeyboard,
        ControllerType.NeGcon or ControllerType.NeGconRumble or ControllerType.KeyboardAndMouse
            => string.Empty,
        ControllerType.SerialMouse => InputSettingsFunctionsConstants.ResourcePlayStationMouse,
        _ => "Emulation.Sony.Controller." + type
    };

    private static IReadOnlyDictionary<string, string> ToStrings(IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(item => item.Key, item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();

    private static IReadOnlyDictionary<string, EmulationKey> ToKeys(IReadOnlyDictionary<string, string> values) => values.Where(item => Enum.TryParse<EmulationKey>(item.Value, true, out _)).ToDictionary(item => item.Key, item => Enum.Parse<EmulationKey>(item.Value, true), StringComparer.Ordinal);
}
