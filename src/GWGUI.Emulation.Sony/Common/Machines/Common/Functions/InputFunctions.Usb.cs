namespace GWGUI.Emulation.Sony.Common.Machines.Common.Functions;

internal static partial class InputSettingsFunctions
{
    internal static bool HasUsbKeyboard(MachineConfiguration configuration) => UsbTypes(configuration)
        .Any(type => type is ControllerType.Keyboard or ControllerType.KeyboardAndMouse or ControllerType.NetPlayController);

    internal static bool HasUsbMouse(MachineConfiguration configuration) => UsbTypes(configuration)
        .Any(type => type is ControllerType.Mouse or ControllerType.KeyboardAndMouse);

    private static IEnumerable<ControllerType> UsbTypes(MachineConfiguration configuration) =>
        configuration.Model == ModelConstants.PlayStation2
            ? Enumerable.Range(0, 2).Select(index => UsbBinding(configuration, index).Type)
                .Where(type => ControllerCatalog.IsSupported(configuration, type, 0, true)) : [];

    private static ControllerBinding UsbBinding(MachineConfiguration configuration, int index)
    {
        if (configuration.EmulatorId == "pcee2")
            return configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == index)
                ?? new ControllerBinding(index, ControllerFunctions.Selected(configuration, index));
        var bindings = configuration.Input?.UsbBindings;
        if (bindings is not null) return bindings.FirstOrDefault(binding => binding.Port == index) ?? new ControllerBinding(index, ControllerType.None);
        // Older LRPS2 configurations stored USB modes in controller slots.
        var legacy = configuration.Input?.ControllerBindings?.FirstOrDefault(binding => binding.Port == index);
        var combined = configuration.Input?.ControllerBindings?.Any(binding => binding.Type == ControllerType.KeyboardAndMouse) == true
            || configuration.Controllers?.Contains(ControllerType.KeyboardAndMouse) == true;
        var type = combined ? index == 0 ? ControllerType.Keyboard : ControllerType.Mouse
            : legacy?.Type ?? configuration.Controllers?.ElementAtOrDefault(index) ?? ControllerType.None;
        return new ControllerBinding(index, type is ControllerType.Keyboard or ControllerType.Mouse ? type : ControllerType.None);
    }

    internal static ControllerType NativeUsbType(MachineConfiguration configuration, int port, ControllerType controller)
    {
        if (configuration.EmulatorId != "pcsx2" || configuration.Input?.UsbBindings is null || port >= 2) return controller;
        var usb = UsbBinding(configuration, port).Type;
        return usb switch
        {
            ControllerType.None => controller,
            ControllerType.Keyboard or ControllerType.Mouse => usb,
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
                ? ControllerFunctions.Selected(configuration, index) : ControllerFunctions.Resolve(configuration, index);
        if (!usb && configuration.Model == ModelConstants.PlayStation2 && configuration.EmulatorId == "pcsx2"
            && type is ControllerType.Keyboard or ControllerType.Mouse or ControllerType.KeyboardAndMouse)
            type = ControllerType.Joystick;
        var available = ControllerCatalog.Available(configuration, index, usb);
        if (!available.Contains(type))
        {
            type = available.Contains(ControllerType.None) ? ControllerType.None : available.FirstOrDefault();
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

    private static IReadOnlyList<InputBindingDefinition> PortDefinitions(ControllerType type, Model model, MachineConfiguration configuration, int port)
    {
        var usb = ControllerCatalog.IsUsbPort(configuration, port);
        if (InstrumentDefinitions(type) is { } instrumentDefinitions) return instrumentDefinitions;
        if (type == ControllerType.KidsStation) return KidsStationDefinitions();
        if (type == ControllerType.MaestroBaton) return MaestroDefinitions();
        if (type == ControllerType.Tambourine) return TambourineDefinitions();
        if (type == ControllerType.UsbFlightStick) return FlightStick2Definitions();
        if (type == ControllerType.Seamic) return SeamicDefinitions();
        if (type is ControllerType.Katana or ControllerType.Chainsaw) return SpecialControllerDefinitions(type);
        if (type == ControllerType.DvdRemote) return DvdRemoteDefinitions();
        if (type == ControllerType.FishingController) return FishingDefinitions();
        if (type == ControllerType.TrainController) return TrainTwoHandleDefinitions();
        if (type == ControllerType.UsbTrainController) return UsbTrainDefinitions();
        if (type == ControllerType.JetController) return JetDefinitions();
        if (type == ControllerType.SerialWheel) return MadCatzWheelDefinitions();
        if (type == ControllerType.UsbWheel) return GtForceDefinitions();
        if (!usb && type == ControllerType.NetPlayController) return NetPlayDefinitions();
        if (type == ControllerType.Grip) return GripDefinitions();
        if (type == ControllerType.BiohazardPad) return BiohazardDefinitions();
        if (type == ControllerType.SphereController) return SphereDefinitions();
        if (type == ControllerType.Glove) return GloveDefinitions();
        if (type == ControllerType.BuzzBuzzers) return BuzzDefinitions();
        if (type == ControllerType.JogCon) return JogConDefinitions();
        if (type == ControllerType.VolumeController) return VolumeDefinitions();
        if (type == ControllerType.Guitar) return model.Id == ModelConstants.PlayStation ? GuitarFreaksDefinitions() : GuitarDefinitions();
        if (type == ControllerType.Beatmania) return BeatmaniaDefinitions(5);
        if (type == ControllerType.BeatmaniaIidx) return BeatmaniaDefinitions(7);
        if (type == ControllerType.PopnMusic) return PopnDefinitions();
        if (type == ControllerType.TaikoDrum) return TaikoDefinitions();
        if (type == ControllerType.ArcadeStick) return PlayStationButtonDefinitions()
            .Where(definition => definition.Id is not (EmulationControllerCommandIds.L3 or EmulationControllerCommandIds.R3)).ToArray();
        if (!usb && type is ControllerType.Mouse or ControllerType.SerialMouse) return MouseDefinitions();
        if (type == ControllerType.GunCon2) return ControllerDefinitions(type, model, configuration, port);
        if (!usb && type is ControllerType.Joystick or ControllerType.DualShock or ControllerType.AnalogController
            or ControllerType.AnalogJoystick or ControllerType.NeGcon or ControllerType.NeGconRumble
            or ControllerType.GunCon or ControllerType.Justifier or ControllerType.DanceMat)
            return ControllerDefinitions(type, model, configuration, port);
        if (!ControllerCatalog.IsSupported(configuration, type, port, usb)
            || usb && type is ControllerType.Keyboard or ControllerType.Mouse or ControllerType.NetPlayController) return [];
        return ControllerDefinitions(type, model, configuration, port);
    }

    private static IReadOnlyList<InputBindingDefinition> UsbKeyboardDefinitions(MachineConfiguration configuration) =>
        (new Engine().Adapter(configuration) as IEmulatorControllerAdapter)?.GetControllerBindingDefinitions(configuration, ControllerType.Keyboard, 0) ?? [];

    private static IReadOnlyList<InputBindingDefinition> NetPlayDefinitions() =>
    [
        .. PlayStationButtonDefinitions(),
        .. AnalogStickDefinitions(),
        new(InputSettingsFunctionsConstants.Analog, string.Empty, string.Empty, "MODE"),
        new("NetPlay.Vibration", string.Empty, string.Empty, "VIBRATION")
    ];

    private static string UsbKeyboardVisualId(MachineConfiguration configuration) => Enumerable.Range(0, 2)
        .Select(index => UsbBinding(configuration, index)).FirstOrDefault(binding => binding.Type is ControllerType.Keyboard or ControllerType.KeyboardAndMouse or ControllerType.NetPlayController)
        ?.Type == ControllerType.NetPlayController ? EmulationControllerVisualIds.SonyNetPlay : EmulationControllerVisualIds.SonyUsbKeyboard;

    private static IReadOnlyDictionary<EmulationControllerVisualControl, string> UsbMouseVisualCommands() => new Dictionary<EmulationControllerVisualControl, string>
    {
        [EmulationControllerVisualControl.MouseLeftButton] = nameof(MouseAction.LeftButton),
        [EmulationControllerVisualControl.MouseRightButton] = nameof(MouseAction.RightButton),
        [EmulationControllerVisualControl.PrimaryAction] = nameof(MouseAction.MiddleButton)
    };
}
