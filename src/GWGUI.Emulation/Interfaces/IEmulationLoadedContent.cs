namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire du descripteur natif d’un contenu et de ses allocations associées.</summary>
/// <remarks>Le contenu regroupe le chemin natif et, lorsque le cœur demande les données en mémoire,
/// leur copie. Le consommateur possède cet ensemble et appelle Dispose dans son finally après
/// la dernière utilisation du descripteur ; le fichier source reste emprunté et inchangé.</remarks>
public interface IEmulationLoadedContent : IDisposable
{
    /// <summary>Adresse empruntée du descripteur GameInfo du contrat d’interopérabilité ; zéro après Dispose.</summary>
    /// <remarks>Le cœur emprunte le descripteur et ses pointeurs ; il ne les libère pas.</remarks>
    nint GameInfo { get; }
}
