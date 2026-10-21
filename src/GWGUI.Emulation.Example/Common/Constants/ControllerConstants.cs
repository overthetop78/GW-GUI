namespace GWGUI.Emulation.Example.Common.Constants;


/// <summary>Bornes de zone morte et représentation numérique des axes de contrôleurs.</summary>
/// <remarks>Les réglages utilisateur expriment la zone morte en pourcentage de la course.
/// Le traitement des entrées convertit ce pourcentage vers l’amplitude des axes signés sur seize bits.
/// Ces constantes décrivent les valeurs ; les fonctions de contrôleur appliquent la transformation.</remarks>
public static class ControllerConstants
{
    /// <summary>Zone morte minimale, en pourcentage : aucune portion de course n’est neutralisée.</summary>
    public const int MinimumDeadZonePercent = 0;
    /// <summary>Zone morte maximale, en pourcentage : toute la course est neutralisée.</summary>
    public const int MaximumDeadZonePercent = 100;
    /// <summary>Zone morte utilisée avant un réglage explicite : quinze pour cent de la course.</summary>
    public const int DefaultDeadZonePercent = 15;
    /// <summary>Amplitude positive maximale d’un axe System.Int16 : 32 767.</summary>
    internal const int MaximumAxisMagnitude = short.MaxValue;
    /// <summary>Diviseur convertissant un pourcentage entier en proportion.</summary>
    internal const int PercentageDivisor = 100;
    /// <summary>Valeur d’un axe au repos ou neutralisé par la zone morte.</summary>
    internal const short NeutralAxis = 0;
    /// <summary>Index analogique du protocole utilisé pour interroger les gâchettes.</summary>
    internal const uint TriggerAnalogIndex = 2;
    /// <summary>Identifiant de la gâchette gauche dans le protocole d’entrée.</summary>
    internal const uint LeftTriggerId = 12;
    /// <summary>Identifiant de la gâchette droite dans le protocole d’entrée.</summary>
    internal const uint RightTriggerId = 13;
    /// <summary>Masque du premier bouton de tir : seul le bit de poids faible est activé.</summary>
    public const uint FireButtonMask = 1u;
    /// <summary>Capacité d’un joystick proposant un seul bouton de tir.</summary>
    public const int SingleButtonJoystickButtonCount = 1;
    /// <summary>Capacité du joystick à deux boutons.</summary>
    public const int JoystickButtonCount = 2;
    /// <summary>Capacité de la variante de joystick à quatre boutons.</summary>
    public const int FourButtonJoystickButtonCount = 4;
    /// <summary>Capacité des contrôleurs étendus à huit boutons.</summary>
    public const int ExtendedJoystickButtonCount = 8;
}

