namespace GWGUI.Emulation.Example.Common.Machines.Common.Functions;

/// <summary>Conversion des catégories du contrat public vers les catégories génériques du module.</summary>
internal static class EmulationPeripheralConversionFunctions
{
    /// <summary>Convertit une demande publique pour le port d’une machine.</summary>
    /// <param name="peripheral">Catégorie annoncée par l’hôte.</param>
    /// <param name="configuration">Configuration dont le profil fournit les ports disponibles.</param>
    /// <param name="port">Indice du port, à partir de zéro.</param>
    /// <returns>Catégorie locale correspondante ; les variantes de manettes rejoignent le profil générique.</returns>
    internal static PeripheralCategory ToModule(EmulationPeripheralCategory peripheral, MachineConfiguration configuration, int port)
    {
        if (port < 0 || port >= ConfigurationFunctions.ControllerPortCount(configuration.Model)) throw new ArgumentOutOfRangeException(nameof(port));
        return peripheral switch
        {
            EmulationPeripheralCategory.None => PeripheralCategory.None,
            EmulationPeripheralCategory.Automatic => PeripheralCategory.Automatic,
            EmulationPeripheralCategory.Keyboard => PeripheralCategory.Keyboard,
            EmulationPeripheralCategory.Mouse => PeripheralCategory.Mouse,
            EmulationPeripheralCategory.Joystick => PeripheralCategory.Joystick,
            EmulationPeripheralCategory.AnalogJoystick => PeripheralCategory.AnalogJoystick,
            EmulationPeripheralCategory.Paddle => PeripheralCategory.Paddle,
            EmulationPeripheralCategory.LightGun => PeripheralCategory.LightGun,
            EmulationPeripheralCategory.NumericKeypad => PeripheralCategory.NumericKeypad,
            EmulationPeripheralCategory.DrivingController => PeripheralCategory.DrivingController,
            _ => PeripheralCategory.RetroPad
        };
    }
}
