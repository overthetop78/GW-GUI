using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Interfaces;

/// <summary>Propriétaire du callback vidéo et de sa dernière image copiée.</summary>
/// <remarks>Le cœur emprunte le délégué et fournit ses pixels pendant le callback. Le propriétaire en copie
/// les octets avant le retour. La session arrête les callbacks avant Dispose dans son finally.</remarks>
public interface IEmulationVideoCallbackHost : IDisposable
{
    /// <summary>Callback copiant une image ou répétant la précédente lorsque le pointeur est nul.</summary>
    ExternalCoreApi.VideoCallback Callback { get; }
    /// <summary>Dernière image publiée, ou null avant réception et après Dispose.</summary>
    VideoFrame? LatestFrame { get; }
    /// <summary>Format des prochains pixels reçus, annoncé par l’environnement du cœur.</summary>
    EmulationPixelFormat PixelFormat { get; set; }
    /// <summary>Rapport largeur/hauteur annoncé par le cœur pour la présentation des prochaines images.</summary>
    float AspectRatio { get; set; }
    /// <summary>Première erreur de format, de taille ou de copie interceptée à la frontière native.</summary>
    Exception? Failure { get; }
}
