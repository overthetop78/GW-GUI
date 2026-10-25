using System.IO;
using System.IO.MemoryMappedFiles;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire d’un segment vidéo partagé redimensionnable utilisé par le protocole d’hébergement.</summary>
/// <remarks>L’appelant possède le rédacteur et le libère dans le finally de sa session.
/// La vue est empruntée pendant l’écriture ; elle reste possédée par le rédacteur et devient invalide
/// lorsqu’un changement de capacité remplace le segment ou lorsque le rédacteur est libéré.</remarks>
public interface IEmulationSharedVideoWriter : IDisposable
{
    /// <summary>Vue d’écriture empruntée du segment courant, ou null avant allocation et après libération.</summary>
    MemoryMappedViewAccessor? View { get; }
    /// <summary>Nom du segment à transmettre au lecteur distant, ou null sans segment courant.</summary>
    string? Name { get; }
    /// <summary>Capacité de chaque emplacement vidéo, en octets.</summary>
    int SlotCapacity { get; }
    /// <summary>Conserve un segment suffisant ou remplace le segment courant selon la règle de capacité du module.</summary>
    /// <param name="frameLength">Longueur des pixels d’une image, en octets.</param>
    void EnsureCapacity(int frameLength);
}
