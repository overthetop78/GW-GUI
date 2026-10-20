namespace GWGUI.Emulation.Functions;

public static class EmulationInputMappingFunctions
{
    internal static short NativeMouseState(EmulationPointerState pointer, uint port, uint index, uint id)
    {
        if (port != ExternalCoreApiConstants.PrimaryPort || index != ExternalCoreApiConstants.LeftAnalogIndex)
            return ExternalCoreApiConstants.InactiveState;
        return id switch
        {
            ExternalCoreApiConstants.MouseXId => Clamp(pointer.DeltaX),
            ExternalCoreApiConstants.MouseYId => Clamp(pointer.DeltaY),
            ExternalCoreApiConstants.MouseLeftId => Boolean(pointer.Left),
            ExternalCoreApiConstants.MouseRightId => Boolean(pointer.Right),
            ExternalCoreApiConstants.MouseWheelUpId => Boolean(pointer.Wheel > ExternalCoreApiConstants.ConsumedRelativeValue),
            ExternalCoreApiConstants.MouseWheelDownId => Boolean(pointer.Wheel < ExternalCoreApiConstants.ConsumedRelativeValue),
            ExternalCoreApiConstants.MouseMiddleId => Boolean(pointer.Middle),
            _ => ExternalCoreApiConstants.InactiveState
        };
    }

    internal static short NativeJoypadState(uint buttons, uint id, uint maximumButtonCount)
    {
        if (id == ExternalCoreApiConstants.JoypadMaskId)
            return unchecked((short)(buttons & ushort.MaxValue));
        return id < maximumButtonCount && (buttons & (1u << checked((int)id))) != 0
            ? ExternalCoreApiConstants.ActiveState : ExternalCoreApiConstants.InactiveState;
    }

    internal static short NativeAnalogState(EmulationControllerState controller, uint index, uint id) =>
        (index, id) switch
        {
            (ExternalCoreApiConstants.LeftAnalogIndex, ExternalCoreApiConstants.AnalogXId) => controller.LeftX,
            (ExternalCoreApiConstants.LeftAnalogIndex, ExternalCoreApiConstants.AnalogYId) => controller.LeftY,
            (ExternalCoreApiConstants.RightAnalogIndex, ExternalCoreApiConstants.AnalogXId) => controller.RightX,
            (ExternalCoreApiConstants.RightAnalogIndex, ExternalCoreApiConstants.AnalogYId) => controller.RightY,
            _ => ExternalCoreApiConstants.InactiveState
        };

    private static short Boolean(bool value) => value ? ExternalCoreApiConstants.ActiveState : ExternalCoreApiConstants.InactiveState;
    private static short Clamp(int value) => (short)Math.Clamp(value, short.MinValue, short.MaxValue);

    internal static string? BindingDisplayValue(string resourceKey, string? invariant = null) =>
        invariant ?? (resourceKey.Contains('.') ? null : resourceKey);

    internal static EmulationInputSnapshot Freeze(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        return new EmulationInputSnapshot(new HashSet<EmulationKey>(snapshot.Keys), snapshot.Pointer with { }, snapshot.Controllers.ToArray());
    }

    internal static int SaturatingAdd(int left, int right) => (int)Math.Clamp((long)left + right, int.MinValue, int.MaxValue);

    private static readonly IReadOnlyDictionary<string, int> ControllerButtons =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            [EmulationInputMappingConstants.B] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.ButtonB] = EmulationInputMappingConstants.BIndex,
            [EmulationInputMappingConstants.Y] = EmulationInputMappingConstants.YIndex,
            [EmulationInputMappingConstants.ButtonY] = EmulationInputMappingConstants.YIndex,
            [EmulationInputMappingConstants.Select] = EmulationInputMappingConstants.SelectIndex,
            [EmulationInputMappingConstants.View] = EmulationInputMappingConstants.SelectIndex,
            [EmulationInputMappingConstants.Start] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Menu] = EmulationInputMappingConstants.StartIndex,
            [EmulationInputMappingConstants.Up] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.DPadUp] = EmulationInputMappingConstants.UpIndex,
            [EmulationInputMappingConstants.Down] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.DPadDown] = EmulationInputMappingConstants.DownIndex,
            [EmulationInputMappingConstants.Left] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.DPadLeft] = EmulationInputMappingConstants.LeftIndex,
            [EmulationInputMappingConstants.Right] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.DPadRight] = EmulationInputMappingConstants.RightIndex,
            [EmulationInputMappingConstants.A] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.ButtonA] = EmulationInputMappingConstants.AIndex,
            [EmulationInputMappingConstants.X] = EmulationInputMappingConstants.XIndex,
            [EmulationInputMappingConstants.ButtonX] = EmulationInputMappingConstants.XIndex,
            [EmulationInputMappingConstants.L] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.LeftShoulder] = EmulationInputMappingConstants.LIndex,
            [EmulationInputMappingConstants.R] = EmulationInputMappingConstants.RIndex,
            [EmulationInputMappingConstants.RightShoulder] = EmulationInputMappingConstants.RIndex,
            [EmulationInputMappingConstants.L2] = EmulationInputMappingConstants.L2Index,
            [EmulationInputMappingConstants.LeftTrigger] = EmulationInputMappingConstants.L2Index,
            [EmulationInputMappingConstants.R2] = EmulationInputMappingConstants.R2Index,
            [EmulationInputMappingConstants.RightTrigger] = EmulationInputMappingConstants.R2Index,
            [EmulationInputMappingConstants.L3] = EmulationInputMappingConstants.L3Index,
            [EmulationInputMappingConstants.LeftStickClick] = EmulationInputMappingConstants.L3Index,
            [EmulationInputMappingConstants.R3] = EmulationInputMappingConstants.R3Index,
            [EmulationInputMappingConstants.RightStickClick] = EmulationInputMappingConstants.R3Index,
            [EmulationInputMappingConstants.XboxButton] = EmulationInputMappingConstants.GuideIndex,
            [EmulationInputMappingConstants.Guide] = EmulationInputMappingConstants.GuideIndex,
            [EmulationInputMappingConstants.Share] = EmulationInputMappingConstants.ShareIndex,
            [EmulationInputMappingConstants.PaddleLeft1] = EmulationInputMappingConstants.PaddleLeft1Index,
            [EmulationInputMappingConstants.PaddleLeft2] = EmulationInputMappingConstants.PaddleLeft2Index,
            [EmulationInputMappingConstants.PaddleRight1] = EmulationInputMappingConstants.PaddleRight1Index,
            [EmulationInputMappingConstants.PaddleRight2] = EmulationInputMappingConstants.PaddleRight2Index
        };

    internal static Dictionary<string, string> MouseBindings<TAction>(IReadOnlyDictionary<string, TAction>? mappings,
        TAction none) where TAction : struct, Enum =>
        (mappings ?? new Dictionary<string, TAction>()).Where(item => !EqualityComparer<TAction>.Default.Equals(item.Value, none))
            .ToDictionary(item => item.Value.ToString(), item => item.Key, StringComparer.Ordinal);

    internal static Dictionary<string, TAction>? CreateMouseMappings<TAction>(EmulationInputBindingSet? bindings,
        TAction none) where TAction : struct, Enum =>
        bindings?.Values.Where(item => !string.IsNullOrWhiteSpace(item.Value)
                && Enum.TryParse<TAction>(item.Key, true, out var action) && !EqualityComparer<TAction>.Default.Equals(action, none))
            .ToDictionary(item => item.Value, item => Enum.Parse<TAction>(item.Key, true), StringComparer.OrdinalIgnoreCase);

    internal static Dictionary<string, string> CopyNonEmptyBindings(IReadOnlyDictionary<string, string> values) =>
        values.Where(item => !string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);

    internal static bool HasInvalidBindings(IReadOnlyDictionary<string, string>? bindings,
        IReadOnlySet<string> commandIds, Func<string, bool> isValidSource) =>
        bindings?.Any(pair => !commandIds.Contains(pair.Key) || !isValidSource(pair.Value)) == true;

    internal static bool HasInvalidControllerPortNumbers(EmulationInputSettings settings, int count) =>
        settings.ControllerPorts.Count != count
        || !settings.ControllerPorts.Select(port => port.Number).Order().SequenceEqual(Enumerable.Range(1, count));

    internal static Dictionary<string, string> ResolveKeyboardBindings(IReadOnlyList<InputBindingDefinition> definitions,
        IReadOnlyDictionary<string, string>? input)
    {
        var bindings = definitions.ToDictionary(definition => definition.Id, definition => definition.DefaultBinding, StringComparer.Ordinal);
        foreach (var pair in input ?? new Dictionary<string, string>())
        {
            if (!bindings.ContainsKey(pair.Key) || !EmulationKeyboardBindingFunctions.IsValid(pair.Value))
                throw new ArgumentException(nameof(input));
            bindings[pair.Key] = pair.Value;
        }

        return bindings;
    }

    internal static Dictionary<string, string> ResolveBindings(IReadOnlyList<InputBindingDefinition> definitions,
        IReadOnlyDictionary<string, string>? values) =>
        definitions.ToDictionary(definition => definition.Id,
            definition => values?.GetValueOrDefault(definition.Id, definition.DefaultBinding) ?? definition.DefaultBinding,
            StringComparer.Ordinal);

    public static TBinding[] CreateControllerBindings<TPeripheral, TBinding>(EmulationInputSettings settings,
        Func<int, TPeripheral, bool> supportsPeripheral,
        Func<EmulationControllerPort, TPeripheral, IReadOnlyDictionary<string, string>, TBinding> createBinding)
        where TPeripheral : struct, Enum =>
        settings.ControllerPorts.Select(port =>
        {
            if (!Enum.TryParse<TPeripheral>(port.SelectedControllerId, out var peripheral) || !supportsPeripheral(port.Number - 1, peripheral))
                throw new ArgumentException(nameof(settings));
            return createBinding(port, peripheral, new Dictionary<string, string>(port.Bindings.Values, StringComparer.Ordinal));
        }).ToArray();

    public static IReadOnlySet<EmulationKey> MapKeyboard(IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new HashSet<EmulationKey>(keys);
        var result = new HashSet<EmulationKey>();
        foreach (var key in keys)
        {
            var mapped = mappings.FirstOrDefault(item => item.Value == key);
            result.Add(!string.IsNullOrWhiteSpace(mapped.Key) && Enum.TryParse<EmulationKey>(mapped.Key, true, out var target) ? target : key);
        }
        return result;
    }

    public static IReadOnlySet<EmulationKey> MapKeyboard(EmulationInputSnapshot snapshot, IReadOnlyDictionary<string, string>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new HashSet<EmulationKey>(snapshot.Keys);
        var simple = new Dictionary<string, EmulationKey>(StringComparer.Ordinal);
        foreach (var mapping in mappings)
        {
            var source = mapping.Value.StartsWith(EmulationInputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase)
                ? mapping.Value[EmulationInputMappingConstants.KeyboardPrefix.Length..] : mapping.Value;
            if (Enum.TryParse<EmulationKey>(source, true, out var key)) simple[mapping.Key] = key;
        }
        var result = new HashSet<EmulationKey>(MapKeyboard(snapshot.Keys, simple));
        var controller = snapshot.Controllers.Count > 0 ? snapshot.Controllers[0] : EmulationControllerState.Empty;
        foreach (var mapping in mappings)
        {
            if (simple.ContainsKey(mapping.Key) || string.IsNullOrWhiteSpace(mapping.Value)) continue;
            if (Enum.TryParse<EmulationKey>(mapping.Key, true, out var target) && InputSourcePressure(mapping.Value, controller, snapshot) > .5f) result.Add(target);
        }
        return result;
    }

    public static string? ParseControllerDeviceId(string? source)
    {
        if (source?.Contains('+') == true && !source.EndsWith('+'))
            source = source.Split('+', StringSplitOptions.TrimEntries).FirstOrDefault(part => part.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(source) ||
            !source.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)) return null;
        var controlSeparator = source.LastIndexOf(EmulationInputMappingConstants.SourceSeparator);
        return controlSeparator <= EmulationInputMappingConstants.ControllerPrefix.Length ? null : source[EmulationInputMappingConstants.ControllerPrefix.Length..controlSeparator];
    }

    public static EmulationControllerState ResolveController(string? deviceId, IReadOnlyList<EmulationControllerState> controllers, int fallbackIndex)
    {
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            var match = controllers.FirstOrDefault(controller => string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase));
            return match ?? EmulationControllerState.Empty;
        }
        return fallbackIndex >= 0 && fallbackIndex < controllers.Count ? controllers[fallbackIndex] : EmulationControllerState.Empty;
    }

    public static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
        => ResolveSourceController(source, controllers, fallback, fallbackWhenMissing: false);

    public static EmulationControllerState ResolveSourceController(string source, IReadOnlyList<EmulationControllerState> controllers,
        EmulationControllerState fallback, bool fallbackWhenMissing)
    {
        var deviceId = ParseControllerDeviceId(source);
        if (deviceId is null) return fallback;
        if (!fallbackWhenMissing) return ResolveController(deviceId, controllers, -1);
        return controllers.FirstOrDefault(controller => string.Equals(controller.DeviceId, deviceId, StringComparison.OrdinalIgnoreCase))
            ?? fallback;
    }

    internal static short ApplyDeadZone(short value, int percent)
    {
        var threshold = short.MaxValue * percent / 100;
        return Math.Abs((int)value) <= threshold ? (short)0 : value;
    }

    internal static EmulationControllerState ApplyDeadZones(EmulationControllerState controller, int percent) =>
        controller with
        {
            LeftX = ApplyDeadZone(controller.LeftX, percent),
            LeftY = ApplyDeadZone(controller.LeftY, percent),
            RightX = ApplyDeadZone(controller.RightX, percent),
            RightY = ApplyDeadZone(controller.RightY, percent),
            LeftTrigger = ApplyDeadZone(controller.LeftTrigger, percent),
            RightTrigger = ApplyDeadZone(controller.RightTrigger, percent)
        };

    internal static bool TryRemovePrefix(string? value, string prefix, out string source)
    {
        source = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;
        source = value[prefix.Length..];
        return source.Length > 0;
    }

    internal static IReadOnlyDictionary<string, bool> PhysicalMouse(EmulationPointerState pointer) => new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
    {
        ["Left"] = pointer.Left, ["Right"] = pointer.Right, ["Middle"] = pointer.Middle,
        ["XButton1"] = pointer.ExtendedButton1, ["XButton2"] = pointer.ExtendedButton2,
        ["WheelUp"] = pointer.Wheel > 0, ["WheelDown"] = pointer.Wheel < 0,
        ["WheelLeft"] = pointer.HorizontalWheel < 0, ["WheelRight"] = pointer.HorizontalWheel > 0
    };

    public static float ControllerSourceValue(string source, EmulationControllerState controller)
    {
        var name = SourceControlName(source);
        if (name is EmulationInputMappingConstants.LeftTrigger or EmulationInputMappingConstants.L2
            or EmulationInputMappingConstants.RightTrigger or EmulationInputMappingConstants.R2)
        {
            if (controller.Controls.TryGetValue(name, out var pressure)) return Math.Clamp(pressure, 0f, 1f);
            var trigger = name is EmulationInputMappingConstants.LeftTrigger or EmulationInputMappingConstants.L2 ? controller.LeftTrigger : controller.RightTrigger;
            if (trigger > 0) return (float)trigger / short.MaxValue;
        }
        if (ControllerButtons.TryGetValue(name, out var button))
            return (controller.Buttons & (1u << button)) != 0 ? 1f : 0f;
        if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase)
            && controller.Controls.TryGetValue(name[..^EmulationInputMappingConstants.PositiveSuffix.Length], out var positive))
            return IsGenericAxisDirection(name) ? Math.Clamp(positive * 2f - 1f, 0f, 1f) : positive;
        if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase)
            && controller.Controls.TryGetValue(name[..^EmulationInputMappingConstants.NegativeSuffix.Length], out var negative))
            return IsGenericAxisDirection(name) ? Math.Clamp(1f - negative * 2f, 0f, 1f) : -negative;
        if (controller.Controls.TryGetValue(name, out var value)) return value;
        return name switch
        {
            EmulationInputMappingConstants.LeftStickLeft => -controller.LeftX,
            EmulationInputMappingConstants.LeftStickRight => controller.LeftX,
            EmulationInputMappingConstants.LeftStickUp => -controller.LeftY,
            EmulationInputMappingConstants.LeftStickDown => controller.LeftY,
            EmulationInputMappingConstants.RightStickLeft => -controller.RightX,
            EmulationInputMappingConstants.RightStickRight => controller.RightX,
            EmulationInputMappingConstants.RightStickUp => -controller.RightY,
            EmulationInputMappingConstants.RightStickDown => controller.RightY,
            _ => 0f
        };
    }

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller) => IsControllerSourcePressed(source, controller, ControllerSourceValue(source, controller));

    public static bool IsControllerSourcePressed(string source, EmulationControllerState controller, float value)
    {
        var name = SourceControlName(source);
        if (IsGenericAxisDirection(name))
            return value > EmulationInputMappingConstants.PressedThreshold;
        if (ControllerButtons.ContainsKey(name))
            return value > EmulationInputMappingConstants.PressedThreshold;
        if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase))
            return value > EmulationInputMappingConstants.PositiveThreshold;
        if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase))
            return value > -EmulationInputMappingConstants.NegativeThreshold;
        if (controller.Controls.ContainsKey(name))
            return value > EmulationInputMappingConstants.PressedThreshold;
        return value > EmulationInputMappingConstants.AnalogThreshold;
    }

    private static bool IsGenericAxisDirection(string name)
    {
        var suffix = name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase)
            ? EmulationInputMappingConstants.PositiveSuffix
            : name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase) ? EmulationInputMappingConstants.NegativeSuffix : string.Empty;
        var numberLength = name.Length - EmulationInputMappingConstants.AxisPrefix.Length - suffix.Length;
        return suffix.Length > 0 && numberLength > 0 && name.StartsWith(EmulationInputMappingConstants.AxisPrefix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(name.AsSpan(EmulationInputMappingConstants.AxisPrefix.Length, numberLength), out var index) && index >= 0;
    }

    private static string SourceControlName(string source) => source.Split(EmulationInputMappingConstants.SourceSeparator, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

    public static short MapControllerAxis(IReadOnlyDictionary<string, string> mappings, string positiveCommand, string negativeCommand,
        EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        var positive = mappings.TryGetValue(positiveCommand, out var positiveSource) ? SourcePressure(positiveSource, controller, physical, keys, mouse) : 0f;
        var negative = mappings.TryGetValue(negativeCommand, out var negativeSource) ? SourcePressure(negativeSource, controller, physical, keys, mouse) : 0f;
        var value = positive - negative;
        return (short)Math.Round(value >= 0f ? value * short.MaxValue : -value * short.MinValue);
    }

    public static short MapControllerPressure(IReadOnlyDictionary<string, string> mappings, string command, EmulationControllerState controller,
        IReadOnlyList<EmulationControllerState> physical, IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) =>
        mappings.TryGetValue(command, out var source) ? (short)Math.Round(SourcePressure(source, controller, physical, keys, mouse) * short.MaxValue) : (short)0;

    private static float SourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse)
    {
        if (string.IsNullOrWhiteSpace(source)) return 0f;
        if (source.Contains('+') && !source.EndsWith('+')) return source.Split('+', StringSplitOptions.TrimEntries).Min(part => SourcePressure(
            part.Contains(':') ? part : EmulationInputMappingConstants.KeyboardPrefix + part, controller, physical, keys, mouse));
        if (source.StartsWith(EmulationInputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return EmulationKeyboardBindingFunctions.IsPressed(source, keys) ? 1f : 0f;
        if (source.StartsWith(EmulationInputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return mouse.GetValueOrDefault(source[EmulationInputMappingConstants.MousePrefix.Length..]) ? 1f : 0f;
        var selected = ResolveSourceController(source, physical, controller);
        return ControllerSourcePressure(source, selected);
    }

    public static float InputSourcePressure(string source, EmulationControllerState controller, EmulationInputSnapshot snapshot) => SourcePressure(
        source.Contains(':') ? source : EmulationInputMappingConstants.KeyboardPrefix + source, controller, snapshot.Controllers, snapshot.Keys,
        PhysicalMouse(snapshot.Pointer));

    public static float InputSourcePressure(string source, EmulationControllerState controller, IReadOnlyList<EmulationControllerState> physical,
        IReadOnlySet<EmulationKey> keys, IReadOnlyDictionary<string, bool> mouse) => SourcePressure(source, controller, physical, keys, mouse);

    public static float ControllerSourcePressure(string source, EmulationControllerState controller)
    {
        var pressure = ControllerSourceValue(source, controller);
        var name = SourceControlName(source);
        var rawStick = !controller.Controls.ContainsKey(name) && name is
            EmulationInputMappingConstants.LeftStickLeft or EmulationInputMappingConstants.LeftStickRight
            or EmulationInputMappingConstants.LeftStickUp or EmulationInputMappingConstants.LeftStickDown
            or EmulationInputMappingConstants.RightStickLeft or EmulationInputMappingConstants.RightStickRight
            or EmulationInputMappingConstants.RightStickUp or EmulationInputMappingConstants.RightStickDown;
        if (rawStick || Math.Abs(pressure) > 1f) pressure /= short.MaxValue;
        return Math.Clamp(pressure, 0f, 1f);
    }

    public static string? OppositeControllerAxisSource(string source)
    {
        if (source.Contains('+') && !source.EndsWith('+'))
        {
            var parts = source.Split('+', StringSplitOptions.TrimEntries);
            var found = false;
            for (var index = 0; index < parts.Length; index++)
                if (OppositeControllerAxisSource(parts[index]) is { } oppositePart)
                {
                    parts[index] = oppositePart;
                    found = true;
                }
            return found ? string.Join("+", parts) : null;
        }
        if (!source.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)) return null;
        var name = SourceControlName(source);
        string? opposite = name switch
        {
            EmulationInputMappingConstants.LeftStickLeft => EmulationInputMappingConstants.LeftStickRight,
            EmulationInputMappingConstants.LeftStickRight => EmulationInputMappingConstants.LeftStickLeft,
            EmulationInputMappingConstants.LeftStickUp => EmulationInputMappingConstants.LeftStickDown,
            EmulationInputMappingConstants.LeftStickDown => EmulationInputMappingConstants.LeftStickUp,
            EmulationInputMappingConstants.RightStickLeft => EmulationInputMappingConstants.RightStickRight,
            EmulationInputMappingConstants.RightStickRight => EmulationInputMappingConstants.RightStickLeft,
            EmulationInputMappingConstants.RightStickUp => EmulationInputMappingConstants.RightStickDown,
            EmulationInputMappingConstants.RightStickDown => EmulationInputMappingConstants.RightStickUp,
            _ => null
        };
        if (IsGenericAxisDirection(name))
        {
            if (name.EndsWith(EmulationInputMappingConstants.PositiveSuffix, StringComparison.OrdinalIgnoreCase))
                opposite = name[..^EmulationInputMappingConstants.PositiveSuffix.Length] + EmulationInputMappingConstants.NegativeSuffix;
            else if (name.EndsWith(EmulationInputMappingConstants.NegativeSuffix, StringComparison.OrdinalIgnoreCase))
                opposite = name[..^EmulationInputMappingConstants.NegativeSuffix.Length] + EmulationInputMappingConstants.PositiveSuffix;
        }
        return opposite is null ? null : source[..^name.Length] + opposite;
    }

    internal static bool IsMappedInputPressed(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot)
    {
        if (mapping.Contains('+') && !mapping.EndsWith('+')) return EmulationInputMappingFunctions.InputSourcePressure(mapping, controller, snapshot) > .5f;
        if (mapping.StartsWith(EmulationInputMappingConstants.KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
            return Enum.TryParse<EmulationKey>(mapping[EmulationInputMappingConstants.KeyboardPrefix.Length..], out var key)
                && snapshot.Keys.Contains(key);
        if (mapping.StartsWith(EmulationInputMappingConstants.MousePrefix, StringComparison.OrdinalIgnoreCase))
            return mapping[EmulationInputMappingConstants.MousePrefix.Length..] switch
            {
                "Left" => snapshot.Pointer.Left, "Right" => snapshot.Pointer.Right, "Middle" => snapshot.Pointer.Middle,
                "XButton1" => snapshot.Pointer.ExtendedButton1, "XButton2" => snapshot.Pointer.ExtendedButton2,
                "WheelUp" => snapshot.Pointer.Wheel > 0, "WheelDown" => snapshot.Pointer.Wheel < 0,
                "WheelLeft" => snapshot.Pointer.HorizontalWheel < 0, "WheelRight" => snapshot.Pointer.HorizontalWheel > 0,
                _ => false
            };
        return mapping.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase)
            && EmulationInputMappingFunctions.IsControllerSourcePressed(mapping, controller);
    }

    internal static short NormalizedAxis(float value) => (short)(Math.Clamp(value, -1f, 1f) * short.MaxValue);

    internal static float MappedInputMagnitude(string mapping, EmulationControllerState controller, EmulationInputSnapshot snapshot, int deadZone)
    {
        if (mapping.Contains('+') && !mapping.EndsWith('+'))
        {
            var combined = EmulationInputMappingFunctions.InputSourcePressure(mapping, controller, snapshot);
            return combined <= deadZone / 100f ? 0f : Math.Clamp(combined, 0f, 1f);
        }
        if (!mapping.StartsWith(EmulationInputMappingConstants.ControllerPrefix, StringComparison.OrdinalIgnoreCase))
            return IsMappedInputPressed(mapping, controller, snapshot) ? 1f : 0f;
        var value = EmulationInputMappingFunctions.ControllerSourcePressure(mapping, controller);
        return value <= deadZone / 100f ? 0f : Math.Clamp(value, 0f, 1f);
    }
}
