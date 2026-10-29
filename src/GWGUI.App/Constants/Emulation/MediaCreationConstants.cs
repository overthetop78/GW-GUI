namespace GWGUI.App.Constants.Emulation;

/// <summary>Présentation et ressources du dialogue commun de création de média.</summary>
internal static class MediaCreationConstants
{
    /// <summary>Largeur du dialogue de création de média, en unités logiques WPF.</summary>
    internal const double Width = 660;

    /// <summary>Largeur des boutons du dialogue, en unités logiques WPF.</summary>
    internal const double ButtonWidth = 110;
    /// <summary>Clé de traduction de l’action de création suivie du montage du média.</summary>
    internal const string CreateAndMount = "Emulation.Storage.Media.CreateAndMount";
    /// <summary>Clé de traduction du message confirmant la création du média.</summary>
    internal const string Created = "Emulation.Storage.Media.Created";
    /// <summary>Diagnostic lorsque la destination de création n'est pas renseignée.</summary>
    internal const string PathRequired = "Emulation.Storage.Disk.ImageRequired";

    /// <summary>Libellé du choix de format fourni par MediaEngine.</summary>
    internal const string FormatLabel = "Emulation.Storage.Floppy.Format";
    /// <summary>Conteneur du fichier, distinct du format et de la géométrie de la disquette.</summary>
    internal const string ImageFormatLabel = "Emulation.Storage.Floppy.ImageFormat";

    /// <summary>Libellé du fichier de destination.</summary>
    internal const string DestinationLabel = "Emulation.Storage.File.Destination";

    /// <summary>Nom initial d'une nouvelle image, sans extension.</summary>
    internal const string BlankFileName = "blank";

    /// <summary>Clé de traduction du message signalant que le fichier de destination existe déjà.</summary>
    internal const string AlreadyExists = "Emulation.Storage.Media.AlreadyExists";
}
