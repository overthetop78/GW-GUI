namespace GWGUI.Emulation;

/// <summary>Accès aux services du SDK par leurs contrats publics, sans connaissance de l’identité des modules.</summary>
public static class EmulationSdk
{
    /// <summary>Service d’inspection sans état partagé mutable et sans bibliothèque native chargée.</summary>
    /// <remarks>L’implémentation reste interne au SDK. Le module utilise le contrat et n’a aucun service à libérer.</remarks>
    public static IEmulationCoreDiagnostics CoreDiagnostics { get; } = new EmulationCoreDiagnosticFunctions();

    /// <summary>Sélection et installation des versions par le contrat commun, sans accès à la classe interne.</summary>
    /// <remarks>Ce service sans état ne possède pas le client HTTP ni les callbacks fournis par le module.</remarks>
    public static IEmulationCoreReleases CoreReleases { get; } = new EmulationCoreReleaseFunctions();
}
