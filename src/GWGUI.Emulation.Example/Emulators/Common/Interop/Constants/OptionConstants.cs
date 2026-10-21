namespace GWGUI.Emulation.Example.Emulators.Common.Interop.Constants;

/// <summary>Clés et valeurs réellement interprétées par les trois simulations de cœur.</summary>
internal static class OptionConstants
{
    /// <summary>Choix de la résolution fixe appliquée lors de l’initialisation d’un cœur.</summary>
    internal const string Resolution = "example_resolution";
    /// <summary>Choix du format des pixels produits par les sorties vidéo simulées.</summary>
    internal const string PixelFormat = "example_pixel_format";
    /// <summary>Activation du signal audio de démonstration ; la sortie reste silencieuse par défaut.</summary>
    internal const string AudioTone = "example_audio_tone";
    /// <summary>Résolution de base, en pixels.</summary>
    internal const string DefaultResolution = "320x200";
    /// <summary>Résolution étendue, en pixels, illustrant la recréation des buffers.</summary>
    internal const string LargeResolution = "640x400";
    /// <summary>Code local d’activation du signal simulé, interprété par les trois cœurs.</summary>
    internal const string Enabled = "enabled";
    /// <summary>Code local de désactivation du signal simulé.</summary>
    internal const string Disabled = "disabled";
}
