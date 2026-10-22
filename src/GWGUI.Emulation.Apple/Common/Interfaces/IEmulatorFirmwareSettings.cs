namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Prépare les fichiers requis par le cœur dans le répertoire de session indiqué.</summary>
    /// <param name="configuration">Configuration empruntée contenant les fichiers de firmware choisis.</param>
    /// <param name="systemDirectory">Dossier système préparé pour la session, possédé par son propriétaire.</param>
    /// <remarks>Les fichiers sources restent empruntés. Cette opération conserve leurs règles de copie et
    /// de destination ; leur propriétaire conserve la responsabilité de les supprimer ou de les remplacer.</remarks>
    void PrepareFirmware(MachineConfiguration configuration, string systemDirectory);
}
