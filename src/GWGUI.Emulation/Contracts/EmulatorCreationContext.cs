namespace GWGUI.Emulation.Contracts;

/// <summary>Services de création d’une machine, indépendants du nom et de l’auteur de son module.</summary>
/// <typeparam name="TConfiguration">Type concret de configuration conservé par le module.</typeparam>
/// <param name="SessionsDirectory">Dossier des sessions temporaires possédées par les machines créées.</param>
/// <param name="CorePath">Chemin du cœur sélectionné par l’adaptateur.</param>
/// <param name="HostExecutablePath">Exécutable de l’hôte, utilisé par les cœurs exécutés dans un processus séparé.</param>
/// <param name="AudioOutputFactory">Fabrique facultative : le contrôleur qui crée une sortie en assure la libération.</param>
/// <param name="SaveDirectoryResolver">Résolution différée du dossier de sauvegarde à partir de la configuration.</param>
public sealed record EmulatorCreationContext<TConfiguration>(
    string SessionsDirectory,
    string CorePath,
    string HostExecutablePath,
    Func<IAudioOutput?>? AudioOutputFactory,
    Func<TConfiguration, string>? SaveDirectoryResolver)
{
    /// <summary>Dossier des firmwares lorsque le backend prépare des fichiers partagés pour sa session.</summary>
    /// <remarks>Les backends qui n’utilisent pas ce dossier conservent le constructeur à cinq arguments.</remarks>
    public string? FirmwareDirectory { get; init; }

    /// <summary>Crée un contexte avec le dossier de firmware fourni explicitement par le module.</summary>
    /// <remarks>Ce constructeur conserve le contrat de préparation des sessions qui exige ce sixième argument.</remarks>
    public EmulatorCreationContext(string SessionsDirectory, string CorePath, string HostExecutablePath,
        Func<IAudioOutput?>? AudioOutputFactory, Func<TConfiguration, string>? SaveDirectoryResolver,
        string FirmwareDirectory)
        : this(SessionsDirectory, CorePath, HostExecutablePath, AudioOutputFactory, SaveDirectoryResolver)
    {
        this.FirmwareDirectory = FirmwareDirectory;
    }
}
