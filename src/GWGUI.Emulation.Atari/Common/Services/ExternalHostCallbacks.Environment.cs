using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable
{
private bool OnEnvironment(uint command, nint data)
    {
        _environmentCommands.Add(command);
        switch (command)
        {
            case ExternalCoreApiConstants.SetRotation:
                return SetRotation(data);
            case ExternalCoreApiConstants.GetOverscan:
                return CoreFunctions.WriteBoolean(data, false);
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
            case ExternalCoreApiConstants.GetVariable:
                return _optionHost.ReturnValue(data);
            case ExternalCoreApiConstants.SetVariables:
                _optionHost.RegisterLegacyVariables(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.GetVariableUpdate:
                return _optionHost.GetAndClearUpdated(data);
            case ExternalCoreApiConstants.SetSupportNoGame:
                SupportsNoGame = data != nint.Zero
                    && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
                return data != nint.Zero;
            case ExternalCoreApiConstants.GetCanDuplicateFrames:
            case ExternalCoreApiConstants.GetInputBitmasks:
                return CoreFunctions.WriteBoolean(data, true);
            case ExternalCoreApiConstants.GetCoreOptionsVersion:
                return CoreFunctions.WriteInteger(data, CoreOptionConstants.SupportedInterfaceVersion);
            case ExternalCoreApiConstants.SetCoreOptions:
                _optionHost.RegisterVersionOne(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetCoreOptionsInternational:
                _optionHost.RegisterVersionOneInternational(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.GetMessageInterfaceVersion:
                return CoreFunctions.WriteInteger(data, ExternalCoreInteropConstants.MessageInterfaceVersion);
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
            case ExternalCoreApiConstants.GetPerformanceInterface:
                return false;
            case ExternalCoreApiConstants.GetLedInterface:
                return SetLedInterface(data);
            case ExternalCoreApiConstants.GetRumbleInterface:
                return SetRumbleInterface(data);
            case ExternalCoreApiConstants.GetSensorInterface:
                return SetSensorInterface(data);
            case ExternalCoreApiConstants.GetInputDeviceCapabilities:
                return CoreFunctions.WriteUnsignedLong(data,
                    EnvironmentConstants.JoypadCapability | EnvironmentConstants.MouseCapability |
                    EnvironmentConstants.KeyboardCapability | EnvironmentConstants.AnalogCapability);
            case ExternalCoreApiConstants.GetLanguage:
                return CoreFunctions.WriteInteger(data, EnvironmentFunctions.CurrentLanguage());
            case ExternalCoreApiConstants.GetFastForwarding:
                return CoreFunctions.WriteBoolean(data, false);
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
                SupportsAchievements = data != nint.Zero
                    && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
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
            case ExternalCoreApiConstants.SetCoreOptionsV2:
                _optionHost.RegisterVersionTwo(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetCoreOptionsV2International:
                _optionHost.RegisterVersionTwoInternational(data);
                return data != nint.Zero;
            case ExternalCoreApiConstants.SetCoreOptionsDisplay:
                return _optionHost.ApplyVisibility(data);
            case ExternalCoreApiConstants.SetCoreOptionsUpdateDisplayCallback:
                return _optionHost.CaptureDisplayUpdate(data);
            case ExternalCoreApiConstants.SetVariable:
                return _optionHost.SetNativeValue(data);
            case ExternalCoreApiConstants.GetVfsInterface:
                return _virtualFileSystem.Provide(data);
            case ExternalCoreApiConstants.GetMidiInterface:
            case ExternalCoreApiConstants.SetFastForwardingOverride:
            case ExternalCoreApiConstants.SetContentInfoOverride:
            case ExternalCoreApiConstants.SetNetworkPacketInterface:
            case ExternalCoreApiConstants.SetSerializationQuirks:
                return false;
            default:
                if (_unknownEnvironmentCommands.Add(command))
                    Diagnostics.Add(EnvironmentFunctions.CreateUnknownCommandDiagnostic(command));
                return false;
        }
    }
}
