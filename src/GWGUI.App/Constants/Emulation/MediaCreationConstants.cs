namespace GWGUI.App.Constants.Emulation;

/// <summary>Présentation et ressources du dialogue commun de création de média.</summary>
internal static class MediaCreationConstants
{
    /// <summary>Largeur du dialogue de création de média, en unités logiques WPF.</summary>
    internal const double Width = 760;

    /// <summary>Largeur des boutons du dialogue, en unités logiques WPF.</summary>
    internal const double ButtonWidth = 110;
    /// <summary>Clé de traduction de l’action de création suivie du montage du média.</summary>
    internal const string CreateAndMount = "Emulation.Storage.Media.CreateAndMount";
    /// <summary>Clé de traduction du message confirmant la création du média.</summary>
    internal const string Created = "Emulation.Storage.Media.Created";
    /// <summary>Clé de traduction du message signalant que le fichier de destination existe déjà.</summary>
    internal const string AlreadyExists = "Emulation.Storage.Media.AlreadyExists";
}
