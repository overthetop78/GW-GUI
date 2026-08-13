namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Distingue une révolution lue sur le support d'une révolution reconstruite par encodage.</summary>
public enum ScpRevolutionOrigin
{
    /// <summary>La révolution provient d'une capture de flux.</summary>
    Captured,

    /// <summary>La révolution a été reconstruite de manière déterministe depuis des données logiques.</summary>
    Synthetic
}
