namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

/// <summary>Prise en charge d’un périphérique de pointage annoncée par l’adaptateur pour une configuration.</summary>
/// <remarks>Common consulte cette capacité pour présenter les réglages et configurer la machine.
/// Elle ne possède aucun périphérique physique. La machine applique séparément les choix de capture de l’utilisateur.</remarks>
internal interface IEmulatorPointerCapabilities
{
    /// <summary>Indique si le cœur accepte les entrées de pointage de cette configuration.</summary>
    /// <param name="configuration">Modèle, options et associations de ports à prendre en compte.</param>
    /// <returns>True lorsque le backend prend en charge le pointeur configuré, sinon false.</returns>
    /// <remarks>Les adaptateurs dont la capacité est fixe conservent leur valeur constante. Les autres consultent
    /// les options et les périphériques acceptés, y compris les entrées USB lorsqu’elles sont configurées.
    /// La disponibilité annoncée reste distincte de l’activation de la capture physique et de son déclenchement au clic.</remarks>
    bool SupportsPointer(MachineConfiguration configuration);

    /// <summary>Indique si le runtime peut proposer la capture physique du pointeur pour cette configuration.</summary>
    /// <param name="configuration">Modèle, options et périphériques dont le runtime prépare la capture.</param>
    /// <returns>Capacité annoncée à l’hôte avant la création de la machine.</returns>
    /// <remarks>Cette annonce reste distincte des entrées acceptées par le cœur et de l’activation choisie par
    /// l’utilisateur. Elle ne capture aucun périphérique, ne crée aucune session et ne possède aucune ressource.</remarks>
    bool SupportsPointerCapture(MachineConfiguration configuration);
}
