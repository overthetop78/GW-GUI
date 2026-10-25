namespace GWGUI.Emulation.Example.Common.Machines.Common.Enums;

/// <summary>Catégories génériques d’entrées proposées par les machines d’exemple.</summary>
public enum PeripheralCategory
{
    /// <summary>Port désactivé.</summary>
    None,
    /// <summary>Choix adapté par l’émulateur aux capacités du port.</summary>
    Automatic,
    /// <summary>Clavier émulé, avec ses touches configurables.</summary>
    Keyboard,
    /// <summary>Souris comportant boutons, axes relatifs et molettes.</summary>
    Mouse,
    /// <summary>Manette générique avec boutons, croix, sticks et gâchettes.</summary>
    RetroPad,
    /// <summary>Joystick numérique avec directions et boutons.</summary>
    Joystick,
    /// <summary>Joystick à axes analogiques et boutons.</summary>
    AnalogJoystick,
    /// <summary>Volant avec direction, pédales et boutons.</summary>
    DrivingController,
    /// <summary>Pistolet optique avec position et commandes de tir.</summary>
    LightGun,
    /// <summary>Contrôleur rotatif à axe et boutons.</summary>
    Paddle,
    /// <summary>Pavé de touches numériques.</summary>
    NumericKeypad,
    /// <summary>Tapis de danse avec directions et panneaux.</summary>
    DanceMat,
    /// <summary>Stylet ou tablette de pointage à coordonnées absolues.</summary>
    Pointer,
}
