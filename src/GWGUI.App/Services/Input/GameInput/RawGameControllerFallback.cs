using GWGUI.Emulation;
using Windows.Gaming.Input;
using System.Windows;

namespace GWGUI.App.Services.Input.GameInput;

internal static class RawGameControllerFallback
{
    private static readonly object Sync = new();
    private static IReadOnlyList<RawDevice> _devices = [];
    private static bool _monitoring;

    internal static void StartMonitoring()
    {
        if (_monitoring) return;
        _monitoring = true;
        RefreshOnUiThread();
    }

    internal static void StopMonitoring()
    {
        if (!_monitoring) return;
        lock (Sync) _devices = [];
        _monitoring = false;
    }

    internal static void RefreshOnUiThread()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(Refresh);
            return;
        }
        Refresh();
    }

    internal static void Refresh()
    {
        try
        {
            var devices = RawGameController.RawGameControllers
                .Select((controller, index) => Describe(controller, index))
                .Where(device => device is not null)
                .Cast<RawDevice>()
                .OrderBy(device => device.Descriptor.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(device => device.Descriptor.Id, StringComparer.Ordinal)
                .ToArray();
            lock (Sync) _devices = DisambiguateDuplicateNames(devices);
        }
        catch
        {
            lock (Sync) _devices = [];
        }
    }

    private static IReadOnlyList<RawDevice> DisambiguateDuplicateNames(IReadOnlyList<RawDevice> devices)
    {
        var duplicates = devices.GroupBy(device => device.Descriptor.ProductName,
                StringComparer.CurrentCultureIgnoreCase)
            .Where(group => group.Count() > 1)
            .ToDictionary(group => group.Key, group => group.OrderBy(device => device.Descriptor.Id,
                StringComparer.Ordinal).ToArray(), StringComparer.CurrentCultureIgnoreCase);
        if (duplicates.Count == 0) return devices;
        return devices.Select(device =>
        {
            if (!duplicates.TryGetValue(device.Descriptor.ProductName, out var group)) return device;
            var number = Array.IndexOf(group, device) + 1;
            return device with
            {
                Descriptor = device.Descriptor with
                {
                    ProductName = $"{device.Descriptor.ProductName} #{number}"
                }
            };
        }).ToArray();
    }

    internal static IReadOnlyList<GameInputDeviceDescriptor> MergeDescriptors(
        IReadOnlyList<GameInputDeviceDescriptor> gameInput)
    {
        var fallback = DistinctFallback(gameInput);
        return gameInput.Concat(fallback.Select(device => device.Descriptor))
            .OrderBy(device => device.ProductName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(device => device.Id, StringComparer.Ordinal)
            .ToArray();
    }

    internal static IReadOnlyList<EmulationControllerState> ReadAll(
        IReadOnlyList<GameInputDeviceDescriptor> gameInput)
    {
        return DistinctFallback(gameInput).Select(ReadState).ToArray();
    }

    internal static bool TryReadDetailed(string deviceId, out GameInputLiveState state)
    {
        RawDevice? device;
        lock (Sync)
            device = _devices.FirstOrDefault(item =>
                string.Equals(item.Descriptor.Id, deviceId, StringComparison.OrdinalIgnoreCase));
        if (device is null)
        {
            state = GameInputLiveState.Empty(deviceId);
            return false;
        }
        state = ReadDetailed(device);
        return true;
    }

    internal static string? GetName(string deviceId)
    {
        lock (Sync)
            return _devices.FirstOrDefault(item =>
                string.Equals(item.Descriptor.Id, deviceId, StringComparison.OrdinalIgnoreCase))
                ?.Descriptor.ProductName;
    }

    private static IReadOnlyList<RawDevice> DistinctFallback(
        IReadOnlyList<GameInputDeviceDescriptor> gameInput)
    {
        RawDevice[] snapshot;
        lock (Sync) snapshot = _devices.ToArray();

        var remainingByVidPid = gameInput
            .Where(device => device.VendorId != 0 || device.ProductId != 0)
            .GroupBy(device => (device.VendorId, device.ProductId))
            .ToDictionary(group => group.Key, group => group.Count());
        var remainingByName = gameInput
            .GroupBy(device => Normalize(device.ProductName), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);
        var result = new List<RawDevice>();
        foreach (var device in snapshot)
        {
            var descriptor = device.Descriptor;
            var vidPid = (descriptor.VendorId, descriptor.ProductId);
            if (vidPid != (0, 0) &&
                remainingByVidPid.TryGetValue(vidPid, out var sameVidPid) && sameVidPid > 0)
            {
                remainingByVidPid[vidPid] = sameVidPid - 1;
                DecrementName(remainingByName, descriptor.ProductName);
                continue;
            }
            var name = Normalize(descriptor.ProductName);
            if (remainingByName.TryGetValue(name, out var sameName) && sameName > 0)
            {
                remainingByName[name] = sameName - 1;
                continue;
            }
            result.Add(device);
        }
        return result;
    }

    private static void DecrementName(IDictionary<string, int> counts, string name)
    {
        var normalized = Normalize(name);
        if (counts.TryGetValue(normalized, out var count) && count > 0)
            counts[normalized] = count - 1;
    }

    private static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static RawDevice? Describe(RawGameController controller, int index)
    {
        var id = controller.NonRoamableId?.TrimEnd('\0');
        if (string.IsNullOrWhiteSpace(id))
            id = $"{controller.HardwareVendorId:X4}:{controller.HardwareProductId:X4}:{index}";
        var database = GameControllerNameDatabase.FindEntry(
            controller.HardwareVendorId, controller.HardwareProductId);
        var productName = GameInputDeviceModelCatalog.ResolveProductName(
            controller.HardwareVendorId, controller.HardwareProductId,
            controller.DisplayName, database?.Name, controller.DisplayName);
        var controls = Enumerable.Range(0, controller.AxisCount)
                .Select(axis => new GameInputControlDescriptor(
                    GameInputControlType.Axis, axis, GameInputLabel.None))
            .Concat(Enumerable.Range(0, controller.ButtonCount)
                .Select(button => new GameInputControlDescriptor(
                    GameInputControlType.Button, button, GameInputLabel.None)))
            .Concat(Enumerable.Range(0, controller.SwitchCount)
                .Select(item => new GameInputControlDescriptor(
                    GameInputControlType.Switch, item, GameInputLabel.None,
                    GameInputSwitchKind.EightWay)))
            .ToArray();
        var supported = GameInputKind.Controller;
        var deviceId = "rawgamecontroller:" + id;
        var visual = GameInputDeviceModelCatalog.ResolveVisualModel(
            controller.HardwareVendorId, controller.HardwareProductId, productName, supported);
        if (ControllerVisualProfileStore.TryGet(deviceId, out var profile))
        {
            productName = ControllerVisualProfileStore.DisplayName(profile.Model, profile.DisplayName);
            visual = (profile.Model, false);
        }
        var descriptor = new GameInputDeviceDescriptor(
            deviceId,
            productName,
            productName,
            string.Empty,
            controller.HardwareVendorId,
            controller.HardwareProductId,
            0,
            default,
            default,
            id,
            Guid.Empty,
            GameInputDeviceFamily.Hid,
            new GameInputUsage { Page = 1, Id = 5 },
            supported,
            GameInputRumbleMotors.None,
            GameInputSystemButtons.None,
            string.Empty,
            [],
            controls,
            EmptyCapabilities(),
            [],
            [],
            [],
            false,
            string.Empty,
            [],
            visual.Model,
            visual.Exact)
        {
            Status = GameInputDeviceStatus.Connected
        };
        return new RawDevice(controller, descriptor, database);
    }

    private static GameInputStandardCapabilities EmptyCapabilities() => new(
        GameInputGamepadButtons.None, 0, 0, false, false, false, false,
        false, false, false, 0,
        new Dictionary<GameInputKind, IReadOnlyList<byte>>(),
        new Dictionary<GameInputKind, IReadOnlyList<byte>>());

    private static EmulationControllerState ReadState(RawDevice device)
    {
        if (!TryRead(device, out var buttons, out var switches, out var axes, out _))
            return EmulationControllerState.Empty with { DeviceId = device.Descriptor.Id };
        var values = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        AddMappedControls(values, device.DatabaseEntry, buttons, switches, axes);
        for (var index = 0; index < buttons.Length; index++)
            values.TryAdd($"Button{index + 1}", buttons[index] ? 1f : 0f);
        for (var index = 0; index < axes.Length; index++)
            values.TryAdd($"Axis{index}", (float)axes[index]);
        AddSwitches(values, switches);
        return new EmulationControllerState(
            0,
            MappedAxis(device.DatabaseEntry, axes, "leftx"),
            MappedAxis(device.DatabaseEntry, axes, "lefty"),
            MappedAxis(device.DatabaseEntry, axes, "rightx"),
            MappedAxis(device.DatabaseEntry, axes, "righty"),
            MappedTrigger(device.DatabaseEntry, axes, buttons, "lefttrigger"),
            MappedTrigger(device.DatabaseEntry, axes, buttons, "righttrigger"))
        {
            DeviceId = device.Descriptor.Id,
            Controls = new EmulationControllerControls(values)
        };
    }

    private static GameInputLiveState ReadDetailed(RawDevice device)
    {
        if (!TryRead(device, out var buttons, out var switches, out var axes, out var timestamp))
            return GameInputLiveState.Empty(device.Descriptor.Id);
        var controls = new List<GameInputControlValue>();
        controls.AddRange(axes.Select((value, index) => new GameInputControlValue(
            GameInputControlType.Axis, index, GameInputLabel.None, (float)value)));
        controls.AddRange(buttons.Select((value, index) => new GameInputControlValue(
            GameInputControlType.Button, index, GameInputLabel.None, value ? 1f : 0f)));
        controls.AddRange(switches.Select((value, index) => new GameInputControlValue(
            GameInputControlType.Switch, index, GameInputLabel.None, 0f, Convert(value))));
        return new GameInputLiveState(
            device.Descriptor.Id, timestamp, GameInputKind.Controller,
            controls, [], GameInputSystemButtons.None, null, null, null, null, true);
    }

    private static bool TryRead(RawDevice device, out bool[] buttons,
        out GameControllerSwitchPosition[] switches, out double[] axes, out ulong timestamp)
    {
        buttons = new bool[device.Controller.ButtonCount];
        switches = new GameControllerSwitchPosition[device.Controller.SwitchCount];
        axes = new double[device.Controller.AxisCount];
        try
        {
            timestamp = device.Controller.GetCurrentReading(buttons, switches, axes);
            return true;
        }
        catch
        {
            timestamp = 0;
            return false;
        }
    }

    private static void AddMappedControls(IDictionary<string, float> values,
        GameControllerDatabaseEntry? entry, IReadOnlyList<bool> buttons,
        IReadOnlyList<GameControllerSwitchPosition> switches, IReadOnlyList<double> axes)
    {
        if (entry is null) return;
        foreach (var mapping in entry.Mappings)
        {
            var name = LogicalControlName(mapping.Key);
            if (name is null) continue;
            values[name] = ReadMappedValue(mapping.Value, buttons, switches, axes);
        }
    }

    private static string? LogicalControlName(string name) => name.ToLowerInvariant() switch
    {
        "a" => "ButtonA", "b" => "ButtonB", "x" => "ButtonX", "y" => "ButtonY",
        "back" => "View", "start" => "Menu", "guide" => "XboxButton",
        "leftshoulder" => "LeftShoulder", "rightshoulder" => "RightShoulder",
        "lefttrigger" => "LeftTrigger", "righttrigger" => "RightTrigger",
        "leftstick" => "LeftStickClick", "rightstick" => "RightStickClick",
        "dpup" => "DPadUp", "dpdown" => "DPadDown",
        "dpleft" => "DPadLeft", "dpright" => "DPadRight",
        _ => null
    };

    private static float ReadMappedValue(string source, IReadOnlyList<bool> buttons,
        IReadOnlyList<GameControllerSwitchPosition> switches, IReadOnlyList<double> axes)
    {
        var value = source.Trim();
        if (value.Length > 1 && value[0] == 'b' &&
            int.TryParse(value.AsSpan(1), out var button) && button < buttons.Count)
            return buttons[button] ? 1f : 0f;
        var sign = value.Length > 0 && (value[0] == '+' || value[0] == '-') ? value[0] : '\0';
        var axisText = sign == '\0' ? value : value[1..];
        if (axisText.Length > 1 && axisText[0] == 'a' &&
            int.TryParse(axisText.AsSpan(1), out var axis) && axis < axes.Count)
            return sign == '-' ? (axes[axis] < .25 ? 1f : 0f)
                : sign == '+' ? (axes[axis] > .75 ? 1f : 0f)
                : (float)axes[axis];
        if (value.Length > 3 && value[0] == 'h')
        {
            var parts = value[1..].Split('.');
            if (parts.Length == 2 && int.TryParse(parts[0], out var hat) && hat < switches.Count &&
                int.TryParse(parts[1], out var mask))
                return HatMatches(switches[hat], mask) ? 1f : 0f;
        }
        return 0f;
    }

    private static bool HatMatches(GameControllerSwitchPosition position, int mask) =>
        (mask & 1) != 0 && HasDirection(position, "Up") ||
        (mask & 2) != 0 && HasDirection(position, "Right") ||
        (mask & 4) != 0 && HasDirection(position, "Down") ||
        (mask & 8) != 0 && HasDirection(position, "Left");

    private static short MappedAxis(GameControllerDatabaseEntry? entry,
        IReadOnlyList<double> axes, string name)
    {
        if (entry is null || !entry.Mappings.TryGetValue(name, out var source) ||
            source.Length < 2 || source[0] != 'a' ||
            !int.TryParse(source.AsSpan(1), out var index))
            return 0;
        return Axis(axes, index);
    }

    private static byte MappedTrigger(GameControllerDatabaseEntry? entry,
        IReadOnlyList<double> axes, IReadOnlyList<bool> buttons, string name)
    {
        if (entry is null || !entry.Mappings.TryGetValue(name, out var source)) return 0;
        return (byte)Math.Clamp(Math.Round(ReadMappedValue(source, buttons, [], axes) * byte.MaxValue),
            byte.MinValue, byte.MaxValue);
    }

    private static short Axis(IReadOnlyList<double> axes, int index)
    {
        if (index >= axes.Count) return 0;
        return (short)Math.Clamp(Math.Round((axes[index] * 2d - 1d) * short.MaxValue),
            short.MinValue, short.MaxValue);
    }

    private static void AddSwitches(IDictionary<string, float> values,
        IReadOnlyList<GameControllerSwitchPosition> switches)
    {
        for (var index = 0; index < switches.Count; index++)
        {
            var position = switches[index];
            foreach (var direction in new[] { "Up", "Down", "Left", "Right" })
                values[$"Switch{index + 1}{direction}"] = HasDirection(position, direction) ? 1f : 0f;
            if (index != 0) continue;
            values["DPadUp"] = HasDirection(position, "Up") ? 1f : 0f;
            values["DPadDown"] = HasDirection(position, "Down") ? 1f : 0f;
            values["DPadLeft"] = HasDirection(position, "Left") ? 1f : 0f;
            values["DPadRight"] = HasDirection(position, "Right") ? 1f : 0f;
        }
    }

    private static bool HasDirection(GameControllerSwitchPosition position, string direction) =>
        direction switch
        {
            "Up" => position is GameControllerSwitchPosition.Up or
                GameControllerSwitchPosition.UpLeft or GameControllerSwitchPosition.UpRight,
            "Down" => position is GameControllerSwitchPosition.Down or
                GameControllerSwitchPosition.DownLeft or GameControllerSwitchPosition.DownRight,
            "Left" => position is GameControllerSwitchPosition.Left or
                GameControllerSwitchPosition.UpLeft or GameControllerSwitchPosition.DownLeft,
            "Right" => position is GameControllerSwitchPosition.Right or
                GameControllerSwitchPosition.UpRight or GameControllerSwitchPosition.DownRight,
            _ => false
        };

    private static GameInputSwitchPosition Convert(GameControllerSwitchPosition position) =>
        position switch
        {
            GameControllerSwitchPosition.Up => GameInputSwitchPosition.Up,
            GameControllerSwitchPosition.UpRight => GameInputSwitchPosition.UpRight,
            GameControllerSwitchPosition.Right => GameInputSwitchPosition.Right,
            GameControllerSwitchPosition.DownRight => GameInputSwitchPosition.DownRight,
            GameControllerSwitchPosition.Down => GameInputSwitchPosition.Down,
            GameControllerSwitchPosition.DownLeft => GameInputSwitchPosition.DownLeft,
            GameControllerSwitchPosition.Left => GameInputSwitchPosition.Left,
            GameControllerSwitchPosition.UpLeft => GameInputSwitchPosition.UpLeft,
            _ => GameInputSwitchPosition.Center
        };

    private sealed record RawDevice(
        RawGameController Controller,
        GameInputDeviceDescriptor Descriptor,
        GameControllerDatabaseEntry? DatabaseEntry);
}
