using GWGUI.Emulation;
using GWGUI.Emulation.Constants;
using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Atari.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
private static IReadOnlyList<string>? CompatibleVisualIds(
        MachineModel model,
        PeripheralCategory category) => category switch
    {
        PeripheralCategory.Joystick when model == MachineModel.Atari2600 =>
            [EmulationControllerVisualIds.AtariCx40],
        PeripheralCategory.Joystick =>
        [
            EmulationControllerVisualIds.QuickShot,
            EmulationControllerVisualIds.QuickShotDeluxe,
            EmulationControllerVisualIds.QuickShotIiTurbo,
            EmulationControllerVisualIds.CompetitionPro5000,
            EmulationControllerVisualIds.ZipstikSuperPro,
            EmulationControllerVisualIds.KonixSpeedkingLeftHand,
            EmulationControllerVisualIds.KonixSpeedkingRightHand,
            EmulationControllerVisualIds.SuncomTac2,
            EmulationControllerVisualIds.PowerplayCruiser,
            EmulationControllerVisualIds.SuzoTheArcadeTurbo,
            EmulationControllerVisualIds.AdvancedGravisGamepad,
            EmulationControllerVisualIds.AtariCx40
        ],
        PeripheralCategory.AnalogJoystick when model == MachineModel.Atari5200 =>
            [EmulationControllerVisualIds.Atari5200Controller],
        PeripheralCategory.Paddle => [EmulationControllerVisualIds.AtariPaddle],
        PeripheralCategory.DrivingController =>
            [EmulationControllerVisualIds.Atari2600DrivingController],
        PeripheralCategory.BoosterGrip => [EmulationControllerVisualIds.AtariBoosterGrip],
        PeripheralCategory.Joy2BPlus => [EmulationControllerVisualIds.AtariJoy2BPlus],
        PeripheralCategory.ProLineController =>
            [EmulationControllerVisualIds.Atari7800ProLineCx24,
                EmulationControllerVisualIds.Atari7800ControlPadEurope],
        PeripheralCategory.LightGun => [EmulationControllerVisualIds.AtariXg1LightGun],
        PeripheralCategory.EnhancedController when model == MachineModel.Lynx =>
            [EmulationControllerVisualIds.AtariLynx, EmulationControllerVisualIds.AtariLynxIi],
        PeripheralCategory.EnhancedController when model is MachineModel.Jaguar
            or MachineModel.JaguarCd =>
            [EmulationControllerVisualIds.AtariJaguarController,
                EmulationControllerVisualIds.AtariJaguarProController],
        _ => null
    };

    private static string? DefaultVisualId(
        MachineModel model,
        PeripheralCategory category) => category switch
    {
        PeripheralCategory.Joystick when model == MachineModel.Atari2600 =>
            EmulationControllerVisualIds.AtariCx40,
        PeripheralCategory.Joystick => EmulationControllerVisualIds.QuickShot,
        PeripheralCategory.ProLineController when model == MachineModel.Atari7800 =>
            EmulationControllerVisualIds.Atari7800ControlPadEurope,
        _ => CompatibleVisualIds(model, category)?.FirstOrDefault()
    };

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string>? VisualCommandIds(
        MachineModel model,
        PeripheralCategory category)
    {
        if (category is PeripheralCategory.None or PeripheralCategory.Automatic)
            return null;

        var actions = ControllerActions(model, category).ToHashSet(StringComparer.Ordinal);
        var result = new Dictionary<EmulationControllerVisualControl, string>();
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionUp,
            EmulationControllerCommandIds.Up);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionDown,
            EmulationControllerCommandIds.Down);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionLeft,
            EmulationControllerCommandIds.Left);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.DirectionRight,
            EmulationControllerCommandIds.Right);

        var jaguar = model is MachineModel.Jaguar or MachineModel.JaguarCd;
        AddVisualCommand(result, actions, EmulationControllerVisualControl.PrimaryAction,
            jaguar ? EmulationControllerCommandIds.A : EmulationControllerCommandIds.Fire1);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.SecondaryAction,
            jaguar ? EmulationControllerCommandIds.B : EmulationControllerCommandIds.Fire2);
        if (jaguar)
            AddVisualCommand(result, actions, EmulationControllerVisualControl.TertiaryAction,
                EmulationControllerCommandIds.C);

        AddVisualCommand(result, actions, EmulationControllerVisualControl.Turbo,
            EmulationControllerCommandIds.Turbo);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Start,
            EmulationControllerCommandIds.Start);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Pause,
            EmulationControllerCommandIds.Pause);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Reset,
            EmulationControllerCommandIds.Reset);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Option,
            EmulationControllerCommandIds.Option);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key0,
            EmulationControllerCommandIds.Key0);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key1,
            EmulationControllerCommandIds.Key1);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key2,
            EmulationControllerCommandIds.Key2);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key3,
            EmulationControllerCommandIds.Key3);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key4,
            EmulationControllerCommandIds.Key4);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key5,
            EmulationControllerCommandIds.Key5);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key6,
            EmulationControllerCommandIds.Key6);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key7,
            EmulationControllerCommandIds.Key7);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key8,
            EmulationControllerCommandIds.Key8);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.Key9,
            EmulationControllerCommandIds.Key9);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.KeyStar,
            EmulationControllerCommandIds.Star);
        AddVisualCommand(result, actions, EmulationControllerVisualControl.KeyHash,
            EmulationControllerCommandIds.Hash);
        return result;
    }

    private static void AddVisualCommand(
        IDictionary<EmulationControllerVisualControl, string> result,
        IReadOnlySet<string> actions,
        EmulationControllerVisualControl control,
        string commandId)
    {
        if (actions.Contains(commandId)) result[control] = commandId;
    }

    private static string ActionResourceKey(string action) => action switch
    {
        InputSettingsFunctionsConstants.Turbo => InputSettingsFunctionsConstants.ResourceControllerActionTurboFire,
        _ => $"Emulation.Controller.Action.{action}"
    };

    private static string? ActionInvariantValue(string action) => action switch
    {
        InputSettingsFunctionsConstants.Up or InputSettingsFunctionsConstants.Down or InputSettingsFunctionsConstants.Left or InputSettingsFunctionsConstants.Right or InputSettingsFunctionsConstants.Fire1 or InputSettingsFunctionsConstants.Fire2 or InputSettingsFunctionsConstants.Turbo => null,
        InputSettingsFunctionsConstants.Option1 => InputSettingsFunctionsConstants.Option12,
        InputSettingsFunctionsConstants.Option2 => InputSettingsFunctionsConstants.Option22,
        _ => action
    };

    private static InputBindingDefinition Definition(string id, string resourceKey, string defaultBinding,
        string? invariant = null) => new(id, resourceKey, defaultBinding, invariant);

    private static string KeyResource(EmulationKey key) => key switch
    {
        EmulationKey.Help => InputSettingsFunctionsConstants.ResourceKeyHelp,
        EmulationKey.Undo => InputSettingsFunctionsConstants.ResourceKeyUndo,
        EmulationKey.Break => InputSettingsFunctionsConstants.ResourceKeyBreak,
        _ => key.ToString()
    };

    private static string DefaultKey(EmulationKey key,
        IReadOnlyDictionary<EmulationKey, EmulationKey> defaults) =>
        defaults.TryGetValue(key, out var defaultKey)
            ? defaultKey.ToString()
            : key.ToString();

    private static IReadOnlyDictionary<string, string> ToStrings(
        IReadOnlyDictionary<string, EmulationKey>? values) => values?.ToDictionary(item => item.Key,
        item => item.Value.ToString(), StringComparer.Ordinal) ?? new Dictionary<string, string>();
}
