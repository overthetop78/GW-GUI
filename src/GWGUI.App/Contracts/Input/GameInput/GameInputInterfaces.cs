using System.Runtime.InteropServices;

namespace GWGUI.App.Services.Input.GameInput;

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate void GameInputReadingCallback(ulong token, IntPtr context, IGameInputReading reading);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate void GameInputDeviceCallback(
    ulong token,
    IntPtr context,
    IGameInputDevice device,
    ulong timestamp,
    GameInputDeviceStatus currentStatus,
    GameInputDeviceStatus previousStatus);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate void GameInputSystemButtonCallback(
    ulong token,
    IntPtr context,
    IGameInputDevice device,
    ulong timestamp,
    GameInputSystemButtons currentButtons,
    GameInputSystemButtons previousButtons);

[UnmanagedFunctionPointer(CallingConvention.StdCall)]
internal delegate void GameInputKeyboardLayoutCallback(
    ulong token,
    IntPtr context,
    IGameInputDevice device,
    ulong timestamp,
    uint currentLayout,
    uint previousLayout);

[ComImport, Guid("20EFC1C7-5D9A-43BA-B26F-B807FA48609C"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInput
{
    [PreserveSig] ulong GetCurrentTimestamp();
    [PreserveSig] int GetCurrentReading(GameInputKind inputKind, IntPtr device, out IGameInputReading? reading);
    [PreserveSig] int GetNextReading(IGameInputReading referenceReading, GameInputKind inputKind, IGameInputDevice? device, out IGameInputReading? reading);
    [PreserveSig] int GetPreviousReading(IGameInputReading referenceReading, GameInputKind inputKind, IGameInputDevice? device, out IGameInputReading? reading);
    [PreserveSig] int RegisterReadingCallback(IGameInputDevice? device, GameInputKind inputKind, IntPtr context, IntPtr callback, out ulong token);
    [PreserveSig] int RegisterDeviceCallback(IGameInputDevice? device, GameInputKind inputKind, GameInputDeviceStatus statusFilter, GameInputEnumerationKind enumerationKind, IntPtr context, IntPtr callback, out ulong token);
    [PreserveSig] int RegisterSystemButtonCallback(IGameInputDevice? device, GameInputSystemButtons filter, IntPtr context, IntPtr callback, out ulong token);
    [PreserveSig] int RegisterKeyboardLayoutCallback(IGameInputDevice? device, IntPtr context, IntPtr callback, out ulong token);
    [PreserveSig] void StopCallback(ulong token);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool UnregisterCallback(ulong token);
    [PreserveSig] int CreateDispatcher(out IGameInputDispatcher? dispatcher);
    [PreserveSig] int FindDeviceFromId(in AppLocalDeviceId value, out IGameInputDevice? device);
    [PreserveSig] int FindDeviceFromPlatformString([MarshalAs(UnmanagedType.LPWStr)] string value, out IGameInputDevice? device);
    [PreserveSig] void SetFocusPolicy(GameInputFocusPolicy policy);
    [PreserveSig] int CreateAggregateDevice(GameInputKind inputKind, out AppLocalDeviceId deviceId);
    [PreserveSig] int DisableAggregateDevice(in AppLocalDeviceId deviceId);
}

[ComImport, Guid("05A42D89-2CB6-45A3-874D-E635723587AB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInputRawDeviceReport
{
    [PreserveSig] void GetDevice(out IGameInputDevice device);
    [PreserveSig] void GetReportInfo(out GameInputRawDeviceReportInfo reportInfo);
    [PreserveSig] nuint GetRawDataSize();
    [PreserveSig] nuint GetRawData(nuint bufferSize, IntPtr buffer);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool SetRawData(nuint bufferSize, IntPtr buffer);
}

[ComImport, Guid("C81C4CDE-ED1A-4631-A30F-C556A6241A1F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInputReading
{
    [PreserveSig] GameInputKind GetInputKind();
    [PreserveSig] ulong GetTimestamp();
    [PreserveSig] void GetDevice(out IGameInputDevice device);
    [PreserveSig] uint GetControllerAxisCount();
    [PreserveSig] uint GetControllerAxisState(uint count, IntPtr states);
    [PreserveSig] uint GetControllerButtonCount();
    [PreserveSig] uint GetControllerButtonState(uint count, IntPtr states);
    [PreserveSig] uint GetControllerSwitchCount();
    [PreserveSig] uint GetControllerSwitchState(uint count, IntPtr states);
    [PreserveSig] uint GetKeyCount();
    [PreserveSig] uint GetKeyState(uint count, IntPtr states);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetMouseState(out GameInputMouseState state);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetSensorsState(out GameInputSensorsState state);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetArcadeStickState(out GameInputArcadeStickState state);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetFlightStickState(out GameInputFlightStickState state);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetGamepadState(out GameInputGamepadState state);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetRacingWheelState(out GameInputRacingWheelState state);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetRawReport(out IGameInputRawDeviceReport? report);
}

[ComImport, Guid("63E2F38B-A399-4275-8AE7-D4C6E524D12A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInputDevice
{
    [PreserveSig] int GetDeviceInfo(out IntPtr info);
    [PreserveSig] int GetHapticInfo(out GameInputHapticInfo info);
    [PreserveSig] GameInputDeviceStatus GetDeviceStatus();
    [PreserveSig] int CreateForceFeedbackEffect(uint motorIndex, in GameInputForceFeedbackParams parameters, out IGameInputForceFeedbackEffect? effect);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool IsForceFeedbackMotorPoweredOn(uint motorIndex);
    [PreserveSig] void SetForceFeedbackMotorGain(uint motorIndex, float masterGain);
    [PreserveSig] void SetRumbleState(IntPtr parameters);
    [PreserveSig] int DirectInputEscape(uint command, IntPtr bufferIn, uint bufferInSize, IntPtr bufferOut, uint bufferOutSize, IntPtr bufferOutSizeWritten);
    [PreserveSig] int CreateInputMapper(out IGameInputMapper? mapper);
    [PreserveSig] int GetExtraAxisCount(GameInputKind inputKind, out uint extraAxisCount);
    [PreserveSig] int GetExtraButtonCount(GameInputKind inputKind, out uint extraButtonCount);
    [PreserveSig] int GetExtraAxisIndexes(GameInputKind inputKind, uint extraAxisCount, IntPtr extraAxisIndexes);
    [PreserveSig] int GetExtraButtonIndexes(GameInputKind inputKind, uint extraButtonCount, IntPtr extraButtonIndexes);
    [PreserveSig] int CreateRawDeviceReport(uint reportId, GameInputRawDeviceReportKind reportKind, out IGameInputRawDeviceReport? report);
    [PreserveSig] int SendRawDeviceOutput(IGameInputRawDeviceReport report);
}

[ComImport, Guid("415EED2E-98CB-42C2-8F28-B94601074E31"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInputDispatcher
{
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool Dispatch(ulong quotaInMicroseconds);
    [PreserveSig] int OpenWaitHandle(out IntPtr waitHandle);
}

[ComImport, Guid("FF61096A-3373-4093-A1DF-6D31846B3511"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInputForceFeedbackEffect
{
    [PreserveSig] void GetDevice(out IGameInputDevice device);
    [PreserveSig] uint GetMotorIndex();
    [PreserveSig] float GetGain();
    [PreserveSig] void SetGain(float gain);
    [PreserveSig] void GetParams(out GameInputForceFeedbackParams parameters);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool SetParams(in GameInputForceFeedbackParams parameters);
    [PreserveSig] GameInputFeedbackEffectState GetState();
    [PreserveSig] void SetState(GameInputFeedbackEffectState state);
}

[ComImport, Guid("3C600700-F16C-49CE-9BE6-6A2EF752ED5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGameInputMapper
{
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetArcadeStickButtonMappingInfo(GameInputArcadeStickButtons button, out GameInputButtonMapping mapping);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetFlightStickAxisMappingInfo(GameInputFlightStickAxes axis, out GameInputAxisMapping mapping);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetFlightStickButtonMappingInfo(GameInputFlightStickButtons button, out GameInputButtonMapping mapping);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetGamepadAxisMappingInfo(GameInputGamepadAxes axis, out GameInputAxisMapping mapping);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetGamepadButtonMappingInfo(GameInputGamepadButtons button, out GameInputButtonMapping mapping);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetRacingWheelAxisMappingInfo(GameInputRacingWheelAxes axis, out GameInputAxisMapping mapping);
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)] bool GetRacingWheelButtonMappingInfo(GameInputRacingWheelButtons button, out GameInputButtonMapping mapping);
}
