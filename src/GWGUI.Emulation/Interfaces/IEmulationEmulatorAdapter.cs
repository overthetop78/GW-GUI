using GWGUI.Emulation.Contracts;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Contrat commun d’un backend, réutilisable par tout module sans déclaration d’assembly ami.</summary>
/// <typeparam name="TConfiguration">Configuration concrète du module ; son schéma reste sous sa responsabilité.</typeparam>
/// <typeparam name="TMachine">Machine concrète implémentant les interfaces publiques du SDK.</typeparam>
public interface IEmulationEmulatorAdapter<TConfiguration, TMachine>
    where TConfiguration : IEmulationConfiguration
    where TMachine : IEmulatedMachine
{
    /// <summary>Identifiant stable utilisé pour sélectionner ce backend dans le module.</summary>
    string EmulatorId { get; }

    /// <summary>Clé conservée par le module pour présenter ou identifier ce backend.</summary>
    string EmulatorKey { get; }

    /// <summary>Description du backend et des machines auxquelles il s’applique.</summary>
    EmulationEmulatorDefinition Definition { get; }

    /// <summary>Reconnaît et traite les arguments d’un cœur exécuté dans un processus séparé.</summary>
    /// <remarks>Retourner false quand les arguments ne concernent pas cet adaptateur.</remarks>
    bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode);

    /// <summary>Décrit l’installation actuellement disponible, sans prendre possession des services de l’hôte.</summary>
    ValueTask<EmulationEmulatorInstallation> GetInstallationAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken);

    /// <summary>Recherche les versions disponibles selon la politique de ce backend.</summary>
    ValueTask<IReadOnlyList<EmulationEmulatorRelease>> FindReleasesAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken);

    /// <summary>Installe la version sélectionnée et transmet la progression à l’appelant.</summary>
    ValueTask<string> InstallAsync(EmulatorManagementContext context,
        EmulationEmulatorRelease release, IProgress<double>? progress, CancellationToken cancellationToken);

    /// <summary>Résout un cœur installé et valide, ou retourne null s’il est absent.</summary>
    ValueTask<string?> FindInstalledCorePathAsync(
        EmulatorManagementContext context, CancellationToken cancellationToken);

    /// <summary>Projette les médias configurés vers les contrats communs sans changer leur ordre.</summary>
    IReadOnlyList<EmulationMedia> ResolveConfiguredMedia(TConfiguration configuration);

    /// <summary>Construit la machine à partir de la configuration et des services fournis.</summary>
    /// <remarks>La création ne transfère pas la propriété des services empruntés ; la machine possède les ressources qu’elle crée.</remarks>
    TMachine Create(TConfiguration configuration, EmulatorCreationContext<TConfiguration> context);
}
