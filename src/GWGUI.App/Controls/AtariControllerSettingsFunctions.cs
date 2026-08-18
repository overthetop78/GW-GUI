using GWGUI.App.Input;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariControllerSettingsFunctions
{
    internal static IReadOnlyList<InputBindingDefinition> Definitions(AtariMachineModel model,
        AtariPeripheralKind selectedPeripheral, int port)
    {
        var peripheral = selectedPeripheral == AtariPeripheralKind.Automatic
            ? DefaultPeripheral(model) : selectedPeripheral;
        return Actions(model, peripheral).Select(action => new InputBindingDefinition(action, ActionLabel(action),
            AtariControllerSettingsConstants.DefaultSources.TryGetValue(action, out var source)
                ? InputBindingSyntax.Controller(port, source) : string.Empty)).ToArray();
    }

    internal static IReadOnlyList<AtariPeripheralKind> Peripherals(AtariMachineModel model)
    {
        if (model == AtariMachineModel.Atari2600)
            return [AtariPeripheralKind.Joystick,
                AtariPeripheralKind.Paddle, AtariPeripheralKind.DrivingController,
                AtariPeripheralKind.BoosterGrip, AtariPeripheralKind.GenesisController,
                AtariPeripheralKind.Joy2BPlus, AtariPeripheralKind.None];
        if (AtariCompatibilityCatalog.Get(model).Core == AtariCoreKind.Hatari)
            return [AtariPeripheralKind.Joystick, AtariPeripheralKind.None];
        return AtariClassicModelCatalog.Get(model).Ports
            .Where(port => port.Capability != AtariClassicPortCapability.Keyboard)
            .Select(port => port.Capability switch
            {
                AtariClassicPortCapability.Joystick => AtariPeripheralKind.Joystick,
                AtariClassicPortCapability.AnalogJoystick => AtariPeripheralKind.AnalogJoystick,
                AtariClassicPortCapability.Paddle => AtariPeripheralKind.Paddle,
                AtariClassicPortCapability.DrivingController => AtariPeripheralKind.DrivingController,
                AtariClassicPortCapability.NumericKeypad when model == AtariMachineModel.Atari5200
                    => AtariPeripheralKind.AnalogJoystick,
                AtariClassicPortCapability.NumericKeypad => AtariPeripheralKind.NumericKeypad,
                AtariClassicPortCapability.LightGun => AtariPeripheralKind.LightGun,
                AtariClassicPortCapability.ProLineController => AtariPeripheralKind.ProLineController,
                AtariClassicPortCapability.EnhancedController => AtariPeripheralKind.EnhancedController,
                _ => AtariPeripheralKind.None
            }).Append(AtariPeripheralKind.None).Distinct().ToArray();
    }

    internal static string PeripheralLabel(AtariMachineModel model, AtariPeripheralKind peripheral) => peripheral switch
    {
        AtariPeripheralKind.None => LocExtension.Get("Emulation.Controller.None"),
        AtariPeripheralKind.Automatic => LocExtension.Get("Emulation.Controller.Automatic"),
        AtariPeripheralKind.Joystick => LocExtension.Get("Emulation.Atari.Controller.Joystick"),
        AtariPeripheralKind.AnalogJoystick when model == AtariMachineModel.Atari5200
            => LocExtension.Get("Emulation.Atari.Controller.Atari5200"),
        AtariPeripheralKind.AnalogJoystick => LocExtension.Get("Emulation.Controller.AnalogJoystick"),
        AtariPeripheralKind.Paddle => LocExtension.Get("Emulation.Atari.Controller.PaddleControllers"),
        AtariPeripheralKind.LightGun => LocExtension.Get("Emulation.Atari.Controller.Xg1LightGun"),
        AtariPeripheralKind.NumericKeypad when model == AtariMachineModel.Atari5200
            => LocExtension.Get("Emulation.Atari.Controller.Atari5200"),
        AtariPeripheralKind.NumericKeypad => LocExtension.Get("Emulation.Atari.Controller.NumericKeypad"),
        AtariPeripheralKind.DrivingController => LocExtension.Get("Emulation.Atari.Controller.Driving"),
        AtariPeripheralKind.ProLineController => LocExtension.Get("Emulation.Atari.Controller.ProLine"),
        AtariPeripheralKind.BoosterGrip => LocExtension.Get("Emulation.Atari.Controller.BoosterGrip"),
        AtariPeripheralKind.GenesisController => LocExtension.Get("Emulation.Atari.Controller.Genesis"),
        AtariPeripheralKind.Joy2BPlus => LocExtension.Get("Emulation.Atari.Controller.Joy2BPlus"),
        AtariPeripheralKind.EnhancedController when model == AtariMachineModel.Lynx
            => LocExtension.Get("Emulation.Atari.Controller.Lynx"),
        AtariPeripheralKind.EnhancedController when model is AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd
            => LocExtension.Get("Emulation.Atari.Controller.Jaguar"),
        _ => peripheral.ToString()
    };

    private static IReadOnlyList<string> Actions(AtariMachineModel model, AtariPeripheralKind peripheral)
    {
        if (peripheral == AtariPeripheralKind.None) return [];
        if (model == AtariMachineModel.Atari5200 && peripheral is AtariPeripheralKind.AnalogJoystick
            or AtariPeripheralKind.NumericKeypad)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.DualFireActions)
                .Concat(AtariControllerSettingsConstants.KeypadActions).Distinct(StringComparer.Ordinal).ToArray();
        if (peripheral == AtariPeripheralKind.NumericKeypad) return AtariControllerSettingsConstants.KeypadActions;
        if (peripheral is AtariPeripheralKind.Paddle or AtariPeripheralKind.DrivingController
            or AtariPeripheralKind.LightGun) return AtariControllerSettingsConstants.SingleFireActions;
        if (peripheral is AtariPeripheralKind.BoosterGrip)
            return AtariControllerSettingsConstants.DirectionActions.Concat(["Fire1", "Fire2", "Turbo"]).ToArray();
        if (peripheral is AtariPeripheralKind.Joy2BPlus)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.DualFireActions).ToArray();
        if (peripheral is AtariPeripheralKind.GenesisController)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.SingleFireActions).ToArray();
        if (model is AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.JaguarActions).ToArray();
        if (model == AtariMachineModel.Lynx)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.LynxActions).ToArray();
        if (model is AtariMachineModel.Atari5200 or AtariMachineModel.Atari7800)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.DualFireActions).ToArray();
        if (AtariCompatibilityCatalog.Get(model).Core == AtariCoreKind.Hatari)
            return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.HatariFireActions).ToArray();
        return AtariControllerSettingsConstants.DirectionActions.Concat(AtariControllerSettingsConstants.SingleFireActions).ToArray();
    }

    internal static AtariPeripheralKind DefaultPeripheral(AtariMachineModel model) => model switch
    {
        AtariMachineModel.Atari5200 => AtariPeripheralKind.AnalogJoystick,
        AtariMachineModel.Atari7800 => AtariPeripheralKind.ProLineController,
        AtariMachineModel.Lynx or AtariMachineModel.Jaguar or AtariMachineModel.JaguarCd
            => AtariPeripheralKind.EnhancedController,
        _ => AtariPeripheralKind.Joystick
    };

    private static string ActionLabel(string action) => action switch
    {
        "Up" => LocExtension.Get("Emulation.Controller.Action.Up"), "Down" => LocExtension.Get("Emulation.Controller.Action.Down"),
        "Left" => LocExtension.Get("Emulation.Controller.Action.Left"), "Right" => LocExtension.Get("Emulation.Controller.Action.Right"),
        "Fire1" => LocExtension.Get("Emulation.Controller.Action.Fire1"), "Fire2" => LocExtension.Get("Emulation.Controller.Action.Fire2"),
        "Turbo" => LocExtension.Get("Emulation.Controller.Action.TurboFire"),
        "Option1" => "Option 1", "Option2" => "Option 2", _ => action
    };
}
