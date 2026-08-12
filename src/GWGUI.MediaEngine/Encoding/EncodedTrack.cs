using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Contient les cellules binaires et la révolution de flux produites par un encodeur.</summary>
public sealed record EncodedTrack
{
    /// <summary>Initialise un résultat en copiant les cellules binaires produites.</summary>
    /// <param name="EncoderId">Identifiant technique de l'encodeur utilisé.</param>
    /// <param name="Bits">Cellules binaires produites, dans leur ordre d'émission.</param>
    /// <param name="Revolution">Révolution de flux construite à partir des cellules.</param>
    public EncodedTrack(string EncoderId, IReadOnlyList<bool> Bits, FluxRevolution Revolution)
    {
        ArgumentNullException.ThrowIfNull(EncoderId);
        ArgumentNullException.ThrowIfNull(Bits);
        ArgumentNullException.ThrowIfNull(Revolution);
        this.EncoderId = EncoderId;
        this.Bits = Array.AsReadOnly(Bits.ToArray());
        this.Revolution = Revolution;
    }

    /// <summary>Obtient l'identifiant technique de l'encodeur utilisé.</summary>
    public string EncoderId { get; }
    /// <summary>Obtient une copie non modifiable des cellules binaires produites.</summary>
    public IReadOnlyList<bool> Bits { get; }
    /// <summary>Obtient la révolution de flux construite à partir des cellules.</summary>
    public FluxRevolution Revolution { get; }
}
