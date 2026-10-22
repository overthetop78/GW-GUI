using CommonCoreHostFunctions = GWGUI.Emulation.Apple.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable, IEmulationCoreEnvironmentCommands
{
    private bool OnEnvironment(uint command, nint data)
    {
        _environmentCommands.Add(command);
        if (CommonCoreHostFunctions.EnvironmentProcessing.TryHandleStatelessCommand(command, data,
                CoreOptionConstants.SupportedInterfaceVersion, ExternalCoreInteropConstants.MessageInterfaceVersion,
                EnvironmentConstants.JoypadCapability | EnvironmentConstants.MouseCapability | EnvironmentConstants.KeyboardCapability | EnvironmentConstants.AnalogCapability, out var commonResult))
            return commonResult;
        if (CommonCoreHostFunctions.EnvironmentProcessing.TryHandleCallbackCommand(command, data, this, out var callbackResult))
            return callbackResult;
        return CommonCoreHostFunctions.EnvironmentProcessing.HandleRemainingCommand(command, data, _optionHost,
            _unknownEnvironmentCommands, Diagnostics, EnvironmentFunctions.CreateUnknownCommandDiagnostic);
    }

    bool IEmulationCoreEnvironmentCommands.WriteSystemDirectory(nint data)
    {
        return CoreFunctions.WritePointer(data, _systemDirectory.Pointer);
    }

    bool IEmulationCoreEnvironmentCommands.WriteContentDirectory(nint data)
    {
        return CoreFunctions.WritePointer(data, _assetsDirectory.Pointer);
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
        InputDescriptors = EnvironmentFunctions.CopyInputDescriptors(data);
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
        return CaptureKeyboardCallback(data);
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
