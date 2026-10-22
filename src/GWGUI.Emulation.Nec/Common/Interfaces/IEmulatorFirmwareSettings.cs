namespace GWGUI.Emulation.Nec.Common.Interfaces;

/// <summary>Emplacements de firmware proposés par un backend pour une configuration.</summary>
/// <remarks>Le catalogue reste possédé par l’adaptateur. Common le consulte pour les champs de sélection,
/// l’identification des candidats et la préparation des fichiers de session, sans modifier les fichiers sources.</remarks>
internal interface IEmulatorFirmwareSettings
{
    /// <summary>Décrit les emplacements disponibles pour le modèle et le cœur sélectionnés.</summary>
    /// <param name="configuration">Configuration empruntée fournissant le contexte de la consultation.</param>
    /// <returns>Emplacements du catalogue existant, avec leurs noms, alias, libellés et contraintes.</returns>
    IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration);
}
