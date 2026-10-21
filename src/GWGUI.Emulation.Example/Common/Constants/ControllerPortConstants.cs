namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Limites et quantités de ports de contrôleur prises en compte par Common.</summary>
/// <remarks>Une quantité de ports n’est pas un indice : avec deux ports, les indices valides sont zéro et un.
/// La description du modèle précise combien de ports sont effectivement disponibles dans cette limite.</remarks>
public static class ControllerPortConstants
{
    /// <summary>Nombre maximal de ports représentés par les configurations et contrôles de ce module.</summary>
    public const int MaximumControllerPortCount = 2;
    /// <summary>Premier indice de port valide dans les associations de périphériques.</summary>
    public const int MinimumControllerPort = 0;
    /// <summary>Quantité à employer pour un modèle disposant d’un seul port.</summary>
    public const int OneControllerPort = 1;
    /// <summary>Quantité à employer pour un modèle disposant de deux ports.</summary>
    public const int TwoControllerPorts = 2;
}


