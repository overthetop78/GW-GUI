namespace GWGUI.Emulation.Interfaces;

/// <summary>Commandes communes des catalogues d’options et de leurs valeurs courantes.</summary>
/// <remarks>Les tables natives sont empruntées ; l’hôte conserve ses copies et ses allocations de valeurs jusqu’à sa destruction. Les différentes versions correspondent aux API réellement déclarées par les cœurs.</remarks>
public interface IEmulationCoreOptionCommands
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

