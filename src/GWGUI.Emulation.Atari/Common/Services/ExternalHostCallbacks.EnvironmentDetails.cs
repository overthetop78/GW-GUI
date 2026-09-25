using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable
{
private bool SetRotation(nint data)
    {
        if (data == nint.Zero) return false;
        var rotation = unchecked((uint)Marshal.ReadInt32(data));
        if (rotation is < EnvironmentConstants.FirstRotation or > EnvironmentConstants.LastRotation)
            return false;
        Rotation = rotation;
        return true;
    }

    private bool CapturePerformanceLevel(nint data)
    {
        if (data == nint.Zero) return false;
        PerformanceLevel = unchecked((uint)Marshal.ReadInt32(data));
        return true;
    }

    private bool CaptureKeyboardCallback(nint data)
    {
        if (data == nint.Zero) return false;
        KeyboardCallbackPointer = Marshal.PtrToStructure<ExternalCoreApi.KeyboardCallback>(data).Callback;
        _keyboardEvent = KeyboardCallbackPointer == nint.Zero
            ? null
            : Marshal.GetDelegateForFunctionPointer<ExternalCoreApi.KeyboardEvent>(KeyboardCallbackPointer);
        return true;
    }

    private bool SetPixelFormat(nint data)
    {
        if (data == nint.Zero) return false;
        switch (Marshal.ReadInt32(data))
        {
            case CommonConstants.PixelFormatXrgb8888:
                _pixelFormat = EmulationPixelFormat.Xrgb8888;
                return true;
            case CommonConstants.PixelFormatRgb565:
                _pixelFormat = EmulationPixelFormat.Rgb565;
                return true;
            case CommonConstants.PixelFormat0Rgb1555:
                _pixelFormat = EmulationPixelFormat.Rgb1555;
                return true;
            default:
                return false;
        }
    }

    private bool SetSystemAvInfo(nint data)
    {
        if (data == nint.Zero) return false;
        var info = Marshal.PtrToStructure<ExternalCoreApi.SystemAvInfo>(data);
        ApplySystemAvInfo(info);
        return true;
    }

    private bool SetGeometry(nint data)
    {
        if (data == nint.Zero) return false;
        Geometry = Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data);
        AspectRatio = Geometry.AspectRatio;
        return true;
    }

    private bool ReadMessage(nint data)
    {
        if (data == nint.Zero) return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.Message>(data);
        var text = CopyDiagnostic(message.Text);
        if (text is not null) Messages.Add(new(text, message.Frames));
        return true;
    }

    private bool ReadExtendedMessage(nint data)
    {
        if (data == nint.Zero) return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.MessageExtended>(data);
        var text = CopyDiagnostic(message.Text);
        if (text is not null) ExtendedMessages.Add(new(text, message.DurationMilliseconds, message.Priority,
            message.Level, message.Target, message.Type, message.Progress));
        return true;
    }

    private string? CopyDiagnostic(nint textPointer)
    {
        var text = Marshal.PtrToStringUTF8(textPointer);
        if (!string.IsNullOrWhiteSpace(text)) Diagnostics.Add(text);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private bool SetLogInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.LogInterface
        {
            Log = Marshal.GetFunctionPointerForDelegate(Log)
        }, data, false);
        return true;
    }

    private bool SetLedInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.LedInterface
        {
            SetLedState = Marshal.GetFunctionPointerForDelegate(SetLedState)
        }, data, false);
        _usesNativeLedInterface = true;
        return true;
    }

    private bool SetRumbleInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.RumbleInterface
        {
            SetState = Marshal.GetFunctionPointerForDelegate(SetRumbleState)
        }, data, false);
        return true;
    }

    private bool SetSensorInterface(nint data)
    {
        if (data == nint.Zero) return false;
        Marshal.StructureToPtr(new ExternalCoreApi.SensorInterface
        {
            SetState = Marshal.GetFunctionPointerForDelegate(SetSensorState),
            GetInput = Marshal.GetFunctionPointerForDelegate(GetSensorInput)
        }, data, false);
        return true;
    }
}
