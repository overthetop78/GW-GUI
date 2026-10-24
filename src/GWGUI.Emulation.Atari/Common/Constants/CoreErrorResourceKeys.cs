namespace GWGUI.Emulation.Atari.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Clé de la ressource CoreNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotFound = "Emulation.Atari.Error.CoreNotFound";
    /// <summary>Clé de la ressource CoreRejected ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreRejected = "Emulation.Atari.Error.CoreRejected";
    /// <summary>Clé de la ressource Details ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string Details = "Emulation.Atari.Error.Details";
    /// <summary>Clé de la ressource HostExecutableMissing ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostExecutableMissing = "Emulation.Atari.Error.HostExecutableMissing";
    /// <summary>Clé de la ressource HostProtocolFailure ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostProtocolFailure = "Emulation.Atari.Error.HostProtocolFailure";
    /// <summary>Clé du diagnostic StateIncompatible dans le catalogue du module.</summary>
    internal const string StateIncompatible = "Emulation.Atari.Error.StateIncompatible";
    /// <summary>Clé du diagnostic Unexpected dans le catalogue du module.</summary>
    internal const string Unexpected = "Emulation.Atari.Error.Unexpected";
    /// <summary>Clé du diagnostic FirmwareMissing dans le catalogue du module.</summary>
    internal const string FirmwareMissing = "Emulation.Atari.Error.FirmwareMissing";
    /// <summary>Clé du diagnostic FirmwareInvalid dans le catalogue du module.</summary>
    internal const string FirmwareInvalid = "Emulation.Atari.Error.FirmwareInvalid";
    /// <summary>Clé du diagnostic ContentNotFound dans le catalogue du module.</summary>
    internal const string ContentNotFound = "Emulation.Atari.Error.ContentNotFound";
    /// <summary>Clé du diagnostic ContentUnsupported dans le catalogue du module.</summary>
    internal const string ContentUnsupported = "Emulation.Atari.Error.ContentUnsupported";
    /// <summary>Clé du diagnostic OptionInvalid dans le catalogue du module.</summary>
    internal const string OptionInvalid = "Emulation.Atari.Error.OptionInvalid";
    /// <summary>Clé du diagnostic StateInvalid dans le catalogue du module.</summary>
    internal const string StateInvalid = "Emulation.Atari.Error.StateInvalid";
}
