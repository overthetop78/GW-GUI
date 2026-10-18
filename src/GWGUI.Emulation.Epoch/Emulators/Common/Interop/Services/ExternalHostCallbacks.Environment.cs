using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable
{
    private bool OnEnvironment(uint command, nint data)
    {
        _environmentCommands.Add(command);
        if (ExternalCoreEnvironmentFunctions.TryHandleStatelessCommand(command, data,
                CoreOptionConstants.SupportedInterfaceVersion, ExternalCoreInteropConstants.MessageInterfaceVersion,
                EnvironmentConstants.JoypadCapability | EnvironmentConstants.MouseCapability | EnvironmentConstants.KeyboardCapability | EnvironmentConstants.LightGunCapability | EnvironmentConstants.AnalogCapability, out var commonResult))
            return commonResult;
        switch (command)
        {
            case ExternalCoreApiConstants.SetRotation:
                return SetRotation(data);
            case ExternalCoreApiConstants.SetPerformanceLevel:
                return CapturePerformanceLevel(data);
            case ExternalCoreApiConstants.GetSystemDirectory:
                return CoreFunctions.WritePointer(data, _systemDirectory.Pointer);
            case ExternalCoreApiConstants.GetContentDirectory:
                return CoreFunctions.WritePointer(data, _assetsDirectory.Pointer);
            case ExternalCoreApiConstants.GetSaveDirectory:
                return CoreFunctions.WritePointer(data, _saveDirectory.Pointer);
            case ExternalCoreApiConstants.SetPixelFormat:
                return SetPixelFormat(data);
            case ExternalCoreApiConstants.SetSupportNoGame:
                SupportsNoGame = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetSystemAvInfo:
                return SetSystemAvInfo(data);
            case ExternalCoreApiConstants.SetGeometry:
                return SetGeometry(data);
            case ExternalCoreApiConstants.SetMessage:
                return ReadMessage(data);
            case ExternalCoreApiConstants.SetMessageExtended:
                return ReadExtendedMessage(data);
            case ExternalCoreApiConstants.GetLogInterface:
                return SetLogInterface(data);
            case ExternalCoreApiConstants.GetLedInterface:
                return SetLedInterface(data);
            case ExternalCoreApiConstants.GetRumbleInterface:
                return SetRumbleInterface(data);
            case ExternalCoreApiConstants.GetSensorInterface:
                return SetSensorInterface(data);
            case ExternalCoreApiConstants.SetInputDescriptors:
                InputDescriptors = EnvironmentFunctions.CopyInputDescriptors(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetKeyboardCallback:
                return CaptureKeyboardCallback(data);
            case ExternalCoreApiConstants.SetControllerInfo:
                ControllerPorts = EnvironmentFunctions.CopyControllerPorts(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetMemoryMaps:
                MemoryDescriptors = EnvironmentFunctions.CopyMemoryMap(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetSupportAchievements:
                SupportsAchievements = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetDiskControl:
                if (data == nint.Zero) return false;
                _diskControl.Capture(data);
                return true;
            case ExternalCoreApiConstants.SetDiskControlExtended:
                if (data == nint.Zero) return false;
                _diskControl.CaptureExtended(data);
                return true;
            case ExternalCoreApiConstants.GetDiskControlVersion:
                return CoreFunctions.WriteInteger(data, DiskControlConstants.InterfaceVersion);
            case ExternalCoreApiConstants.GetVfsInterface:
                return _virtualFileSystem.Provide(data);
            case ExternalCoreApiConstants.GetMidiInterface:
            case ExternalCoreApiConstants.SetFastForwardingOverride:
            case ExternalCoreApiConstants.SetContentInfoOverride:
            case ExternalCoreApiConstants.SetNetworkPacketInterface:
            case ExternalCoreApiConstants.SetSerializationQuirks:
                return false;
            default:
                if (ExternalCoreEnvironmentFunctions.TryHandleOptionCommand(command, data, _optionHost, out var optionResult))
                    return optionResult;
                if (_unknownEnvironmentCommands.Add(command))
                    Diagnostics.Add(EnvironmentFunctions.CreateUnknownCommandDiagnostic(command));
                return false;
        }
    }
}
