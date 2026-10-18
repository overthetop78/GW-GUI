using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Sharp.Emulators.Common.Interop.Functions;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Sharp.Emulators.Common.Interop.Services;
internal sealed partial class ExternalHostCallbacks : IDisposable, IEmulationCoreEnvironmentCommands
{
    private bool OnEnvironment(uint command, nint data)
    {
        _environmentCommands.Add(command);
        if (ExternalCoreEnvironmentFunctions.TryHandleStatelessCommand(command, data,
                CoreOptionConstants.SupportedInterfaceVersion, ExternalCoreInteropConstants.MessageInterfaceVersion,
                EnvironmentConstants.SupportedInputCapabilities, out var commonResult))
            return commonResult;
        if (ExternalCoreEnvironmentFunctions.TryHandleCallbackCommand(command, data, this, out var callbackResult))
            return callbackResult;
        switch (command)
        {
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

    bool IEmulationCoreEnvironmentCommands.WriteSystemDirectory(nint data)
    {
        return CoreFunctions.WritePointer(data, _systemDirectory.Pointer);
    }

    bool IEmulationCoreEnvironmentCommands.WriteContentDirectory(nint data)
    {
        return CoreFunctions.WritePointer(data, _contentDirectory.Pointer);
    }

    bool IEmulationCoreEnvironmentCommands.WriteSaveDirectory(nint data)
    {
        return CoreFunctions.WritePointer(data, _saveDirectory.Pointer);
    }

    bool IEmulationCoreEnvironmentCommands.SetSupportNoGame(nint data)
    {
        SupportsNoGame = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
        return data != nint.Zero;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureInputDescriptors(nint data)
    {
        InputDescriptors = EnvironmentFunctions.CopyInputDescriptors(data, _inputDescriptorLimit);
        return data != nint.Zero;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureControllerInfo(nint data)
    {
        ControllerPorts = EnvironmentFunctions.CopyControllerPorts(data);
        return data != nint.Zero;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureMemoryMaps(nint data)
    {
        MemoryDescriptors = EnvironmentFunctions.CopyMemoryMap(data);
        return data != nint.Zero;
    }

    bool IEmulationCoreEnvironmentCommands.SetSupportAchievements(nint data)
    {
        SupportsAchievements = data != nint.Zero && Marshal.ReadByte(data) != ExternalCoreInteropConstants.NativeBooleanFalse;
        return data != nint.Zero;
    }

    bool IEmulationCoreEnvironmentCommands.ProvideVfs(nint data)
    {
        return _virtualFileSystem.Provide(data);
    }

    bool IEmulationCoreEnvironmentCommands.SetKeyboardCallback(nint data)
    {
        if (data == nint.Zero)
                            return false;
        KeyboardCallbackPointer = Marshal.ReadIntPtr(data);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureDiskControl(nint data)
    {
        _diskControl.Capture(data);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.CaptureExtendedDiskControl(nint data)
    {
        _diskControl.CaptureExtended(data);
        return true;
    }

    bool IEmulationCoreEnvironmentCommands.GetDiskControlVersion(nint data)
    {
        return CoreFunctions.WriteInteger(data, DiskControlConstants.InterfaceVersion);
    }
}