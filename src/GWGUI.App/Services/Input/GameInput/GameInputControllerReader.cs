using GWGUI.App.Contracts.Services.Input;
using GWGUI.Emulation;
using GWGUI.App.Functions.Input.Controllers;
using GWGUI.App.Functions.Input.Keyboard;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace GWGUI.App.Services.Input.GameInput;

internal static class GameInputControllerReader
{
    private const GameInputKind ControllerKinds = GameInputKind.Controller | GameInputKind.ArcadeStick |
        GameInputKind.FlightStick | GameInputKind.Gamepad | GameInputKind.RacingWheel;
    private static readonly GameInputKind[] DeviceCallbackFilters =
    [
        GameInputKind.RawDeviceReport,
        ControllerKinds,
        GameInputKind.Keyboard,
        GameInputKind.Mouse
    ];
    private static readonly GameInputKind[] ControllerRefreshFilters =
    [
        GameInputKind.RawDeviceReport,
        ControllerKinds
    ];
    internal static IReadOnlyList<GameInputKind> RegisteredDeviceCallbackFilters => DeviceCallbackFilters;
    internal static IReadOnlyList<GameInputKind> RegisteredControllerRefreshFilters => ControllerRefreshFilters;
    private static readonly object Sync = new();
    private static readonly GameInputWorker Worker = new();
    private static readonly Dictionary<string, DeviceEntry> Devices = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentQueue<PendingDeviceChange> PendingDeviceChanges = new();
    private static readonly Dictionary<string, GameInputSystemButtons> SystemButtons = new(StringComparer.OrdinalIgnoreCase);
    private static readonly GameInputDeviceCallback DeviceCallback = DeviceChanged;
    private static readonly GameInputSystemButtonCallback SystemButtonCallback = SystemButtonsChanged;
    private static readonly GameInputReadingCallback RawReadingCallback = RawReadingChanged;
    private static IGameInput? _gameInput;
    private static readonly List<ulong> DeviceTokens = [];
    private static ulong _systemButtonToken;
    private static bool _initialized;
    private static readonly Dictionary<string, GameInputMouseState> PreviousMouse = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, byte[]> LatestRawReports = new(StringComparer.OrdinalIgnoreCase);

    internal static bool InitializationFailed { get; private set; }
    internal static string LastReadDiagnostic { get; private set; } = string.Empty;
    internal static string LastDetailedReadDiagnostic { get; private set; } = string.Empty;
    internal static string LastCallbackDiagnostic { get; private set; } = string.Empty;
    private static readonly Dictionary<string, string> EnumerationDiagnostics = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> RawEnumerationDiagnostics = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> DeviceCallbackTraceLines = [];
    internal static string LastEnumerationDiagnostic => string.Join(Environment.NewLine, EnumerationDiagnostics.OrderBy(item => item.Key).Select(item => item.Value));
    internal static string RawEnumerationDiagnostic => string.Join(Environment.NewLine, RawEnumerationDiagnostics.OrderBy(item => item.Key).Select(item => item.Value));
    internal static string DeviceCallbackTrace => string.Join(Environment.NewLine, DeviceCallbackTraceLines);

    internal static void StartMonitoring()
    {
        RawGameControllerFallback.StartMonitoring();
        Worker.Post(EnsureInitialized);
    }

    internal static void StopMonitoring()
    {
        RawGameControllerFallback.StopMonitoring();
        Worker.Invoke(() =>
        {
            lock (Sync)
            {
                Shutdown();
                _initialized = false;
                InitializationFailed = false;
            }
        });
    }

    internal static IReadOnlyList<GameInputDeviceDescriptor> GetConnectedControllerDetailsCached()
    {
        GameInputDeviceDescriptor[] gameInput;
        lock (Sync)
            gameInput = Devices.Values.Where(entry => entry.IsController)
                .Select(entry => entry.Descriptor).ToArray();
        return RawGameControllerFallback.MergeDescriptors(gameInput);
    }

    internal static IReadOnlyList<EmulationControllerState> ReadAll() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        GameInputDeviceDescriptor[] descriptors;
        EmulationControllerState[] gameInput;
        lock (Sync)
        {
            var entries = Devices.Values.Where(entry => entry.IsController)
                .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
            descriptors = entries.Select(entry => entry.Descriptor).ToArray();
            gameInput = entries.Select(Read).ToArray();
        }
        return gameInput.Concat(RawGameControllerFallback.ReadAll(descriptors))
            .Select(ControllerAnalogDeadZoneFunctions.ApplyConfigured).ToArray();
    });

    internal static IReadOnlyList<GameControllerDevice> GetConnectedDevices() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        return GetConnectedControllerDetailsCached()
            .Select(device => new GameControllerDevice(device.Id, device.ProductName)).ToArray();
    });

    internal static IReadOnlyList<GameInputDeviceDescriptor> GetConnectedControllerDetails() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        return GetConnectedControllerDetailsCached();
    });

    internal static GameInputLiveState ReadDetailedState(string deviceId) => Worker.Invoke(() =>
    {
        EnsureInitialized();
        GameInputLiveState state;
        lock (Sync)
            if (Devices.TryGetValue(deviceId, out var entry) && entry.IsController)
                state = ReadDetailed(entry);
            else
                state = RawGameControllerFallback.TryReadDetailed(deviceId, out var fallback)
                    ? fallback : GameInputLiveState.Empty(deviceId);
        return ControllerAnalogDeadZoneFunctions.ApplyConfigured(state);
    });

    internal static IReadOnlyList<GameInputLiveState> ReadAllDetailedStates() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        GameInputDeviceDescriptor[] gameInputDescriptors;
        var result = new List<GameInputLiveState>();
        lock (Sync)
        {
            var entries = Devices.Values.Where(entry => entry.IsController)
                .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
            gameInputDescriptors = entries.Select(entry => entry.Descriptor).ToArray();
            result.AddRange(entries.Select(ReadDetailed));
        }
        foreach (var descriptor in RawGameControllerFallback.MergeDescriptors(gameInputDescriptors))
        {
            if (result.Any(state => string.Equals(state.DeviceId, descriptor.Id,
                    StringComparison.OrdinalIgnoreCase))) continue;
            if (RawGameControllerFallback.TryReadDetailed(descriptor.Id, out var state))
                result.Add(state);
        }
        return result.Select(ControllerAnalogDeadZoneFunctions.ApplyConfigured).ToArray();
    });

    internal static string? GetControllerName(string deviceId)
    {
        lock (Sync)
            if (Devices.TryGetValue(deviceId, out var entry) && entry.IsController)
                return entry.Name;
        return RawGameControllerFallback.GetName(deviceId);
    }

    internal static void RefreshConnectedDevices() => Worker.Invoke(() =>
    {
        if (InitializationFailed)
        {
            lock (Sync)
            {
                if (!Shutdown()) return;
                _initialized = false;
                InitializationFailed = false;
            }
        }

        EnsureInitialized();
        if (_gameInput is null) return;

        // GameInput can omit one of multiple controllers when raw, standard,
        // keyboard and mouse kinds share one callback. Enumerate each family
        // independently, then merge the queued devices by their GameInput ID.
        var refreshTokens = new List<ulong>(ControllerRefreshFilters.Length);
        foreach (var filter in ControllerRefreshFilters)
        {
            var result = _gameInput.RegisterDeviceCallback(null, filter,
                GameInputDeviceStatus.Any, GameInputEnumerationKind.Blocking, new IntPtr(unchecked((long)(uint)filter)),
                Marshal.GetFunctionPointerForDelegate(DeviceCallback), out var refreshToken);
            if (result < 0)
            {
                foreach (var token in refreshTokens) SafeUnregister(token);
                LastCallbackDiagnostic =
                    $"GameInput refresh registration failed for {filter}: 0x{result:X8}";
                return;
            }
            refreshTokens.Add(refreshToken);
        }

        DrainDeviceChanges();
        RawGameControllerFallback.RefreshOnUiThread();
        foreach (var token in refreshTokens)
            if (!SafeUnregister(token))
                LastCallbackDiagnostic =
                    $"GameInput refresh callback could not be unregistered: 0x{token:X16}";
    });

    internal static void SetFocusPolicyForDiagnostics(GameInputFocusPolicy policy) => Worker.Invoke(() =>
    {
        EnsureInitialized();
        _gameInput?.SetFocusPolicy(policy);
    });

    internal static bool SetRumble(
        string deviceId,
        float lowFrequency,
        float highFrequency,
        float leftTrigger,
        float rightTrigger) => Worker.Invoke(() => SetRumbleCore(
            deviceId, lowFrequency, highFrequency, leftTrigger, rightTrigger));

    private static unsafe bool SetRumbleCore(
        string deviceId,
        float lowFrequency,
        float highFrequency,
        float leftTrigger,
        float rightTrigger)
    {
        EnsureInitialized();
        lock (Sync)
        {
            if (!Devices.TryGetValue(deviceId, out var entry) || !entry.IsController) return false;
            var rumble = new GameInputRumbleParams
            {
                LowFrequency = Math.Clamp(lowFrequency, 0f, 1f),
                HighFrequency = Math.Clamp(highFrequency, 0f, 1f),
                LeftTrigger = Math.Clamp(leftTrigger, 0f, 1f),
                RightTrigger = Math.Clamp(rightTrigger, 0f, 1f)
            };
            try
            {
                entry.Device.SetRumbleState((IntPtr)(&rumble));
                return true;
            }
            catch (Exception exception) when (IsInteropFailure(exception))
            {
                return false;
            }
        }
    }

    internal static GameInputPhysicalState ReadPhysicalInput() => Worker.Invoke(() =>
    {
        EnsureInitialized();
        lock (Sync)
        {
            var keys = new HashSet<EmulationKey>();
            var deltaX = 0L; var deltaY = 0L; var wheelX = 0L; var wheelY = 0L;
            var mouseButtons = (GameInputMouseButtons)0;
            foreach (var entry in Devices.Values)
            {
                if ((entry.InputKinds & GameInputKind.Keyboard) != 0) ReadKeyboard(entry, keys);
                if ((entry.InputKinds & GameInputKind.Mouse) != 0)
                    ReadMouse(entry, ref deltaX, ref deltaY, ref wheelX, ref wheelY, ref mouseButtons);
            }
            var pointer = new EmulationPointerState(Clamp(deltaX), Clamp(deltaY), Clamp(wheelY),
                mouseButtons.HasFlag(GameInputMouseButtons.Left),
                mouseButtons.HasFlag(GameInputMouseButtons.Right),
                mouseButtons.HasFlag(GameInputMouseButtons.Middle),
                mouseButtons.HasFlag(GameInputMouseButtons.Button4),
                mouseButtons.HasFlag(GameInputMouseButtons.Button5), Clamp(wheelX));
            var entries = Devices.Values.Where(entry => entry.IsController)
                .OrderBy(entry => entry.Id, StringComparer.Ordinal).ToArray();
            var descriptors = entries.Select(entry => entry.Descriptor).ToArray();
            var controllers = entries.Select(Read)
                .Concat(RawGameControllerFallback.ReadAll(descriptors))
                .Select(ControllerAnalogDeadZoneFunctions.ApplyConfigured).ToArray();
            return new GameInputPhysicalState(keys, pointer, controllers);
        }
    });

    private static unsafe void ReadKeyboard(DeviceEntry entry, HashSet<EmulationKey> keys)
    {
        if (_gameInput is null) return;
        IGameInputReading? reading = null;
        try
        {
            if (_gameInput.GetCurrentReading(GameInputKind.Keyboard, entry.DevicePointer, out reading) < 0 || reading is null) return;
            var count = checked((int)reading.GetKeyCount());
            var states = new GameInputKeyState[count];
            fixed (GameInputKeyState* pointer = states)
                reading.GetKeyState((uint)count, (IntPtr)pointer);
            foreach (var state in states)
                if (EmulationKeyMapper.TryMap(KeyInterop.KeyFromVirtualKey(state.VirtualKey), out var key)) keys.Add(key);
        }
        catch (Exception exception) when (IsInteropFailure(exception)) { }
        finally { Release(reading); }
    }

    private static void ReadMouse(DeviceEntry entry, ref long deltaX, ref long deltaY,
        ref long wheelX, ref long wheelY, ref GameInputMouseButtons buttons)
    {
        if (_gameInput is null) return;
        IGameInputReading? reading = null;
        try
        {
            if (_gameInput.GetCurrentReading(GameInputKind.Mouse, entry.DevicePointer, out reading) < 0 ||
                reading is null || !reading.GetMouseState(out var state)) return;
            buttons |= state.Buttons;
            if (PreviousMouse.TryGetValue(entry.Id, out var previous))
            {
                deltaX += state.PositionX - previous.PositionX;
                deltaY += state.PositionY - previous.PositionY;
                wheelX += state.WheelX - previous.WheelX;
                wheelY += state.WheelY - previous.WheelY;
            }
            PreviousMouse[entry.Id] = state;
        }
        catch (Exception exception) when (IsInteropFailure(exception)) { }
        finally { Release(reading); }
    }

    private static int Clamp(long value) => (int)Math.Clamp(value, int.MinValue, int.MaxValue);

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        lock (Sync)
        {
            if (_initialized) return;
            _initialized = true;
            try
            {
                var result = GameInputNative.GameInputInitialize(GameInputNative.InterfaceId, out _gameInput);
                if (result < 0 || _gameInput is null)
                {
                    InitializationFailed = true;
                    return;
                }
                _gameInput.SetFocusPolicy(GameInputFocusPolicy.ExclusiveForegroundGuideButton);
                foreach (var filter in DeviceCallbackFilters)
                {
                    result = _gameInput.RegisterDeviceCallback(null, filter,
                        GameInputDeviceStatus.Any, GameInputEnumerationKind.Async, new IntPtr(unchecked((long)(uint)filter)),
                        Marshal.GetFunctionPointerForDelegate(DeviceCallback), out var deviceToken);
                    if (result < 0)
                        throw new COMException(
                            $"GameInput device enumeration failed for {filter}.", result);
                    DeviceTokens.Add(deviceToken);
                }
                result = _gameInput.RegisterSystemButtonCallback(null,
                    GameInputSystemButtons.Guide | GameInputSystemButtons.Share, IntPtr.Zero,
                    Marshal.GetFunctionPointerForDelegate(SystemButtonCallback), out _systemButtonToken);
                if (result < 0) throw new COMException("GameInput system button registration failed.", result);
                DrainDeviceChanges();
            }
            catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException
                or BadImageFormatException or COMException)
            {
                InitializationFailed = true;
                Shutdown();
            }
        }
    }

    private static void DeviceChanged(ulong token, IntPtr context, IGameInputDevice device,
        ulong timestamp, GameInputDeviceStatus currentStatus, GameInputDeviceStatus previousStatus)
    {
        // A GameInput callback must never register or unregister another callback.
        // Keep the RCW alive in the queued closure and do all COM lifecycle work on
        // our dedicated MTA worker after this native callback has returned.
        var lifetime = IntPtr.Zero;
        try
        {
            lifetime = Marshal.GetIUnknownForObject(device);
            PendingDeviceChanges.Enqueue(new PendingDeviceChange(
                token, context, device, lifetime, timestamp, currentStatus, previousStatus));
            lifetime = IntPtr.Zero;
            Worker.Post(DrainDeviceChanges);
        }
        catch (Exception exception)
        {
            if (lifetime != IntPtr.Zero) Marshal.Release(lifetime);
            LastCallbackDiagnostic = $"GameInput device callback queue failed: {exception.GetType().Name} 0x{exception.HResult:X8}";
        }
    }

    private static void DrainDeviceChanges()
    {
        while (PendingDeviceChanges.TryDequeue(out var change))
        {
            try
            {
                ProcessDeviceChange(
                    change.Token, change.Context, change.Device, change.Timestamp,
                    change.CurrentStatus, change.PreviousStatus);
            }
            finally
            {
                Marshal.Release(change.Lifetime);
            }
        }
    }

    private static void ProcessDeviceChange(ulong token, IntPtr context, IGameInputDevice device,
        ulong timestamp, GameInputDeviceStatus currentStatus, GameInputDeviceStatus previousStatus)
    {
        try
        {
            if (!TryDescribe(device, out var entry)) return;
            lock (Sync)
            {
                DeviceCallbackTraceLines.Add(
                    $"filter=0x{context.ToInt64():X8} token=0x{token:X16} " +
                    $"device={entry.Descriptor.VidPid} id={entry.Id} " +
                    $"current=0x{(uint)currentStatus:X8} previous=0x{(uint)previousStatus:X8}");
                if (DeviceCallbackTraceLines.Count > 256) DeviceCallbackTraceLines.RemoveAt(0);
                if ((currentStatus & GameInputDeviceStatus.Connected) != 0)
                {
                    entry = RegisterRawReading(entry);
                    if (Devices.Remove(entry.Id, out var previous)) DisposeEntry(previous);
                    Devices[entry.Id] = entry;
                    CaptureRawDevice(entry.Id, device, token, context, timestamp, currentStatus, previousStatus);
                }
                else
                {
                    DisposeEntry(entry);
                    if (Devices.Remove(entry.Id, out var previous)) DisposeEntry(previous);
                    EnumerationDiagnostics.Remove(entry.Id);
                    RawEnumerationDiagnostics.Remove(entry.Id);
                    SystemButtons.Remove(entry.Id);
                    PreviousMouse.Remove(entry.Id);
                    LatestRawReports.Remove(entry.Id);
                }
            }
        }
        catch (Exception exception)
        {
            LastCallbackDiagnostic = $"GameInput device callback failed: {exception.GetType().Name} 0x{exception.HResult:X8}";
        }
    }

    private static void SystemButtonsChanged(ulong token, IntPtr context, IGameInputDevice device,
        ulong timestamp, GameInputSystemButtons currentButtons, GameInputSystemButtons previousButtons)
    {
        try
        {
            if (!TryGetDeviceId(device, out var deviceId)) return;
            lock (Sync) SystemButtons[deviceId] = currentButtons;
        }
        catch (Exception exception)
        {
            LastCallbackDiagnostic = $"GameInput system-button callback failed: {exception.GetType().Name} 0x{exception.HResult:X8}";
        }
    }

    private static void RawReadingChanged(ulong token, IntPtr context, IGameInputReading reading)
    {
        try
        {
            if (context == IntPtr.Zero) return;
            var handle = GCHandle.FromIntPtr(context);
            if (handle.Target is not string deviceId) return;
            var bytes = ReadRawReport(reading);
            if (bytes.Count == 0) return;
            lock (Sync) LatestRawReports[deviceId] = bytes.ToArray();
        }
        catch (Exception exception)
        {
            LastDetailedReadDiagnostic = $"GameInput raw-reading callback failed: {exception.GetType().Name} 0x{exception.HResult:X8}";
        }
    }

    private static DeviceEntry RegisterRawReading(DeviceEntry entry)
    {
        if (_gameInput is null || (entry.InputKinds & GameInputKind.RawDeviceReport) == 0) return entry;
        var handle = GCHandle.Alloc(entry.Id);
        var context = GCHandle.ToIntPtr(handle);
        var result = _gameInput.RegisterReadingCallback(entry.Device, GameInputKind.RawDeviceReport,
            context, Marshal.GetFunctionPointerForDelegate(RawReadingCallback), out var token);
        if (result >= 0) return entry with { RawReadingToken = token, RawReadingContext = context };
        handle.Free();
        return entry;
    }

    private static bool TryGetDeviceId(IGameInputDevice device, out string deviceId)
    {
        deviceId = string.Empty;
        if (device.GetDeviceInfo(out var pointer) < 0 || pointer == IntPtr.Zero) return false;
        var info = Marshal.PtrToStructure<GameInputDeviceInfo>(pointer);
        deviceId = "gameinput:" + info.DeviceId.ToHex().ToLowerInvariant();
        return true;
    }

    private static void CaptureRawDevice(string id, IGameInputDevice device, ulong token,
        IntPtr context, ulong timestamp, GameInputDeviceStatus currentStatus,
        GameInputDeviceStatus previousStatus)
    {
        if (device.GetDeviceInfo(out var infoPointer) < 0 || infoPointer == IntPtr.Zero) return;
        var info = Marshal.PtrToStructure<GameInputDeviceInfo>(infoPointer);
        var chunks = new List<string>
        {
            $"callbackToken=0x{token:X16}",
            $"context=0x{context.ToInt64():X16}",
            $"deviceIUnknown=0x{GetIUnknownValue(device):X16}",
            $"timestamp=0x{timestamp:X16}",
            $"currentStatus=0x{(uint)currentStatus:X8}",
            $"previousStatus=0x{(uint)previousStatus:X8}",
            $"deviceInfoPointer=0x{infoPointer.ToInt64():X16}",
            $"deviceInfoBytes[256]={ReadHex(infoPointer, 256)}",
            $"displayNamePointer=0x{info.DisplayName.ToInt64():X16}",
            $"displayNameBytes={ReadNullTerminatedHex(info.DisplayName, 512)}",
            $"pnpPathPointer=0x{info.PnpPath.ToInt64():X16}",
            $"pnpPathBytes={ReadNullTerminatedHex(info.PnpPath, 2048)}",
            $"controllerInfoPointer=0x{info.ControllerInfo.ToInt64():X16}",
            $"controllerInfoBytes[48]={ReadHex(info.ControllerInfo, 48)}",
            $"gamepadInfoPointer=0x{info.GamepadInfo.ToInt64():X16}",
            $"gamepadInfoBytes[76]={ReadHex(info.GamepadInfo, 76)}"
        };
        if (info.ControllerInfo != IntPtr.Zero)
        {
            var controller = Marshal.PtrToStructure<GameInputControllerInfo>(info.ControllerInfo);
            chunks.Add($"axisLabelsPointer=0x{controller.AxisLabels.ToInt64():X16}");
            chunks.Add($"axisLabelsBytes[{controller.AxisCount * 4}]={ReadHex(controller.AxisLabels, checked((int)controller.AxisCount * 4))}");
            chunks.Add($"buttonLabelsPointer=0x{controller.ButtonLabels.ToInt64():X16}");
            chunks.Add($"buttonLabelsBytes[{controller.ButtonCount * 4}]={ReadHex(controller.ButtonLabels, checked((int)controller.ButtonCount * 4))}");
            chunks.Add($"switchInfoPointer=0x{controller.SwitchInfo.ToInt64():X16}");
        }
        RawEnumerationDiagnostics[id] = string.Join(Environment.NewLine, chunks);
    }

    private static long GetIUnknownValue(object value)
    {
        var pointer = Marshal.GetIUnknownForObject(value);
        try { return pointer.ToInt64(); }
        finally { Marshal.Release(pointer); }
    }

    private static string ReadHex(IntPtr pointer, int length)
    {
        if (pointer == IntPtr.Zero || length <= 0) return string.Empty;
        var bytes = new byte[length];
        Marshal.Copy(pointer, bytes, 0, length);
        return Convert.ToHexString(bytes);
    }

    private static string ReadNullTerminatedHex(IntPtr pointer, int maximum)
    {
        if (pointer == IntPtr.Zero) return string.Empty;
        var bytes = new List<byte>();
        for (var offset = 0; offset < maximum; offset++)
        {
            var value = Marshal.ReadByte(pointer, offset);
            bytes.Add(value);
            if (value == 0) break;
        }
        return Convert.ToHexString(CollectionsMarshal.AsSpan(bytes));
    }

    private static bool TryDescribe(IGameInputDevice device, out DeviceEntry entry)
    {
        entry = default!;
        HidReportDecoder? hidDecoder = null;
        try
        {
            var descriptor = GameInputDeviceInspector.Describe(device);
            if (descriptor is null) return false;
            IGameInputMapper? mapper = null;
            if (device.CreateInputMapper(out mapper) < 0) mapper = null;
            var isController = GameInputDeviceClassifier.IsGamingController(descriptor);

            if (isController &&
                descriptor.Controls.Count == 0 &&
                (descriptor.SupportedInput & GameInputKind.RawDeviceReport) != 0 &&
                HidReportDecoder.TryCreate(descriptor.PnpPath, out hidDecoder) &&
                hidDecoder is not null)
            {
                descriptor = descriptor with { Controls = hidDecoder.Controls };
            }

            if (isController && device.GetDeviceInfo(out var pointer) >= 0 && pointer != IntPtr.Zero)
            {
                var info = Marshal.PtrToStructure<GameInputDeviceInfo>(pointer);
                EnumerationDiagnostics[descriptor.Id] = DescribeDevice(
                    descriptor.ProductName,
                    descriptor.GameInputDisplayName,
                    string.Join(" || ", descriptor.WindowsIdentityChain),
                    descriptor.PnpPath,
                    info,
                    mapper);
            }

            entry = new DeviceEntry(
                descriptor.Id,
                descriptor.ProductName,
                descriptor.SupportedInput,
                device,
                Marshal.GetIUnknownForObject(device),
                mapper,
                isController,
                descriptor,
                hidDecoder);
            return true;
        }
        catch (Exception exception) when (IsInteropFailure(exception))
        {
            hidDecoder?.Dispose();
            return false;
        }
    }

    private static string DescribeDevice(string resolvedName, string? gameInputName,
        string windowsNames, string pnpPath, GameInputDeviceInfo info, IGameInputMapper? mapper)
    {
        var controller = info.ControllerInfo == IntPtr.Zero
            ? default
            : Marshal.PtrToStructure<GameInputControllerInfo>(info.ControllerInfo);
        var axisLabels = ReadInt32Array(controller.AxisLabels, controller.AxisCount);
        var buttonLabels = ReadInt32Array(controller.ButtonLabels, controller.ButtonCount);
        var mappings = new List<string>();
        if (mapper is not null)
        {
            foreach (var axis in Enum.GetValues<GameInputGamepadAxes>())
                if (axis != 0 && mapper.GetGamepadAxisMappingInfo(axis, out var mapping))
                    mappings.Add($"{axis}->{mapping.ControllerElementKind}[{mapping.ControllerIndex}],inv={mapping.IsInverted},two={mapping.FromTwoButtons},min={mapping.ButtonMinIndexValue},dir={mapping.ReferenceDirection}");
            foreach (var button in Enum.GetValues<GameInputGamepadButtons>())
                if (button != 0 && mapper.GetGamepadButtonMappingInfo(button, out var mapping))
                    mappings.Add($"{button}->{mapping.ControllerElementKind}[{mapping.ControllerIndex}],inv={mapping.IsInverted},pos={mapping.SwitchPosition}");
        }
        return string.Join(Environment.NewLine, new[]
        {
            $"ResolvedName: {resolvedName}",
            $"GameInputName: {gameInputName}",
            $"VID:PID: {info.VendorId:X4}:{info.ProductId:X4}",
            $"DeviceId: {info.DeviceId.ToHex()}",
            $"DeviceRootId: {info.DeviceRootId.ToHex()}",
            $"ContainerId: {info.ContainerId}",
            $"Family: {info.DeviceFamily}",
            $"Usage: {info.Usage.Page:X4}:{info.Usage.Id:X4}",
            $"SupportedInput: {info.SupportedInput} (0x{(int)info.SupportedInput:X8})",
            $"ControllerInfo: axes={controller.AxisCount}, buttons={controller.ButtonCount}, switches={controller.SwitchCount}",
            $"AxisLabels: [{string.Join(", ", axisLabels)}]",
            $"ButtonLabels: [{string.Join(", ", buttonLabels)}]",
            $"Mapper: {(mappings.Count == 0 ? "<aucune correspondance>" : string.Join(" | ", mappings))}",
            $"PnP: {pnpPath}",
            $"PnP names: {windowsNames}"
        });
    }

    private static int[] ReadInt32Array(IntPtr pointer, uint count)
    {
        if (pointer == IntPtr.Zero || count == 0 || count > 1024) return Array.Empty<int>();
        var values = new int[count];
        Marshal.Copy(pointer, values, 0, checked((int)count));
        return values;
    }

    private static EmulationControllerState Read(DeviceEntry entry)
    {
        if (_gameInput is null) return EmulationControllerState.Empty with { DeviceId = entry.Id };
        IGameInputReading? reading = null;
        try
        {
            var kind = PreferredReadingKind(entry.InputKinds);
            var result = _gameInput.GetCurrentReading(kind, entry.DevicePointer, out reading);
            if (result < 0 || reading is null)
            {
                LastReadDiagnostic = $"{entry.Id}: GetCurrentReading({kind})=0x{result:X8}";
                return EmulationControllerState.Empty with { DeviceId = entry.Id };
            }
            var readingKind = reading.GetInputKind();
            var gamepad = default(GameInputGamepadState);
            var hasGamepad = kind == GameInputKind.Gamepad && reading.GetGamepadState(out gamepad);
            var hasControllerArrays = kind != GameInputKind.RawDeviceReport &&
                (entry.InputKinds & GameInputKind.Controller) != 0;
            if (!hasControllerArrays)
            {
                LastReadDiagnostic = $"{entry.Id}: kind={readingKind}, gamepad={hasGamepad}, controller arrays unavailable";
                return hasGamepad
                    ? MapGamepad(entry.Id, gamepad)
                    : EmulationControllerState.Empty with { DeviceId = entry.Id };
            }
            LastReadDiagnostic = $"{entry.Id}: kind={readingKind}, gamepad={hasGamepad}, axes={reading.GetControllerAxisCount()}, buttons={reading.GetControllerButtonCount()}, switches={reading.GetControllerSwitchCount()}";
            return hasGamepad ? MapGamepad(entry.Id, gamepad) : MapController(entry.Id, reading, entry.Mapper);
        }
        catch (Exception exception) when (IsInteropFailure(exception))
        {
            LastReadDiagnostic = $"{entry.Id}: COM 0x{exception.HResult:X8} ({exception.GetType().Name})";
            return EmulationControllerState.Empty with { DeviceId = entry.Id };
        }
        finally { Release(reading); }
    }

    private static unsafe GameInputLiveState ReadDetailed(DeviceEntry entry)
    {
        if (_gameInput is null) return GameInputLiveState.Empty(entry.Id);
        IGameInputReading? reading = null;
        try
        {
            var kind = PreferredReadingKind(entry.InputKinds);
            var readingResult = _gameInput.GetCurrentReading(kind, entry.DevicePointer, out reading);
            if (readingResult < 0 || reading is null)
            {
                LastDetailedReadDiagnostic = $"{entry.Id}: GetCurrentReading({kind})=0x{readingResult:X8}";
                if (entry.HidDecoder is null) return GameInputLiveState.Empty(entry.Id);
                var latest = LatestRawReports.GetValueOrDefault(entry.Id) ?? [];
                return new GameInputLiveState(
                    entry.Id,
                    0,
                    GameInputKind.RawDeviceReport,
                    latest.Length == 0
                        ? entry.HidDecoder.NeutralControls()
                        : entry.HidDecoder.Decode(latest),
                    latest,
                    SystemButtons.GetValueOrDefault(entry.Id),
                    null,
                    null,
                    null,
                    null,
                    true);
            }
            var readingKind = reading.GetInputKind();
            LastDetailedReadDiagnostic = $"{entry.Id}: reading={readingKind}";

            var axes = Array.Empty<float>();
            var buttons = Array.Empty<byte>();
            var switches = Array.Empty<int>();
            if (kind != GameInputKind.RawDeviceReport &&
                (entry.InputKinds & GameInputKind.Controller) != 0)
            {
                var axisCount = checked((int)reading.GetControllerAxisCount());
                axes = new float[axisCount];
                fixed (float* pointer = axes)
                    reading.GetControllerAxisState((uint)axisCount, (IntPtr)pointer);

                var buttonCount = checked((int)reading.GetControllerButtonCount());
                buttons = new byte[buttonCount];
                fixed (byte* pointer = buttons)
                    reading.GetControllerButtonState((uint)buttonCount, (IntPtr)pointer);

                var switchCount = checked((int)reading.GetControllerSwitchCount());
                switches = new int[switchCount];
                fixed (int* pointer = switches)
                    reading.GetControllerSwitchState((uint)switchCount, (IntPtr)pointer);
            }

            var labels = entry.Descriptor.Controls.ToDictionary(
                control => (control.Type, control.Index),
                control => control.Label);
            var controls = new List<GameInputControlValue>(axes.Length + buttons.Length + switches.Length);
            for (var index = 0; index < axes.Length; index++)
                controls.Add(new GameInputControlValue(
                    GameInputControlType.Axis,
                    index,
                    labels.GetValueOrDefault((GameInputControlType.Axis, index), GameInputLabel.None),
                    axes[index]));
            for (var index = 0; index < buttons.Length; index++)
                controls.Add(new GameInputControlValue(
                    GameInputControlType.Button,
                    index,
                    labels.GetValueOrDefault((GameInputControlType.Button, index), GameInputLabel.None),
                    buttons[index] == 0 ? 0f : 1f));
            for (var index = 0; index < switches.Length; index++)
            {
                var position = (GameInputSwitchPosition)switches[index];
                controls.Add(new GameInputControlValue(
                    GameInputControlType.Switch,
                    index,
                    labels.GetValueOrDefault((GameInputControlType.Switch, index), GameInputLabel.None),
                    switches[index],
                    position));
            }

            var rawBytes = ReadRawReport(reading);
            if (rawBytes.Count == 0 && LatestRawReports.TryGetValue(entry.Id, out var latestRaw))
                rawBytes = latestRaw;
            if (entry.HidDecoder is not null)
                controls = entry.HidDecoder.Decode(rawBytes).ToList();
            GameInputArcadeStickState? arcade = null;
            if (entry.Descriptor.StandardCapabilities.HasArcadeStick &&
                reading.GetArcadeStickState(out var arcadeValue)) arcade = arcadeValue;
            GameInputFlightStickState? flight = null;
            if (entry.Descriptor.StandardCapabilities.HasFlightStick &&
                reading.GetFlightStickState(out var flightValue)) flight = flightValue;
            GameInputGamepadState? gamepad = null;
            if (entry.Descriptor.StandardCapabilities.HasGamepad &&
                reading.GetGamepadState(out var gamepadValue)) gamepad = gamepadValue;
            GameInputRacingWheelState? wheel = null;
            if (entry.Descriptor.StandardCapabilities.HasRacingWheel &&
                reading.GetRacingWheelState(out var wheelValue)) wheel = wheelValue;

            return new GameInputLiveState(
                entry.Id,
                reading.GetTimestamp(),
                reading.GetInputKind(),
                controls,
                rawBytes,
                SystemButtons.GetValueOrDefault(entry.Id),
                arcade,
                flight,
                gamepad,
                wheel,
                entry.HidDecoder is not null);
        }
        catch (Exception exception) when (IsInteropFailure(exception))
        {
            LastDetailedReadDiagnostic = $"{entry.Id}: COM 0x{exception.HResult:X8} ({exception.GetType().Name})";
            return GameInputLiveState.Empty(entry.Id);
        }
        finally
        {
            Release(reading);
        }
    }

    private static unsafe IReadOnlyList<byte> ReadRawReport(IGameInputReading reading)
    {
        IGameInputRawDeviceReport? report = null;
        try
        {
            reading.GetRawReport(out report);
            if (report is null)
            {
                LastDetailedReadDiagnostic += "; rawReport=null";
                return [];
            }
            var size = report.GetRawDataSize();
            if (size == 0 || size > 65536)
            {
                LastDetailedReadDiagnostic += $"; rawSize={size}";
                return [];
            }
            var bytes = new byte[checked((int)size)];
            fixed (byte* pointer = bytes)
            {
                var copied = report.GetRawData(size, (IntPtr)pointer);
                LastDetailedReadDiagnostic += $"; rawSize={size}; copied={copied}";
                return copied == 0 ? [] : bytes[..checked((int)Math.Min(copied, size))];
            }
        }
        catch (Exception exception) when (IsInteropFailure(exception))
        {
            return [];
        }
        finally
        {
            Release(report);
        }
    }

    private static GameInputKind PreferredReadingKind(GameInputKind kinds)
    {
        if ((kinds & GameInputKind.Gamepad) != 0) return GameInputKind.Gamepad;
        if ((kinds & GameInputKind.RacingWheel) != 0) return GameInputKind.RacingWheel;
        if ((kinds & GameInputKind.FlightStick) != 0) return GameInputKind.FlightStick;
        if ((kinds & GameInputKind.ArcadeStick) != 0) return GameInputKind.ArcadeStick;
        if ((kinds & GameInputKind.Controller) != 0) return GameInputKind.Controller;
        return GameInputKind.RawDeviceReport;
    }

    internal static EmulationControllerState MapGamepad(string deviceId, GameInputGamepadState gamepad)
    {
        uint buttons = 0;
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.B, 0);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.Y, 1);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.View, 2);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.Menu, 3);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadUp, 4);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadDown, 5);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadLeft, 6);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.DPadRight, 7);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.A, 8);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.X, 9);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.LeftShoulder, 10);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.RightShoulder, 11);
        if (gamepad.LeftTrigger > .12f || gamepad.Buttons.HasFlag(GameInputGamepadButtons.LeftTriggerButton)) buttons |= 1u << 12;
        if (gamepad.RightTrigger > .12f || gamepad.Buttons.HasFlag(GameInputGamepadButtons.RightTriggerButton)) buttons |= 1u << 13;
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.LeftThumbstick, 14);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.RightThumbstick, 15);
        var system = SystemButtons.GetValueOrDefault(deviceId);
        if (system.HasFlag(GameInputSystemButtons.Guide)) buttons |= 1u << 16;
        if (system.HasFlag(GameInputSystemButtons.Share)) buttons |= 1u << 17;
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleLeft1, 18);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleLeft2, 19);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleRight1, 20);
        buttons = Set(buttons, gamepad.Buttons, GameInputGamepadButtons.PaddleRight2, 21);
        return new EmulationControllerState(buttons,
            Axis(gamepad.LeftThumbstickX), Axis(-gamepad.LeftThumbstickY),
            Axis(gamepad.RightThumbstickX), Axis(-gamepad.RightThumbstickY),
            Trigger(gamepad.LeftTrigger), Trigger(gamepad.RightTrigger)) { DeviceId = deviceId };
    }

    private static unsafe EmulationControllerState MapController(
        string deviceId, IGameInputReading reading, IGameInputMapper? mapper)
    {
        var buttonCount = checked((int)reading.GetControllerButtonCount());
        var buttonStates = new byte[buttonCount];
        fixed (byte* pointer = buttonStates)
            reading.GetControllerButtonState((uint)buttonCount, (IntPtr)pointer);
        var axisCount = checked((int)reading.GetControllerAxisCount());
        var axisStates = new float[axisCount];
        fixed (float* pointer = axisStates)
            reading.GetControllerAxisState((uint)axisCount, (IntPtr)pointer);
        var switchCount = checked((int)reading.GetControllerSwitchCount());
        var switchStates = new int[switchCount];
        fixed (int* pointer = switchStates)
            reading.GetControllerSwitchState((uint)switchCount, (IntPtr)pointer);

        var controls = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < buttonStates.Length; index++)
            controls[$"Button{index}"] = buttonStates[index] != 0 ? 1f : 0f;
        for (var index = 0; index < axisStates.Length; index++) controls[$"Axis{index}"] = axisStates[index];
        for (var index = 0; index < switchStates.Length; index++) controls[$"Switch{index}"] = switchStates[index];

        if (mapper is null)
            return new EmulationControllerState(0, 0, 0, 0, 0, 0, 0)
                { DeviceId = deviceId, Controls = new EmulationControllerControls(controls) };

        var mapped = new GameInputGamepadState
        {
            LeftTrigger = ReadMappedAxis(mapper, GameInputGamepadAxes.LeftTrigger, axisStates, buttonStates, switchStates, 0f),
            RightTrigger = ReadMappedAxis(mapper, GameInputGamepadAxes.RightTrigger, axisStates, buttonStates, switchStates, 0f),
            LeftThumbstickX = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.LeftThumbstickX, axisStates, buttonStates, switchStates, .5f)),
            LeftThumbstickY = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.LeftThumbstickY, axisStates, buttonStates, switchStates, .5f)),
            RightThumbstickX = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.RightThumbstickX, axisStates, buttonStates, switchStates, .5f)),
            RightThumbstickY = ToThumbAxis(ReadMappedAxis(mapper, GameInputGamepadAxes.RightThumbstickY, axisStates, buttonStates, switchStates, .5f))
        };
        foreach (var button in Enum.GetValues<GameInputGamepadButtons>())
            if (button != 0 &&
                ReadMappedButton(mapper, button, axisStates, buttonStates, switchStates))
                mapped.Buttons |= button;
        return MapGamepad(deviceId, mapped) with { Controls = new EmulationControllerControls(controls) };
    }

    private static float ReadMappedAxis(IGameInputMapper mapper, GameInputGamepadAxes axis,
        float[] axes, byte[] buttons, int[] switches, float unmappedValue)
    {
        if (!mapper.GetGamepadAxisMappingInfo(axis, out var mapping)) return unmappedValue;
        var value = mapping.ControllerElementKind switch
        {
            GameInputElementKind.Axis when mapping.ControllerIndex < axes.Length => axes[mapping.ControllerIndex],
            GameInputElementKind.Button when mapping.ControllerIndex < buttons.Length =>
                ReadButtonAxis(mapping, buttons),
            GameInputElementKind.Switch when mapping.ControllerIndex < switches.Length =>
                ReadSwitchAxis(switches[mapping.ControllerIndex], (int)mapping.ReferenceDirection),
            _ => 0f
        };
        return mapping.IsInverted ? 1f - value : value;
    }

    private static float ReadButtonAxis(GameInputAxisMapping mapping, byte[] buttons)
    {
        var maximum = buttons[mapping.ControllerIndex] != 0;
        if (!mapping.FromTwoButtons || mapping.ButtonMinIndexValue >= buttons.Length)
            return maximum ? 1f : 0f;
        var minimum = buttons[mapping.ButtonMinIndexValue] != 0;
        return maximum == minimum ? .5f : maximum ? 1f : 0f;
    }

    private static float ReadSwitchAxis(int position, int referenceDirection)
    {
        if (position == 0) return .5f;
        if (position == referenceDirection) return 1f;
        var opposite = ((referenceDirection - 1 + 4) % 8) + 1;
        return position == opposite ? 0f : .5f;
    }

    private static bool ReadMappedButton(IGameInputMapper mapper, GameInputGamepadButtons button,
        float[] axes, byte[] buttons, int[] switches)
    {
        if (!mapper.GetGamepadButtonMappingInfo(button, out var mapping)) return false;
        return mapping.ControllerElementKind switch
        {
            GameInputElementKind.Button when mapping.ControllerIndex < buttons.Length =>
                buttons[mapping.ControllerIndex] != 0,
            GameInputElementKind.Axis when mapping.ControllerIndex < axes.Length =>
                mapping.IsInverted ? axes[mapping.ControllerIndex] < .5f : axes[mapping.ControllerIndex] > .5f,
            GameInputElementKind.Switch when mapping.ControllerIndex < switches.Length =>
                switches[mapping.ControllerIndex] == (int)mapping.SwitchPosition,
            _ => false
        };
    }

    private static float ToThumbAxis(float value) => Math.Clamp(value * 2f - 1f, -1f, 1f);

    private static uint Set(uint result, GameInputGamepadButtons buttons,
        GameInputGamepadButtons source, int target) =>
        (buttons & source) == 0 ? result : result | 1u << target;
    private static short Trigger(float value) => Axis(Math.Clamp(value, 0f, 1f));
    private static short Axis(float value) =>
        (short)Math.Round(Math.Clamp(value, -1f, 1f) * short.MaxValue);

    private static bool Shutdown()
    {
        if (_gameInput is not null)
        {
            try
            {
                if (_systemButtonToken != 0) _gameInput.StopCallback(_systemButtonToken);
                foreach (var token in DeviceTokens)
                    if (token != 0) _gameInput.StopCallback(token);
                foreach (var entry in Devices.Values)
                    if (entry.RawReadingToken != 0) _gameInput.StopCallback(entry.RawReadingToken);
            }
            catch (Exception exception) when (IsInteropFailure(exception)) { }

            if (!SafeUnregister(_systemButtonToken)) return false;
            _systemButtonToken = 0;
            foreach (var token in DeviceTokens)
                if (!SafeUnregister(token)) return false;
            DeviceTokens.Clear();
        }

        foreach (var entry in Devices.Values.ToArray())
        {
            if (!DisposeEntry(entry)) return false;
            Devices.Remove(entry.Id);
        }

        Devices.Clear();
        EnumerationDiagnostics.Clear();
        RawEnumerationDiagnostics.Clear();
        DeviceCallbackTraceLines.Clear();
        SystemButtons.Clear();
        PreviousMouse.Clear();
        LatestRawReports.Clear();
        while (PendingDeviceChanges.TryDequeue(out var pending))
            Marshal.Release(pending.Lifetime);
        Release(_gameInput);
        _gameInput = null;
        return true;
    }

    private static bool DisposeEntry(DeviceEntry entry)
    {
        if (_gameInput is not null && entry.RawReadingToken != 0)
        {
            try { _gameInput.StopCallback(entry.RawReadingToken); }
            catch (Exception exception) when (IsInteropFailure(exception)) { }
            if (!SafeUnregister(entry.RawReadingToken)) return false;
        }
        if (entry.RawReadingContext != IntPtr.Zero)
        {
            var handle = GCHandle.FromIntPtr(entry.RawReadingContext);
            if (handle.IsAllocated) handle.Free();
        }
        if (entry.DevicePointer != IntPtr.Zero) Marshal.Release(entry.DevicePointer);
        Release(entry.Mapper);
        entry.HidDecoder?.Dispose();
        return true;
    }

    private static bool SafeUnregister(ulong token)
    {
        if (_gameInput is null || token == 0) return true;
        try { return _gameInput.UnregisterCallback(token); }
        catch (Exception exception) when (IsInteropFailure(exception)) { return false; }
    }

    private static bool IsInteropFailure(Exception exception) => exception is
        COMException or InvalidComObjectException or InvalidCastException or InvalidOperationException or
        ArgumentException or OverflowException;

    private static void Release(object? value)
    {
        if (value is not null && Marshal.IsComObject(value)) Marshal.ReleaseComObject(value);
    }

    private sealed class GameInputWorker
    {
        private readonly BlockingCollection<Action> _queue = new();
        private readonly int _threadId;

        internal GameInputWorker()
        {
            using var ready = new ManualResetEventSlim();
            var threadId = 0;
            var thread = new Thread(() =>
            {
                threadId = Environment.CurrentManagedThreadId;
                ready.Set();
                foreach (var action in _queue.GetConsumingEnumerable()) action();
            })
            {
                IsBackground = true,
                Name = "GWGUI GameInput"
            };
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            ready.Wait();
            _threadId = threadId;
        }

        internal void Post(Action action) => _queue.Add(action);

        internal void Invoke(Action action) => Invoke(() =>
        {
            action();
            return true;
        });

        internal T Invoke<T>(Func<T> action)
        {
            if (Environment.CurrentManagedThreadId == _threadId) return action();
            using var completed = new ManualResetEventSlim();
            T? result = default;
            Exception? failure = null;
            _queue.Add(() =>
            {
                try { result = action(); }
                catch (Exception exception) { failure = exception; }
                finally { completed.Set(); }
            });
            completed.Wait();
            if (failure is not null) ExceptionDispatchInfo.Capture(failure).Throw();
            return result!;
        }
    }

    private sealed record PendingDeviceChange(
        ulong Token,
        IntPtr Context,
        IGameInputDevice Device,
        IntPtr Lifetime,
        ulong Timestamp,
        GameInputDeviceStatus CurrentStatus,
        GameInputDeviceStatus PreviousStatus);

    private sealed record DeviceEntry(
        string Id,
        string Name,
        GameInputKind InputKinds,
        IGameInputDevice Device,
        IntPtr DevicePointer,
        IGameInputMapper? Mapper,
        bool IsController,
        GameInputDeviceDescriptor Descriptor,
        HidReportDecoder? HidDecoder = null,
        ulong RawReadingToken = 0,
        IntPtr RawReadingContext = default);
}
