namespace GWGUI.Emulation.Snk.Common.Interfaces;

/// <summary>Réglages de firmware utilisés par Common pour configurer l’adaptateur.</summary>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Rôles proposés pour le modèle émulé.</summary>
    IReadOnlyList<FirmwareCategory> FirmwareRoles(MachineModel model);
    /// <summary>Indique si le rôle est obligatoire pour ce modèle.</summary>
    bool IsFirmwareRequired(MachineModel model, FirmwareCategory category);
}
