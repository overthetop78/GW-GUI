namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Prépare les fichiers requis par le cœur dans le répertoire de session indiqué.</summary>
    void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
}
