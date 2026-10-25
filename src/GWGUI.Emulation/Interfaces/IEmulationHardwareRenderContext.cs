namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire d’un contexte graphique natif utilisé par un cœur d’émulation.</summary>
/// <remarks>La session appelle ces opérations sur son thread graphique et attend l’arrêt du cœur avant Dispose dans finally. Les adresses des callbacks sont empruntées et ne restent valides que jusqu’à cette libération.</remarks>
public interface IEmulationHardwareRenderContext : IDisposable
{
    /// <summary>Adresse du callback fournissant le framebuffer courant au cœur.</summary>
    nint CurrentFramebufferCallback { get; }
    /// <summary>Adresse du callback résolvant les fonctions graphiques demandées par le cœur.</summary>
    nint ProcAddressCallback { get; }
    /// <summary>Type de contexte annoncé par le propriétaire graphique.</summary>
    int ContextType { get; }
    /// <summary>Version majeure du contexte.</summary>
    uint VersionMajor { get; }
    /// <summary>Version mineure du contexte.</summary>
    uint VersionMinor { get; }
    /// <summary>Copie les pixels dans un nouveau tableau possédé par l’appelant si le contexte est disponible.</summary>
    bool TryReadFramebuffer(int width, int height, out byte[] pixels);
    /// <summary>Active le contexte sur le thread appelant avant une commande du cœur.</summary>
    bool MakeCurrentForCore();
    /// <summary>Réutilise le framebuffer possédé ou le remplace pour les dimensions demandées.</summary>
    void EnsureFramebuffer(int width, int height);
}
