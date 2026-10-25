namespace GWGUI.Emulation.Interfaces;

/// <summary>Identité de l’émulateur réellement sélectionné pour un cœur, indépendante du modèle de machine.</summary>
/// <typeparam name="TEmulator">Identifiant du module : enum ou texte, comme dans son en-tête d’état.</typeparam>
/// <remarks>Le SDK transporte cette valeur sans connaître les émulateurs du module, leur catalogue ni leur propriétaire.
/// Le cœur conserve la valeur choisie par sa fabrique ou sa configuration ; les fonctions de comparaison des états
/// continuent à utiliser le type et les règles de sérialisation du module. La consultation ne crée ni ne transfère
/// de bibliothèque, de processus ou de ressource native.</remarks>
public interface IEmulationCoreIdentity<out TEmulator>
{
    /// <summary>Identifiant du backend sélectionné pour cette instance de cœur.</summary>
    /// <remarks>Une identité configurée à l’initialisation peut être vide avant celle-ci ; elle ne remplace
    /// ni le nom, ni la version, ni l’empreinte du cœur chargé, qui restent fournis par son runtime.</remarks>
    TEmulator Emulator { get; }
}
