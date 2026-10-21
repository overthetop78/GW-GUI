namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Clés de présentation et actions proposées par les réglages de contrôleurs.</summary>
/// <remarks>Les identifiants d’action sont techniques et indépendants de la langue.
/// Les clés de ressources sont résolues lors de la présentation ; elles ne servent pas de valeurs de configuration.</remarks>
internal static class InputSettingsFunctionsConstants
{
    /// <summary>Clé du choix qui désactive le périphérique d’un port.</summary>
    internal const string ResourceControllerNone = "Emulation.Controller.None";
    /// <summary>Clé du choix qui laisse le module résoudre automatiquement le périphérique du port.</summary>
    internal const string ResourceControllerAutomatic = "Emulation.Controller.Automatic";
    /// <summary>Clé du libellé de la catégorie de joystick analogique.</summary>
    internal const string ResourceControllerAnalogJoystick = "Emulation.Controller.AnalogJoystick";
    /// <summary>Préfixe auquel l’identifiant d’action est ajouté pour retrouver son libellé traduit.</summary>
    internal const string ResourceActionPrefix = "Emulation.Controller.Action.";

    /// <summary>Actions présentées dans l’ordre : haut, bas, gauche, droite et premier bouton de tir.</summary>
    /// <remarks>Les valeurs proviennent des identifiants communs de commandes utilisés par les associations d’entrée.</remarks>
    internal static readonly IReadOnlyList<string> ControllerActions =
    [
        EmulationControllerCommandIds.Up, EmulationControllerCommandIds.Down,
        EmulationControllerCommandIds.Left, EmulationControllerCommandIds.Right,
        EmulationControllerCommandIds.Fire1
    ];
}
