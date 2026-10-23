using CoreAudioBufferStatusCallback = GWGUI.Emulation.Interop.ExternalCoreApi.AudioBufferStatusCallback;
using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Coleco.Emulators.Common.Interop.Functions;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using GWGUI.Emulation;
using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Coleco.Emulators.Common.Interop.Services;

internal sealed partial class ExternalHostCallbacks : IDisposable, IEmulationCoreEnvironmentCommands
{
    /// <summary>Demande native de fermeture mémorisée pour la session, sans destruction depuis le callback.</summary>
    internal bool ShutdownRequested { get; private set; }


    private bool OnEnvironment(uint command, nint data)
    {
        if (command == ExternalCoreApiConstants.Shutdown) { ShutdownRequested = true; return true; }
        _environmentCommands.Add(command);
        if (CommonCoreHostFunctions.EnvironmentProcessing.TryHandleAudioBufferCommand(command, data, ref _audioBufferStatusCallback, ref _minimumAudioLatencyMilliseconds, out var audioResult)) return audioResult;
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
    /// <summary>Callback natif emprunté, conservé uniquement pendant la session du cœur.</summary>
    private CoreAudioBufferStatusCallback? _audioBufferStatusCallback;
    private int _minimumAudioLatencyMilliseconds;
    /// <summary>Instantané de la sortie physique transmis par Common avant le calcul de la frame.</summary>
    internal AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <summary>Latence minimale négociée avec le cœur, en millisecondes.</summary>
    internal int MinimumAudioLatencyMilliseconds => _minimumAudioLatencyMilliseconds;
    /// <summary>Notifie le délégué du cœur avec le calcul commun de remplissage et de sous-alimentation.</summary>
    internal void NotifyAudioBufferStatus() => CommonCoreHostFunctions.EnvironmentProcessing.PublishAudioBufferStatus(_audioBufferStatusCallback, OutputAudioStatus, FramesPerSecond);
    /// <summary>Retire les références empruntées avant le déchargement du code natif.</summary>
    private void ClearAudioBufferFeedback()
    {
        _audioBufferStatusCallback = null;
        OutputAudioStatus = null;
        _minimumAudioLatencyMilliseconds = 0;
    }
}
