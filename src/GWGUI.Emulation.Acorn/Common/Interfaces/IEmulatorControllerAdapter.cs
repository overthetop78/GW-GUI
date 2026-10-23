namespace GWGUI.Emulation.Acorn.Common.Interfaces;

/// <summary>Organisation des ports de contrôleurs pour la configuration complète de la machine.</summary>
internal interface IEmulatorControllerAdapter
{
    /// <summary>Calcule le nombre de ports disponibles avec les options et extensions de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée portant le modèle, le cœur choisi et ses options.</param>
    /// <returns>Nombre de ports émulés ; zéro pour une machine sans port de contrôleur.</returns>
    int GetControllerPortCount(MachineConfiguration configuration);

}
