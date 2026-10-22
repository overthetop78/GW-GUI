using CommonControllerFunctions = GWGUI.Emulation.Sony.Common.Functions.ControllerFunctions;
namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static bool HasUsbKeyboard(MachineConfiguration configuration) => UsbTypes(configuration)
        .Any(type => type is PeripheralCategory.Keyboard or PeripheralCategory.KeyboardAndMouse or PeripheralCategory.NetPlayController);

    internal static bool HasUsbMouse(MachineConfiguration configuration) => UsbTypes(configuration)
        .Any(type => type is PeripheralCategory.Mouse or PeripheralCategory.KeyboardAndMouse);

    private static IEnumerable<PeripheralCategory> UsbTypes(MachineConfiguration configuration) =>
        configuration.Model == ModelConstants.PlayStation2
            ? Enumerable.Range(0, 2).Select(index => UsbBinding(configuration, index).Type)
                .Where(type => ControllerCatalog.IsSupported(configuration, type, 0, true)) : [];

    private static ControllerBinding UsbBinding(MachineConfiguration configuration, int index)
    {
        if (configuration.EmulatorId == "pcee2")
            return configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == index)
                ?? new ControllerBinding(index, CommonControllerFunctions.Selected(configuration, index));
        var bindings = configuration.Input?.UsbBindings;
        if (bindings is not null) return bindings.FirstOrDefault(binding => binding.Port == index) ?? new ControllerBinding(index, PeripheralCategory.None);
        // Older LRPS2 configurations stored USB modes in controller slots.
        var legacy = configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == index);
        var combined = configuration.Input?.ControllerBindings?.Any(binding => binding.Type == PeripheralCategory.KeyboardAndMouse) == true
            || configuration.Controllers?.Contains(PeripheralCategory.KeyboardAndMouse) == true;
        var type = combined ? index == 0 ? PeripheralCategory.Keyboard : PeripheralCategory.Mouse
            : legacy?.Type ?? configuration.Controllers?.ElementAtOrDefault(index) ?? PeripheralCategory.None;
        return new ControllerBinding(index, type is PeripheralCategory.Keyboard or PeripheralCategory.Mouse ? type : PeripheralCategory.None);
    }

    internal static PeripheralCategory NativeUsbType(MachineConfiguration configuration, int port, PeripheralCategory controller)
    {
        if (configuration.EmulatorId != "pcsx2" || configuration.Input?.UsbBindings is null || port >= 2) return controller;
        var usb = UsbBinding(configuration, port).Type;
        return usb switch
        {
            PeripheralCategory.None => controller,
            PeripheralCategory.Keyboard or PeripheralCategory.Mouse => usb,
            _ => throw new NotSupportedException(configuration.EmulatorId + ": " + usb)
        };
    }

    private static EmulationControllerPort DescribePort(MachineConfiguration configuration, Model model, int index)
    {
        var usb = ControllerCatalog.IsUsbPort(configuration, index);
        var synthetic = usb && configuration.EmulatorId != "pcee2";
        var current = synthetic ? UsbBinding(configuration, index - 8)
            : configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == index);
        var type = synthetic ? current!.Type
            : configuration.Model is ModelConstants.PlayStation or ModelConstants.PlayStation2
                ? CommonControllerFunctions.Selected(configuration, index) : CommonControllerFunctions.Resolve(configuration, index);
        if (!usb && configuration.Model == ModelConstants.PlayStation2 && configuration.EmulatorId == "pcsx2"
            && type is PeripheralCategory.Keyboard or PeripheralCategory.Mouse or PeripheralCategory.KeyboardAndMouse)
            type = PeripheralCategory.Joystick;
        var available = ControllerCatalog.Available(configuration, index, usb);
        if (!available.Contains(type))
        {
            type = available.Contains(PeripheralCategory.None) ? PeripheralCategory.None : available.FirstOrDefault();
            current = null;
        }
        var adapter = new Engine().Adapter(configuration) as IEmulatorControllerAdapter;
        var definitions = current?.VisualId is { } visualId
            && PeripheralVisualDefinitions(type)?.TryGetValue(visualId, out var visualDefinitions) == true
                ? visualDefinitions : PortDefinitions(type, model, configuration, index);
        return new EmulationControllerPort(index + 1,
            available.Select(choice => Choice(choice, model, configuration, index)).ToArray(),
            type.ToString(), current?.DeviceId,
            new EmulationInputBindingSet(definitions,
                current?.ButtonMappings ?? new Dictionary<string, string>(),
                EmulationInputSource.Keyboard | EmulationInputSource.Mouse | EmulationInputSource.Controller, true),
            VisualId: current?.VisualId)
        {
            IsUsb = usb,
            DisplayName = usb ? "USB " + (synthetic ? index - 7 : index + 1) : adapter?.GetControllerPortDisplayName(configuration, index),
            DisplayNumber = usb ? null : adapter?.GetControllerPortDisplayNumber(configuration, index),
            OutputBindings = ValidOutputBindings(configuration, type, current?.VisualId, current?.OutputBindings)
        };
    }

    private static IReadOnlyList<InputBindingDefinition> PortDefinitions(PeripheralCategory type, Model model, MachineConfiguration configuration, int port)
    {
        var usb = ControllerCatalog.IsUsbPort(configuration, port);
        if (InstrumentDefinitions(type) is { } instrumentDefinitions) return instrumentDefinitions;
        if (type == PeripheralCategory.KidsStation) return KidsStationDefinitions();
        if (type == PeripheralCategory.MaestroBaton) return MaestroDefinitions();
        if (type == PeripheralCategory.Tambourine) return TambourineDefinitions();
        if (type == PeripheralCategory.UsbFlightStick) return FlightStick2Definitions();
        if (type == PeripheralCategory.Seamic) return SeamicDefinitions();
        if (type is PeripheralCategory.Katana or PeripheralCategory.Chainsaw) return SpecialControllerDefinitions(type);
        if (type == PeripheralCategory.DvdRemote) return DvdRemoteDefinitions();
        if (type == PeripheralCategory.FishingController) return FishingDefinitions();
        if (type == PeripheralCategory.TrainController) return TrainTwoHandleDefinitions();
        if (type == PeripheralCategory.UsbTrainController) return UsbTrainDefinitions();
        if (type == PeripheralCategory.JetController) return JetDefinitions();
        if (type == PeripheralCategory.SerialWheel) return MadCatzWheelDefinitions();
        if (type == PeripheralCategory.UsbWheel) return GtForceDefinitions();
        if (!usb && type == PeripheralCategory.NetPlayController) return NetPlayDefinitions();
        if (type == PeripheralCategory.Grip) return GripDefinitions();
        if (type == PeripheralCategory.BiohazardPad) return BiohazardDefinitions();
        if (type == PeripheralCategory.SphereController) return SphereDefinitions();
        if (type == PeripheralCategory.Glove) return GloveDefinitions();
        if (type == PeripheralCategory.BuzzBuzzers) return BuzzDefinitions();
        if (type == PeripheralCategory.JogCon) return JogConDefinitions();
        if (type == PeripheralCategory.VolumeController) return VolumeDefinitions();
        if (type == PeripheralCategory.Guitar) return model.Id == ModelConstants.PlayStation ? GuitarFreaksDefinitions() : GuitarDefinitions();
        if (type == PeripheralCategory.Beatmania) return BeatmaniaDefinitions(5);
        if (type == PeripheralCategory.BeatmaniaIidx) return BeatmaniaDefinitions(7);
        if (type == PeripheralCategory.PopnMusic) return PopnDefinitions();
        if (type == PeripheralCategory.TaikoDrum) return TaikoDefinitions();
        if (type == PeripheralCategory.ArcadeStick) return PlayStationButtonDefinitions()
            .Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray();
        if (!usb && type is PeripheralCategory.Mouse or PeripheralCategory.SerialMouse) return MouseDefinitions();
        if (type == PeripheralCategory.GunCon2) return ControllerDefinitions(type, model, configuration, port);
        if (!usb && type is PeripheralCategory.Joystick or PeripheralCategory.DualShock or PeripheralCategory.AnalogController
            or PeripheralCategory.AnalogJoystick or PeripheralCategory.NeGcon or PeripheralCategory.NeGconRumble
            or PeripheralCategory.GunCon or PeripheralCategory.Justifier or PeripheralCategory.DanceMat)
            return ControllerDefinitions(type, model, configuration, port);
        if (!ControllerCatalog.IsSupported(configuration, type, port, usb)
            || usb && type is PeripheralCategory.Keyboard or PeripheralCategory.Mouse or PeripheralCategory.NetPlayController) return [];
        return ControllerDefinitions(type, model, configuration, port);
    }

    private static IReadOnlyList<InputBindingDefinition> UsbKeyboardDefinitions(MachineConfiguration configuration) =>
        (new Engine().Adapter(configuration) as IEmulatorControllerAdapter)?.GetControllerBindingDefinitions(configuration, PeripheralCategory.Keyboard, 0) ?? [];

    private static IReadOnlyList<InputBindingDefinition> NetPlayDefinitions() =>
    [
        .. PlayStationButtonDefinitions(),
        .. AnalogStickDefinitions(),
        new(InputSettingsFunctionsConstants.Analog, string.Empty, string.Empty, "MODE"),
        new("NetPlay.Vibration", string.Empty, string.Empty, "VIBRATION")
    ];

    private static string UsbKeyboardVisualId(MachineConfiguration configuration) => Enumerable.Range(0, 2)
        .Select(index => UsbBinding(configuration, index)).FirstOrDefault(binding => binding.Type is PeripheralCategory.Keyboard or PeripheralCategory.KeyboardAndMouse or PeripheralCategory.NetPlayController)
        ?.Type == PeripheralCategory.NetPlayController ? EmulationControllerVisualIds.SonyNetPlay : EmulationControllerVisualIds.SonyUsbKeyboard;

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> UsbMouseVisualCommands() => new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.MouseLeftButton] = nameof(MouseAction.LeftButton),
        [EmulationControllerVisualControl.MouseRightButton] = nameof(MouseAction.RightButton),
        [EmulationControllerVisualControl.PrimaryAction] = nameof(MouseAction.MiddleButton)
    };
}
