namespace GWGUI.Emulation;

/// <summary>Accès aux services du SDK par leurs contrats publics, sans connaissance de l’identité des modules.</summary>
public static class EmulationSdk
{
    /// <summary>Service d’inspection sans état partagé mutable et sans bibliothèque native chargée.</summary>
    /// <remarks>L’implémentation reste interne au SDK. Le module utilise le contrat et n’a aucun service à libérer.</remarks>
    public static IEmulationCoreDiagnostics CoreDiagnostics { get; } = new EmulationCoreDiagnosticFunctions();
}
