namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Valeurs binaires du protocole d’interopérabilité des cœurs externes.</summary>
/// <remarks>Les versions identifient les contrats natifs. Les codes de pixels décrivent l’organisation
/// des bits, indépendamment de l’affichage de l’hôte. Les tailles natives utilisent nuint pour suivre
/// la largeur de pointeur du processus. Ces valeurs sont partagées par les adaptateurs du module.</remarks>
internal static class ExternalCoreInteropConstants
{
    /// <summary>Version attendue du contrat principal de l’API native.</summary>
    internal const uint ApiVersion = 1;
    /// <summary>Version du contrat des messages natifs.</summary>
    internal const int MessageInterfaceVersion = 1;
    /// <summary>Code du format RGB sur seize bits avec cinq bits par composante.</summary>
    internal const int PixelFormat0Rgb1555 = 0;
    /// <summary>Code du format XRGB sur trente-deux bits avec huit bits par composante.</summary>
    internal const int PixelFormatXrgb8888 = 1;
    /// <summary>Code du format RGB sur seize bits avec six bits pour le vert.</summary>
    internal const int PixelFormatRgb565 = 2;
    /// <summary>État natif d’une entrée inactive, typé comme le mot de l’API d’entrée.</summary>
    internal const uint NoInputState = 0;
    /// <summary>Représentation sur un octet de la valeur booléenne fausse.</summary>
    internal const byte NativeBooleanFalse = 0;
    /// <summary>Représentation sur un octet de la valeur booléenne vraie.</summary>
    internal const byte NativeBooleanTrue = 1;
    /// <summary>Nombre d’éléments d’une collection native vide.</summary>
    internal const uint EmptyNativeCollectionCount = 0;
    /// <summary>Dimension en pixels d’une image native absente.</summary>
    internal const uint EmptyFrameDimension = 0;
    /// <summary>Longueur ou taille native nulle, avec la largeur du pointeur courant.</summary>
    internal const nuint EmptyNativeSize = 0;
    /// <summary>État entier signalant qu’une fonctionnalité native est inactive.</summary>
    internal const int InactiveState = 0;
}
