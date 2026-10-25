namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Bornes de zone morte, masque de bouton et nombres de boutons des profils de contrôleurs.</summary>
/// <remarks>La zone morte s'exprime en pourcentage de la course. Les nombres de boutons décrivent les profils
/// proposés et le masque code un état de bouton dans le mot d'entrées. Common transmet ces choix aux opérations
/// d'entrées par leurs interfaces ; les tableaux d'associations restent décrits séparément.</remarks>
public static class ControllerConstants
{
    /// <summary>Zone morte minimale, en pourcentage : aucune portion de course n’est neutralisée.</summary>
    public const int MinimumDeadZonePercent = 0;
    /// <summary>Zone morte maximale, en pourcentage : toute la course est neutralisée.</summary>
    public const int MaximumDeadZonePercent = 100;
    /// <summary>Zone morte utilisée avant un réglage explicite : quinze pour cent de la course.</summary>
    public const int DefaultDeadZonePercent = 15;
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

