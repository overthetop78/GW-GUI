namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreEnvironmentCommands
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
}
