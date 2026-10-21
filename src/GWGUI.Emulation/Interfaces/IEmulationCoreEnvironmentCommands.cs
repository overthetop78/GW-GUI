namespace GWGUI.Emulation.Interfaces;

/// <summary>Opérations d’environnement natives partagées par les adaptateurs de modules.</summary>
/// <remarks>Les pointeurs sont empruntés selon l’ABI ExternalCoreApi. Chaque opération renvoie sa prise en charge réelle ; les copies, allocations et délégués qu’elle crée restent sous sa responsabilité.</remarks>
public interface IEmulationCoreEnvironmentCommands
{
    bool SetRotation(nint data);
    bool CapturePerformanceLevel(nint data);
    bool SetPixelFormat(nint data);
    bool SetSystemAvInfo(nint data);
    bool SetGeometry(nint data);
    bool ReadMessage(nint data);
    bool ReadExtendedMessage(nint data);
    bool SetLogInterface(nint data);
    bool SetLedInterface(nint data);
    bool SetRumbleInterface(nint data);
    bool SetSensorInterface(nint data);
    bool WriteSystemDirectory(nint data);
    bool WriteContentDirectory(nint data);
    bool WriteSaveDirectory(nint data);
    bool SetSupportNoGame(nint data);
    bool CaptureInputDescriptors(nint data);
    bool CaptureControllerInfo(nint data);
    bool CaptureMemoryMaps(nint data);
    bool SetSupportAchievements(nint data);
    bool ProvideVfs(nint data);
    bool SetKeyboardCallback(nint data);
    bool CaptureDiskControl(nint data);
    bool CaptureExtendedDiskControl(nint data);
    bool GetDiskControlVersion(nint data);
}
