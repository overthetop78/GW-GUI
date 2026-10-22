namespace GWGUI.Emulation.Sony.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Indique si le cœur exige au moins un fichier externe.</summary>
    bool RequiresExternalFirmware { get; }
    /// <summary>Décrit les emplacements proposés pour la configuration courante.</summary>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
}
