using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputDeviceInspector
{
    private static readonly GameInputKind[] StandardKinds =
    [
        GameInputKind.ArcadeStick,
        GameInputKind.FlightStick,
        GameInputKind.Gamepad,
        GameInputKind.RacingWheel
    ];

    internal static GameInputDeviceDescriptor? Describe(IGameInputDevice device)
    {
        if (device.GetDeviceInfo(out var pointer) < 0 || pointer == IntPtr.Zero) return null;
        var info = Marshal.PtrToStructure<GameInputDeviceInfo>(pointer);
        var id = "gameinput:" + info.DeviceId.ToHex().ToLowerInvariant();
        var gameInputName = Marshal.PtrToStringUTF8(info.DisplayName) ?? string.Empty;
        var pnpPath = Marshal.PtrToStringUTF8(info.PnpPath) ?? string.Empty;
        var identityChain = WindowsDeviceNameResolver.GetCandidates(pnpPath);
        var windowsName = WindowsDeviceNameResolver.ResolveProductName(pnpPath);
        var databaseName = GameControllerNameDatabase.Find(info.VendorId, info.ProductId);
        var productName = GameInputDeviceModelCatalog.ResolveProductName(
            info.VendorId, info.ProductId, windowsName, databaseName, gameInputName);
        var visual = GameInputDeviceModelCatalog.ResolveVisualModel(
            info.VendorId, info.ProductId, productName, info.SupportedInput);

        var controls = ReadControls(info.ControllerInfo);
        var gamepadInfo = ReadOptional<GameInputGamepadInfo>(info.GamepadInfo);
        var racingWheelInfo = ReadOptional<GameInputRacingWheelInfo>(info.RacingWheelInfo);
        var extraAxes = new Dictionary<GameInputKind, IReadOnlyList<byte>>();
        var extraButtons = new Dictionary<GameInputKind, IReadOnlyList<byte>>();
        foreach (var kind in StandardKinds)
        {
            extraAxes[kind] = ReadExtraIndexes(device, kind, axes: true);
            extraButtons[kind] = ReadExtraIndexes(device, kind, axes: false);
        }

        var standard = new GameInputStandardCapabilities(
            gamepadInfo?.SupportedLayout ?? GameInputGamepadButtons.None,
            gamepadInfo?.ExtraButtonCount ?? 0,
            gamepadInfo?.ExtraAxisCount ?? 0,
            info.ArcadeStickInfo != IntPtr.Zero,
            info.FlightStickInfo != IntPtr.Zero,
            info.GamepadInfo != IntPtr.Zero,
            info.RacingWheelInfo != IntPtr.Zero,
            racingWheelInfo?.HasClutch ?? false,
            racingWheelInfo?.HasHandbrake ?? false,
            racingWheelInfo?.HasPatternShifter ?? false,
            racingWheelInfo?.MaxWheelAngle ?? 0,
            extraAxes,
            extraButtons);

        var hasHaptics = TryReadHaptics(device, out var hapticInfo);
        var endpoint = hasHaptics ? ReadHapticEndpoint(hapticInfo) : string.Empty;
        var locations = hasHaptics ? ReadHapticLocations(hapticInfo) : [];

        return new GameInputDeviceDescriptor(
            id,
            productName,
            gameInputName,
            pnpPath,
            info.VendorId,
            info.ProductId,
            info.RevisionNumber,
            info.HardwareVersion,
            info.FirmwareVersion,
            info.DeviceRootId.ToHex(),
            info.ContainerId,
            info.DeviceFamily,
            info.Usage,
            info.SupportedInput,
            info.SupportedRumbleMotors,
            info.SupportedSystemButtons,
            ReadManufacturer(identityChain),
            identityChain,
            controls,
            standard,
            ReadForceFeedbackMotors(device, info),
            ReadReports(info.InputReportInfo, info.InputReportCount),
            ReadReports(info.OutputReportInfo, info.OutputReportCount),
            hasHaptics,
            endpoint,
            locations,
            visual.Model,
            visual.Exact)
        {
            Status = device.GetDeviceStatus()
        };
    }

    private static IReadOnlyList<GameInputControlDescriptor> ReadControls(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero) return [];
        var info = Marshal.PtrToStructure<GameInputControllerInfo>(pointer);
        if (info.AxisCount > 1024 || info.ButtonCount > 1024 || info.SwitchCount > 1024) return [];
        var controls = new List<GameInputControlDescriptor>(
            checked((int)(info.AxisCount + info.ButtonCount + info.SwitchCount)));
        foreach (var item in ReadLabels(info.AxisLabels, info.AxisCount).Select((label, index) => (label, index)))
            controls.Add(new GameInputControlDescriptor(GameInputControlType.Axis, item.index, item.label));
        foreach (var item in ReadLabels(info.ButtonLabels, info.ButtonCount).Select((label, index) => (label, index)))
            controls.Add(new GameInputControlDescriptor(GameInputControlType.Button, item.index, item.label));

        if (info.SwitchInfo == IntPtr.Zero || info.SwitchCount > 1024) return controls;
        var stride = Marshal.SizeOf<GameInputControllerSwitchInfo>();
        for (var index = 0; index < info.SwitchCount; index++)
        {
            var switchInfo = Marshal.PtrToStructure<GameInputControllerSwitchInfo>(
                IntPtr.Add(info.SwitchInfo, checked((int)index * stride)));
            controls.Add(new GameInputControlDescriptor(
                GameInputControlType.Switch,
                checked((int)index),
                GameInputLabel.None,
                switchInfo.Kind,
                ReadSwitchLabels(switchInfo)));
        }
        return controls;
    }

    private static IReadOnlyList<GameInputLabel> ReadLabels(IntPtr pointer, uint count)
    {
        if (pointer == IntPtr.Zero || count == 0 || count > 1024) return [];
        var values = new int[count];
        Marshal.Copy(pointer, values, 0, checked((int)count));
        return values.Select(value => (GameInputLabel)value).ToArray();
    }

    private static unsafe IReadOnlyList<GameInputLabel> ReadSwitchLabels(GameInputControllerSwitchInfo info)
    {
        var result = new GameInputLabel[8];
        int* labels = info.Labels;
        for (var index = 0; index < result.Length; index++)
            result[index] = (GameInputLabel)labels[index];
        return result;
    }

    private static T? ReadOptional<T>(IntPtr pointer) where T : struct =>
        pointer == IntPtr.Zero ? null : Marshal.PtrToStructure<T>(pointer);

    private static unsafe IReadOnlyList<byte> ReadExtraIndexes(
        IGameInputDevice device, GameInputKind kind, bool axes)
    {
        try
        {
            var countResult = axes
                ? device.GetExtraAxisCount(kind, out var count)
                : device.GetExtraButtonCount(kind, out count);
            if (countResult < 0 || count == 0 || count > 1024) return [];
            var result = new byte[count];
            fixed (byte* pointer = result)
            {
                var indexesResult = axes
                    ? device.GetExtraAxisIndexes(kind, count, (IntPtr)pointer)
                    : device.GetExtraButtonIndexes(kind, count, (IntPtr)pointer);
                return indexesResult < 0 ? [] : result;
            }
        }
        catch (COMException) { return []; }
    }

    private static IReadOnlyList<GameInputForceFeedbackMotorDescriptor> ReadForceFeedbackMotors(
        IGameInputDevice device, GameInputDeviceInfo info)
    {
        if (info.ForceFeedbackMotorInfo == IntPtr.Zero ||
            info.ForceFeedbackMotorCount == 0 || info.ForceFeedbackMotorCount > 64) return [];
        var result = new List<GameInputForceFeedbackMotorDescriptor>();
        var stride = Marshal.SizeOf<GameInputForceFeedbackMotorInfo>();
        for (var index = 0; index < info.ForceFeedbackMotorCount; index++)
        {
            var motor = Marshal.PtrToStructure<GameInputForceFeedbackMotorInfo>(
                IntPtr.Add(info.ForceFeedbackMotorInfo, checked((int)index * stride)));
            var effects = new List<GameInputForceFeedbackEffectKind>();
            Add(effects, motor.IsConstantEffectSupported, GameInputForceFeedbackEffectKind.Constant);
            Add(effects, motor.IsRampEffectSupported, GameInputForceFeedbackEffectKind.Ramp);
            Add(effects, motor.IsSineWaveEffectSupported, GameInputForceFeedbackEffectKind.SineWave);
            Add(effects, motor.IsSquareWaveEffectSupported, GameInputForceFeedbackEffectKind.SquareWave);
            Add(effects, motor.IsTriangleWaveEffectSupported, GameInputForceFeedbackEffectKind.TriangleWave);
            Add(effects, motor.IsSawtoothUpWaveEffectSupported, GameInputForceFeedbackEffectKind.SawtoothUpWave);
            Add(effects, motor.IsSawtoothDownWaveEffectSupported, GameInputForceFeedbackEffectKind.SawtoothDownWave);
            Add(effects, motor.IsSpringEffectSupported, GameInputForceFeedbackEffectKind.Spring);
            Add(effects, motor.IsFrictionEffectSupported, GameInputForceFeedbackEffectKind.Friction);
            Add(effects, motor.IsDamperEffectSupported, GameInputForceFeedbackEffectKind.Damper);
            Add(effects, motor.IsInertiaEffectSupported, GameInputForceFeedbackEffectKind.Inertia);
            var poweredOn = false;
            try { poweredOn = device.IsForceFeedbackMotorPoweredOn((uint)index); }
            catch (COMException) { }
            result.Add(new GameInputForceFeedbackMotorDescriptor(
                checked((int)index), motor.SupportedAxes, effects, poweredOn));
        }
        return result;
    }

    private static void Add(
        ICollection<GameInputForceFeedbackEffectKind> effects,
        bool supported,
        GameInputForceFeedbackEffectKind effect)
    {
        if (supported) effects.Add(effect);
    }

    private static IReadOnlyList<GameInputRawReportDescriptor> ReadReports(IntPtr pointer, uint count)
    {
        if (pointer == IntPtr.Zero || count == 0 || count > 1024) return [];
        var result = new List<GameInputRawReportDescriptor>(checked((int)count));
        var stride = Marshal.SizeOf<GameInputRawDeviceReportInfo>();
        for (var index = 0; index < count; index++)
        {
            var report = Marshal.PtrToStructure<GameInputRawDeviceReportInfo>(
                IntPtr.Add(pointer, checked((int)index * stride)));
            result.Add(new GameInputRawReportDescriptor(report.Kind, report.Id, report.Size));
        }
        return result;
    }

    private static string ReadManufacturer(IReadOnlyList<string> identityChain)
    {
        const string marker = ":Manufacturer=";
        var item = identityChain.FirstOrDefault(value =>
            value.Contains(marker, StringComparison.OrdinalIgnoreCase));
        if (item is null) return string.Empty;
        var position = item.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return item[(position + marker.Length)..].Trim();
    }


    private static bool TryReadHaptics(IGameInputDevice device, out GameInputHapticInfo info)
    {
        info = default;
        try { return device.GetHapticInfo(out info) >= 0; }
        catch (COMException) { return false; }
    }

    private static unsafe string ReadHapticEndpoint(GameInputHapticInfo info)
    {
        char* endpoint = info.AudioEndpointId;
        var length = 0;
        while (length < 256 && endpoint[length] != '\0') length++;
        return new string(endpoint, 0, length);
    }

    private static unsafe IReadOnlyList<Guid> ReadHapticLocations(GameInputHapticInfo info)
    {
        var count = checked((int)Math.Min(info.LocationCount, 8));
        var result = new Guid[count];
        byte* pointer = info.Locations;
        for (var index = 0; index < count; index++)
            result[index] = new Guid(new ReadOnlySpan<byte>(pointer + index * 16, 16));
        return result;
    }
}
