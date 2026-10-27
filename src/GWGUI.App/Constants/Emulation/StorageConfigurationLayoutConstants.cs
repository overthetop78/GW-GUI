namespace GWGUI.App.Constants.Emulation;

/// <summary>Dimensions de présentation des dialogues génériques de stockage.</summary>
internal static class StorageConfigurationLayoutConstants
{
    /// <summary>Largeur du dialogue lorsque les champs sont répartis en groupes, en unités logiques WPF.</summary>
    internal const double GroupedWidth = 820;

    /// <summary>Largeur du dialogue de configuration géré, en unités logiques WPF.</summary>
    internal const double ManagedWidth = 720;

    /// <summary>Largeur minimale du dialogue, en unités logiques WPF.</summary>
    internal const double MinimumWidth = 560;

    /// <summary>Part maximale de la largeur de la zone de travail utilisable par le dialogue.</summary>
    internal const double WorkAreaWidthRatio = 0.95;

    /// <summary>Part maximale de la hauteur de la zone de travail utilisable par le dialogue.</summary>
    internal const double WorkAreaHeightRatio = 0.85;
    /// <summary>Largeur minimale d’un curseur de réglage, en unités logiques WPF.</summary>
    internal const double SliderMinimumWidth = 120;

    /// <summary>Largeur maximale d’un curseur de réglage, en unités logiques WPF.</summary>
    internal const double SliderMaximumWidth = 240;

    /// <summary>Largeur réservée à la valeur d'un curseur discret.</summary>
    internal const double PresetValueWidth = 64;

    /// <summary>Nombre maximal de colonnes de groupes de réglages.</summary>
    internal const int MaximumColumns = 2;
    /// <summary>Largeur de la fenêtre de lecteur de disquette.</summary>
    internal const double FloppyWidth = 760;
    /// <summary>Largeur de la fenêtre de disque dur.</summary>
    internal const double HardDiskWidth = 900;
    /// <summary>Hauteur initiale de la fenêtre de disque dur.</summary>
    internal const double HardDiskHeight = 600;
    /// <summary>Hauteur des curseurs verticaux de réglage.</summary>
    internal const double VerticalSliderHeight = 72;
    /// <summary>Hauteur commune des libellés verticaux, permettant deux lignes sans décaler les curseurs.</summary>
    internal const double VerticalLabelHeight = 36;

    /// <summary>Largeur d'un réglage vertical avec son libellé.</summary>
    internal const double VerticalFieldWidth = 90;
    /// <summary>Largeur de la colonne des réglages HDD.</summary>
    internal const double BehaviorColumnWidth = 350;
    /// <summary>Part de la zone de travail disponible pour les réglages du lecteur.</summary>
    internal const double BodyHeightRatio = 0.65;
}
