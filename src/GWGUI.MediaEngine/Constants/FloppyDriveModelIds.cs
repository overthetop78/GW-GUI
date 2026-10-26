namespace GWGUI.MediaEngine.Constants;

/// <summary>Identifiants persistants et présentation des profils physiques de lecteurs BBC.</summary>
public static class FloppyDriveModelIds
{
    /// <summary>Identifiant de la famille de lecteurs de disquettes Acorn BBC.</summary>
    public const string AcornBbc = "acorn-bbc";
    /// <summary>Identifiant du lecteur simple face à 40 pistes par face, en simple densité.</summary>
    public const string Single40 = "ss40";

    /// <summary>Identifiant du lecteur double face à 40 pistes par face, en simple densité.</summary>
    public const string Double40 = "ds40";

    /// <summary>Identifiant du lecteur simple face à 80 pistes par face, en simple densité.</summary>
    public const string Single80 = "ss80";

    /// <summary>Identifiant du lecteur double face à 80 pistes par face, en simple densité.</summary>
    public const string Double80 = "ds80";
    /// <summary>Identifiant du lecteur simple face à 40 pistes par face, en double densité.</summary>
    public const string Single40Dd = "ss40-dd";

    /// <summary>Identifiant du lecteur double face à 40 pistes par face, en double densité.</summary>
    public const string Double40Dd = "ds40-dd";

    /// <summary>Identifiant du lecteur simple face à 80 pistes par face, en double densité.</summary>
    public const string Single80Dd = "ss80-dd";

    /// <summary>Identifiant du lecteur double face à 80 pistes par face, en double densité.</summary>
    public const string Double80Dd = "ds80-dd";
    /// <summary>Format du libellé : nombre de faces, densité et capacité en Kio, pour un lecteur de 5,25 pouces.</summary>
    internal const string DisplayFormat = "5.25″ {0} · {1} · {2} KiB";

    /// <summary>Abréviation technique d’un lecteur simple face.</summary>
    internal const string SingleSided = "SS";

    /// <summary>Abréviation technique d’un lecteur double face.</summary>
    internal const string DoubleSided = "DS";

    /// <summary>Abréviation technique de la simple densité.</summary>
    internal const string SingleDensity = "SD";

    /// <summary>Abréviation technique de la double densité.</summary>
    internal const string DoubleDensity = "DD";
}
