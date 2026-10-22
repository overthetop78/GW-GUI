using CommonCoreHostFunctions = GWGUI.Emulation.Snk.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Services;
internal sealed partial class ExternalHostCallbacks : IDisposable
{
    public bool SetRotation(nint data)
    {
        if (data == nint.Zero)
            return false;
        var rotation = unchecked((uint)Marshal.ReadInt32(data));
        if (rotation is < EnvironmentConstants.FirstRotation or > EnvironmentConstants.LastRotation)
            return false;
        Rotation = rotation;
        return true;
    }

    public bool CapturePerformanceLevel(nint data)
    {
        if (data == nint.Zero)
            return false;
        PerformanceLevel = unchecked((uint)Marshal.ReadInt32(data));
        return true;
    }

    public bool SetPixelFormat(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.SetPixelFormat(data, ref _pixelFormat);

    public bool SetSystemAvInfo(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.ApplyStructure<ExternalCoreApi.SystemAvInfo>(data, ApplySystemAvInfo);

    public bool SetGeometry(nint data)
    {
        if (data == nint.Zero)
            return false;
        Geometry = Marshal.PtrToStructure<ExternalCoreApi.Geometry>(data);
        AspectRatio = Geometry.AspectRatio;
        return true;
    }

    public bool ReadMessage(nint data)
    {
        if (data == nint.Zero)
            return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.Message>(data);
        var text = CopyDiagnostic(message.Text);
        if (text is not null)
            Messages.Add(new(text, message.Frames));
        return true;
    }

    public bool ReadExtendedMessage(nint data)
    {
        if (data == nint.Zero)
            return false;
        var message = Marshal.PtrToStructure<ExternalCoreApi.MessageExtended>(data);
        var text = CopyDiagnostic(message.Text);
        if (text is not null)
            ExtendedMessages.Add(new(text, message.DurationMilliseconds, message.Priority, message.Level, message.Target, message.Type, message.Progress));
        return true;
    }

    private string? CopyDiagnostic(nint textPointer)
        => CommonCoreHostFunctions.EnvironmentProcessing.CopyDiagnostic(textPointer, Diagnostics.Add);

    public bool SetLogInterface(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.SetLogInterface(data, Log);

    public bool SetLedInterface(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.SetLedInterface(data, SetLedState);

    public bool SetRumbleInterface(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.SetRumbleInterface(data, SetRumbleState);

    public bool SetSensorInterface(nint data)
        => CommonCoreHostFunctions.EnvironmentProcessing.SetSensorInterface(data, SetSensorState, GetSensorInput);
}