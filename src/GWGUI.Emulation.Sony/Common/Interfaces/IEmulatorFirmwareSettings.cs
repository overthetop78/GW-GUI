namespace GWGUI.Emulation.Sony.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Indique si le cœur exige au moins un fichier externe.</summary>
    /// <remarks>Cette exigence globale peut être satisfaite par plusieurs alternatives ; le caractère
    /// obligatoire d’un emplacement précis reste porté par IsRequired dans sa description FirmwareSlot.</remarks>
    bool RequiresExternalFirmware { get; }
    /// <summary>Décrit les emplacements proposés pour la configuration courante.</summary>
    /// <param name="configuration">Configuration empruntée dont le modèle, le cœur et les options sont consultés.</param>
    /// <returns>Emplacements du catalogue du backend, avec leurs fichiers, libellés et contraintes.</returns>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
}
