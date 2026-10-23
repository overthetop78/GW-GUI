namespace GWGUI.Emulation.Sony.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Indique si le cœur exige au moins un fichier externe.</summary>
    /// <remarks>Cette exigence globale peut être satisfaite par plusieurs alternatives ; le caractère
    /// obligatoire d’un emplacement précis reste porté par IsRequired dans sa description FirmwareSlot.</remarks>
    bool RequiresExternalFirmware { get; }
}
