namespace GWGUI.Emulation.Nintendo.Common.Interfaces;

/// <summary>Emplacements et métadonnées de firmware proposés par le backend.</summary>
/// <remarks>Common consulte les règles du modèle et du cœur pour présenter les firmwares ; les fichiers sources restent empruntés.</remarks>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Décrit les emplacements de firmware proposés par cet adaptateur.</summary>
    /// <param name="configuration">Configuration du modèle pour lequel les emplacements sont demandés.</param>
    /// <returns>Descriptions des champs de sélection, fichiers attendus et contraintes associées.</returns>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
}
