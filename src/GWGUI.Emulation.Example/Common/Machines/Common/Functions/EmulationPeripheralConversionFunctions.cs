namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Adaptation des catégories de périphériques du contrat SDK vers les catégories internes du module.</summary>
internal static class EmulationPeripheralConversionFunctions
{
    /// <summary>Résout la catégorie interne correspondant au périphérique demandé par l’hôte.</summary>
    /// <param name="peripheral">Catégorie commune reçue par l’interface d’entrée.</param>
    /// <returns>Catégorie interne ; Joystick, RetroPad et AnalogJoystick utilisent tous la catégorie analogique.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La catégorie SDK n’est pas prise en charge par cette conversion.</exception>
    internal static PeripheralCategory ToAcorn(EmulationPeripheralCategory peripheral) => peripheral switch
    {
        EmulationPeripheralCategory.None => PeripheralCategory.None,
        EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
        EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
        EmulationPeripheralCategory.Joystick or EmulationPeripheralCategory.RetroPad
            or EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
        _ => throw new ArgumentOutOfRangeException(nameof(peripheral), peripheral, null)
    };
}
