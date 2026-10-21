using System.Buffers;
using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;
using GWGUI.Emulation.Example.Common.Constants;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Configure le jeu de buffers vidéo du module et réutilise son propriétaire commun par interface.</summary>
/// <param name="pool">Pool emprunté ; null utilise le pool partagé du service commun.</param>
/// <remarks>Le nombre d’emplacements vient de VideoConstants.BufferCount. Le service SDK conserve
/// les locations, le roulement et les restitutions. Le cœur qui crée ce relais le détruit dans son finally,
/// après la dernière utilisation des images ; le pool fourni n’est jamais détruit par ce relais.</remarks>
internal sealed class VideoBufferSet(ArrayPool<byte>? pool = null) : IEmulationVideoBufferSet
{
    /// <summary>Jeu possédé de tableaux vidéo ; seule son interface traverse la frontière du SDK.</summary>
    private readonly IEmulationVideoBufferSet _buffers = EmulationSdk.CreateVideoBufferSet(VideoConstants.BufferCount, pool);

    /// <summary>Obtient le tableau du prochain emplacement de roulement.</summary>
    /// <param name="length">Longueur minimale nécessaire aux pixels, en octets.</param>
    /// <returns>Tableau emprunté au jeu, de longueur au moins égale à la demande.</returns>
    /// <remarks>Un emplacement suffisamment grand est réutilisé. Sinon, son ancien tableau est rendu
    /// au pool et un remplacement est loué. Le consommateur utilise uniquement la longueur utile de la frame,
    /// copie les pixels s’il doit les conserver et ne rend pas lui-même ce tableau au pool.</remarks>
    public byte[] Rent(int length) => _buffers.Rent(length);

    /// <summary>Copie une image native empruntée dans le prochain emplacement du jeu possédé.</summary>
    /// <param name="data">Pointeur vers les pixels, valide pendant le callback natif.</param>
    /// <param name="height">Nombre de lignes de l’image.</param>
    /// <param name="pitch">Distance en octets entre deux débuts de lignes.</param>
    /// <returns>Tableau emprunté au jeu ; le cœur le restitue uniquement en libérant ce jeu.</returns>
    public byte[] CopyFramePixels(nint data, uint height, nuint pitch) => _buffers.CopyFramePixels(data, height, pitch);

    /// <summary>Restitue au pool les tableaux encore détenus dans tous les emplacements du jeu.</summary>
    /// <remarks>Le service détache chaque tableau avant sa restitution et tente les restitutions restantes
    /// même si l’une échoue. Le propriétaire appelle cette opération après la fin de la production vidéo.</remarks>
    public void Dispose() => _buffers.Dispose();
}
