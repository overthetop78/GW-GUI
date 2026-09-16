using GWGUI.Emulation;
using GWGUI.App.Constants.Input.GameInput;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace GWGUI.App.Services.Input.GameInput;

internal static partial class GameInputDeviceMonitor
{
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan CallbackUnregisterTimeout = TimeSpan.FromSeconds(3);
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
    private static readonly Dictionary<string, GameInputDeviceEntry> Devices = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentQueue<PendingGameInputDeviceChange> PendingDeviceChanges = new();
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
    internal static string LastReadDiagnostic => GameInputDiagnostics.LastRead;
    internal static string LastDetailedReadDiagnostic => GameInputDiagnostics.LastDetailedRead;
    internal static string LastCallbackDiagnostic => GameInputDiagnostics.LastCallback;
    internal static string LastEnumerationDiagnostic => GameInputDiagnostics.Enumeration;
    internal static string RawEnumerationDiagnostic => GameInputDiagnostics.RawEnumeration;
    internal static string DeviceCallbackTrace => GameInputDiagnostics.DeviceCallbackTrace;

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        var shutdownRequired = false;
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
                }
                else
                {
                    _gameInput.SetFocusPolicy(GameInputFocusPolicy.ExclusiveForegroundGuideButton);
                    foreach (var filter in DeviceCallbackFilters)
                    {
                        result = _gameInput.RegisterDeviceCallback(null, filter,
                            GameInputDeviceStatus.Any, GameInputEnumerationKind.Async, new IntPtr(unchecked((long)(uint)filter)),
                            Marshal.GetFunctionPointerForDelegate(DeviceCallback), out var deviceToken);
                        if (result < 0)
                            throw new COMException(
                                GameInputDiagnostics.DeviceEnumerationFailure(filter), result);
                        DeviceTokens.Add(deviceToken);
                    }
                    result = _gameInput.RegisterSystemButtonCallback(null,
                        GameInputSystemButtons.Guide | GameInputSystemButtons.Share, IntPtr.Zero,
                        Marshal.GetFunctionPointerForDelegate(SystemButtonCallback), out _systemButtonToken);
                    if (result < 0)
                        throw new COMException(
                            GameInputDiagnostics.SystemButtonRegistrationFailure, result);
                    DrainDeviceChanges();
                }
            }
            catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException
                or BadImageFormatException or COMException)
            {
                InitializationFailed = true;
                shutdownRequired = true;
            }
        }
        if (shutdownRequired) Shutdown();
        if (InitializationFailed) RawGameControllerFallback.StartMonitoring();
        else RawGameControllerFallback.StopMonitoring();
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
            PendingDeviceChanges.Enqueue(new PendingGameInputDeviceChange(
                token, context, device, lifetime, timestamp, currentStatus, previousStatus));
            lifetime = IntPtr.Zero;
            Worker.Post(DrainDeviceChanges);
        }
        catch (Exception exception)
        {
            if (lifetime != IntPtr.Zero) Marshal.Release(lifetime);
            GameInputDiagnostics.RecordDeviceCallbackQueueFailure(exception);
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
                GameInputDiagnostics.RecordDeviceCallback(
                    context, token, entry, currentStatus, previousStatus);
                if ((currentStatus & GameInputDeviceStatus.Connected) != 0)
                {
                    entry = RegisterRawReading(entry);
                    if (Devices.Remove(entry.Id, out var previous)) DisposeEntry(previous);
                    Devices[entry.Id] = entry;
                    GameInputDiagnostics.CaptureRawDevice(
                        entry.Id, device, token, context, timestamp, currentStatus, previousStatus);
                }
                else
                {
                    DisposeEntry(entry);
                    if (Devices.Remove(entry.Id, out var previous)) DisposeEntry(previous);
                    GameInputDiagnostics.RemoveDevice(entry.Id);
                    SystemButtons.Remove(entry.Id);
                    PreviousMouse.Remove(entry.Id);
                    LatestRawReports.Remove(entry.Id);
                }
            }
        }
        catch (Exception exception)
        {
            GameInputDiagnostics.RecordDeviceCallbackFailure(exception);
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
            GameInputDiagnostics.RecordSystemButtonCallbackFailure(exception);
        }
    }

    private static void RawReadingChanged(ulong token, IntPtr context, IGameInputReading reading)
    {
        try
        {
            if (context == IntPtr.Zero) return;
            var handle = GCHandle.FromIntPtr(context);
            if (handle.Target is not string deviceId) return;
            var bytes = GameInputControllerStateReader.ReadRawReport(
                reading, IsInteropFailure, Release);
            if (bytes.Count == 0) return;
            lock (Sync) LatestRawReports[deviceId] = bytes.ToArray();
        }
        catch (Exception exception)
        {
            GameInputDiagnostics.RecordRawReadingCallbackFailure(exception);
        }
    }

    private static GameInputDeviceEntry RegisterRawReading(GameInputDeviceEntry entry)
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
        deviceId = GameInputConstants.DeviceIdPrefix + info.DeviceId.ToHex().ToLowerInvariant();
        return true;
    }

    private static bool TryDescribe(IGameInputDevice device, out GameInputDeviceEntry entry)
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
                GameInputDiagnostics.RecordEnumeration(descriptor, info, mapper);
            }

            entry = new GameInputDeviceEntry(
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

    private static EmulationControllerState Read(GameInputDeviceEntry entry) =>
        GameInputControllerStateReader.Read(
            _gameInput, entry, SystemButtons, IsInteropFailure, Release);

    private static GameInputLiveState ReadDetailed(GameInputDeviceEntry entry) =>
        GameInputControllerStateReader.ReadDetailed(
            _gameInput, entry, LatestRawReports, SystemButtons, IsInteropFailure, Release);

    internal static EmulationControllerState MapGamepad(string deviceId, GameInputGamepadState gamepad) =>
        GameInputControllerStateMapper.MapGamepad(deviceId, gamepad, SystemButtons);


    private static bool Shutdown()
    {
        IGameInput? gameInput;
        ulong systemButtonToken;
        ulong[] deviceTokens;
        GameInputDeviceEntry[] entries;
        lock (Sync)
        {
            gameInput = _gameInput;
            systemButtonToken = _systemButtonToken;
            deviceTokens = DeviceTokens.ToArray();
            entries = Devices.Values.ToArray();
        }

        if (gameInput is not null)
        {
            var callbackTokens = deviceTokens
                .Append(systemButtonToken)
                .Concat(entries.Select(entry => entry.RawReadingToken))
                .Where(token => token != 0)
                .Distinct()
                .ToArray();
            // Never wait for a callback while holding Sync: the system-button and
            // raw-reading callbacks both acquire it before returning.
            var pendingTokens = callbackTokens.ToHashSet();
            var unregisterDeadline = DateTime.UtcNow + CallbackUnregisterTimeout;
            while (pendingTokens.Count > 0 && DateTime.UtcNow < unregisterDeadline)
            {
                foreach (var token in pendingTokens.ToArray())
                {
                    if (SafeUnregister(token)) pendingTokens.Remove(token);
                }
                if (pendingTokens.Count > 0)
                    Thread.Sleep(GameInputConstants.CallbackUnregisterRetryInterval);
            }
            if (pendingTokens.Count > 0) return false;
        }

        lock (Sync)
        {
            _systemButtonToken = 0;
            DeviceTokens.Clear();
            foreach (var entry in entries) GameInputResourceReleaser.ReleaseEntryResources(entry);

            Devices.Clear();
            GameInputDiagnostics.Clear();
            SystemButtons.Clear();
            PreviousMouse.Clear();
            LatestRawReports.Clear();
            while (PendingDeviceChanges.TryDequeue(out var pending))
                Marshal.Release(pending.Lifetime);
            GameInputResourceReleaser.FinalRelease(_gameInput);
            _gameInput = null;
        }
        return true;
    }

    private static bool DisposeEntry(GameInputDeviceEntry entry)
    {
        if (_gameInput is not null && entry.RawReadingToken != 0)
        {
            if (!SafeUnregister(entry.RawReadingToken)) return false;
        }
        GameInputResourceReleaser.ReleaseEntryResources(entry);
        return true;
    }

    private static bool SafeUnregister(ulong token)
        => GameInputResourceReleaser.SafeUnregister(_gameInput, token);

    private static bool IsInteropFailure(Exception exception) =>
        GameInputResourceReleaser.IsInteropFailure(exception);

    private static void Release(object? value) => GameInputResourceReleaser.Release(value);

}
