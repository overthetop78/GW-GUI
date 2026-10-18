namespace GWGUI.Emulation.Interfaces;

internal interface IEmulationCoreOptionCommands
{
    bool ReturnValue(nint data);
    void RegisterLegacyVariables(nint data);
    bool GetAndClearUpdated(nint data);
    void RegisterVersionOne(nint data);
    void RegisterVersionOneInternational(nint data);
    void RegisterVersionTwo(nint data);
    void RegisterVersionTwoInternational(nint data);
    bool ApplyVisibility(nint data);
    bool CaptureDisplayUpdate(nint data);
    bool SetNativeValue(nint data);
}

