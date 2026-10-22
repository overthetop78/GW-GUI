namespace GWGUI.Emulation.Apple.Common.Interfaces;

/// <summary>Adaptation du matériel de stockage aux lecteurs choisis dans la configuration.</summary>
/// <remarks>Common valide les emplacements et les médias avant de demander les adaptations propres au backend.</remarks>
internal interface IEmulatorStorageSettings
{
    /// <summary>Adapte les options matérielles à la sélection de lecteurs.</summary>
    /// <param name="configuration">Configuration de départ, conservée immuable.</param>
    /// <param name="slots">Emplacements choisis par l’appelant, indépendamment des médias insérés.</param>
    /// <returns>Configuration dont les options de matériel respectent les règles existantes du backend.</returns>
    MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots);
}
