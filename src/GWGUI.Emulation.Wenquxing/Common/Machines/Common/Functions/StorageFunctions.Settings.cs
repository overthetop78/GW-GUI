namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Functions;

/// <summary>Expose le contrat de stockage commun pour une machine sans lecteur de média externe.</summary>
/// <remarks>Le firmware et les sauvegardes de machine gardent leurs propriétaires habituels. Cette description
/// porte uniquement sur les lecteurs et montages proposés par le contrat de stockage de l’application.</remarks>
internal static class StorageSettingsFunctions
{
    /// <summary>Décrit les collections vides correspondant aux capacités réelles de la machine.</summary>
    /// <param name="configuration">Configuration empruntée ; aucun lecteur n’est créé pour la compléter.</param>
    /// <returns>Stockage sans lecteur, emplacement, montage ou paramètre de périphérique.</returns>
    internal static EmulationStorageSettings Describe(MachineConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return new([], [], [], []);
    }

    /// <summary>Conserve la configuration lorsque les réglages ne demandent aucun stockage externe.</summary>
    /// <param name="configuration">Configuration empruntée, conservée par référence lorsque les réglages sont valides.</param>
    /// <param name="settings">Choix de l’appelant ; toutes ses collections doivent être vides.</param>
    /// <returns>La même configuration ; cette opération ne crée ni média ni fichier.</returns>
    /// <exception cref="ArgumentException">Un lecteur, montage ou paramètre de périphérique est demandé.</exception>
    internal static MachineConfiguration Apply(MachineConfiguration configuration, EmulationStorageSettings settings)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.AvailableDevices.Count != 0 || settings.ConfiguredSlots.Count != 0 || settings.MountedMedia.Count != 0 || settings.DeviceSettings is { Count: > 0 })
            throw new ArgumentException(nameof(settings));
        return configuration;
    }
}
