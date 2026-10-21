namespace GWGUI.Emulation.Example.Common.Machines.Common.Constants;

/// <summary>Identifiants et préfixes reconnus pendant la projection des associations vers un instantané d’entrée.</summary>
/// <remarks>Ces valeurs sont des éléments de syntaxe technique, conservés indépendamment de la langue.
/// Les préfixes distinguent les sources clavier et souris ; les noms de boutons ou directions désignent les actions.</remarks>
internal static class InputSnapshotFunctionsConstants
{
    /// <summary>Identifiant du premier bouton de tir du contrôleur émulé.</summary>
    internal const string Fire1 = "Fire1";
    /// <summary>Identifiant commun de la direction haute du contrôleur.</summary>
    internal const string Up = EmulationInputMappingConstants.Up;
    /// <summary>Identifiant commun de la direction basse du contrôleur.</summary>
    internal const string Down = EmulationInputMappingConstants.Down;
    /// <summary>Identifiant commun de la direction gauche du contrôleur.</summary>
    internal const string Left = EmulationInputMappingConstants.Left;
    /// <summary>Identifiant commun de la direction droite du contrôleur.</summary>
    internal const string Right = EmulationInputMappingConstants.Right;
    /// <summary>Préfixe d’une association utilisant une touche du clavier physique.</summary>
    internal const string Keyboard = "Keyboard:";
    /// <summary>Préfixe d’une association utilisant un bouton ou une molette de la souris physique.</summary>
    internal const string Mouse = "Mouse:";
    /// <summary>Nom technique du bouton central de souris.</summary>
    internal const string Middle = "Middle";
    /// <summary>Nom technique du premier bouton supplémentaire de souris.</summary>
    internal const string XButton1 = "XButton1";
    /// <summary>Nom technique du second bouton supplémentaire de souris.</summary>
    internal const string XButton2 = "XButton2";
    /// <summary>Nom technique du mouvement de molette verticale vers le haut.</summary>
    internal const string WheelUp = "WheelUp";
    /// <summary>Nom technique du mouvement de molette verticale vers le bas.</summary>
    internal const string WheelDown = "WheelDown";
    /// <summary>Nom technique du mouvement de molette horizontale vers la gauche.</summary>
    internal const string WheelLeft = "WheelLeft";
    /// <summary>Nom technique du mouvement de molette horizontale vers la droite.</summary>
    internal const string WheelRight = "WheelRight";
}
