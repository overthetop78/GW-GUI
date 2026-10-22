namespace GWGUI.Emulation.Snk.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Rôles proposés pour le modèle émulé.</summary>
    /// <param name="model">Modèle matériel dont les catégories doivent être présentées.</param>
    /// <returns>Rôles du catalogue du backend, dans leur ordre de présentation.</returns>
    IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);
    /// <summary>Indique si le rôle est obligatoire pour ce modèle.</summary>
    /// <param name="model">Modèle dont les exigences sont consultées.</param>
    /// <param name="category">Rôle précis de firmware à contrôler.</param>
    /// <returns>True lorsque les règles existantes du backend exigent ce rôle pour ce modèle.</returns>
    bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
}
