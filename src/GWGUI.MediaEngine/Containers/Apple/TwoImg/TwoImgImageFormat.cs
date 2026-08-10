namespace GWGUI.MediaEngine.Containers.Apple.TwoImg;

/// <summary>Identifie l’organisation fermée de la charge utile déclarée dans un en-tête 2IMG.</summary>
public enum TwoImgImageFormat : uint
{
    /// <summary>Secteurs Apple II enregistrés dans l’ordre logique Apple DOS.</summary>
    DOS = 0,

    /// <summary>Blocs Apple II enregistrés dans l’ordre logique ProDOS.</summary>
    ProDOS = 1,

    /// <summary>Pistes Apple II enregistrées sous forme nibblisée.</summary>
    NIB = 2
}
