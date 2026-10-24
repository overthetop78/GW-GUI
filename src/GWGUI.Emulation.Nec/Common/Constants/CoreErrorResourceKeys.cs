namespace GWGUI.Emulation.Nec.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Clé de la ressource HostConfigurationInvalid ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostConfigurationInvalid = "Emulation.Error.Core.HostConfigurationInvalid";
    /// <summary>Clé de la ressource HostNotInitialized ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostNotInitialized = "Emulation.Error.Core.HostNotInitialized";
    /// <summary>Clé de la ressource InvalidSaveRamSize ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string InvalidSaveRamSize = "Emulation.Error.Core.InvalidSaveRamSize";
    /// <summary>Clé de la ressource SaveRamTooLarge ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string SaveRamTooLarge = "Emulation.Error.Core.SaveRamTooLarge";
    /// <summary>Clé de la ressource UnknownHostCommand ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string UnknownHostCommand = "Emulation.Error.Core.UnknownHostCommand";
}
