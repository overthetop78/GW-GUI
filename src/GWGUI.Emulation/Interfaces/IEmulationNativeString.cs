namespace GWGUI.Emulation.Interfaces;

/// <summary>Chaîne terminée par zéro, stockée dans une allocation native possédée.</summary>
/// <remarks>Le créateur possède cette ressource jusqu’à son transfert explicite à un propriétaire.
/// Il appelle Dispose dans son finally après le dernier appel natif utilisant la chaîne.
/// L’encodage est UTF-8, sauf demande explicite d’ANSI sous Windows.</remarks>
public interface IEmulationNativeString : IDisposable
{
    /// <summary>Adresse empruntée de la chaîne ; zéro après sa libération.</summary>
    /// <remarks>Le destinataire natif ne libère pas cette adresse et ne la conserve pas au-delà de la vie du propriétaire.</remarks>
    nint Pointer { get; }
}
