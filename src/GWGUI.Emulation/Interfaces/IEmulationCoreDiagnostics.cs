namespace GWGUI.Emulation.Interfaces;

/// <summary>API commune d’inspection des bibliothèques de cœur, disponible pour tout module indépendant.</summary>
/// <remarks>Le service inspecte les fichiers sans charger ni démarrer le cœur. Il possède et ferme ses flux ;
/// les chemins et fournisseurs de messages restent sous la responsabilité de l’appelant.</remarks>
public interface IEmulationCoreDiagnostics
{
    /// <summary>Vérifie l’architecture annoncée ; évalue le message seulement en cas de désaccord.</summary>
    void RequireArchitecture(string? actual, string expected, Func<string> error);

    /// <summary>Produit le manifeste complet : date, tailles, empreinte, architecture, version et exports.</summary>
    EmulationCoreDiagnosticManifest CreateManifest(string releaseId, string releaseVersion, string downloadUrl,
        long archiveSize, string libraryPath);

    /// <summary>Renvoie le chemin vérifié, ou null si l’installation manque, est incomplète ou invalide.</summary>
    string? FindInstalledLibraryPath(string path, Action<string>? verifyInstallation,
        Func<bool>? isInstallationComplete = null);

    /// <summary>Vérifie une bibliothèque PE x64 avec les messages différés fournis par le module.</summary>
    void VerifyWindowsX64Library(string path, Func<string> notPe, Func<string> invalidPe,
        Func<string> wrongArchitecture);

    /// <summary>Calcule le SHA-256 hexadécimal du fichier et ferme le flux avant de rendre la main.</summary>
    string CalculateSha256(string path);
}
