namespace GWGUI.Emulation.Interfaces;

/// <summary>Accumulation des entrées puis instantané stable pour une frame.</summary>
public interface IEmulationInputFrames
{
    /// <summary>Copie les collections empruntées et accumule les mouvements relatifs.</summary>
    void Update(EmulationInputSnapshot? snapshot);
    /// <summary>Fige les entrées et consomme leurs mouvements relatifs.</summary>
    void Poll();
    /// <summary>Instantané figé, valable jusqu’au prochain Poll.</summary>
    EmulationInputSnapshot Polled { get; }
}
