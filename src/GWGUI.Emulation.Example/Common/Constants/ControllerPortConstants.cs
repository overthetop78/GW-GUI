namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Limites et quantités de ports de contrôleur prises en compte par Common.</summary>
/// <remarks>Une quantité de ports n’est pas un indice : avec deux ports, les indices valides sont zéro et un.
/// La description du modèle précise combien de ports sont effectivement disponibles dans cette limite.</remarks>
public static class ControllerPortConstants
{
    /// <summary>Limite de dix emplacements représentables par le transport d’entrée.</summary>
    /// <remarks>Cette limite ne déclare pas dix ports physiques sur chaque machine. Le catalogue matériel et les réglages
    /// du cœur déterminent les ports réellement utilisables pour chaque profil.</remarks>
    public const int MaximumControllerPortCount = 10;
    /// <summary>Limite de huit emplacements de manette dans les parcours de conversion des entrées de jeu.</summary>
    public const int MaximumGamepadPortCount = 8;
    /// <summary>Premier indice de port valide dans les associations de périphériques.</summary>
    public const int MinimumControllerPort = 0;
    /// <summary>Nombre de ports de contrôleur d’un modèle qui n’en expose aucun.</summary>
    public const int NoControllerPorts = 0;
    /// <summary>Quantité à employer pour un modèle disposant d’un seul port.</summary>
    public const int OneControllerPort = 1;
    /// <summary>Quantité à employer pour un modèle disposant de deux ports.</summary>
    public const int TwoControllerPorts = 2;
    /// <summary>Quantité à employer pour un modèle ou une configuration disposant de quatre ports.</summary>
    public const int FourControllerPorts = 4;
    /// <summary>Total de cinq contrôleurs disponibles quand un multitap équipe un des deux ports : quatre sur le multitap et un sur l’autre port.</summary>
    public const int SingleMultitapControllerCount = 5;
    /// <summary>Nombre de contrôleurs supplémentaires lorsqu’un multitap remplace un contrôleur directement connecté.</summary>
    public const int AdditionalControllersPerMultitap = 3;
    /// <summary>Nombre de contrôleurs supplémentaires lorsque le multitap est désactivé.</summary>
    public const int NoAdditionalControllers = 0;
}


