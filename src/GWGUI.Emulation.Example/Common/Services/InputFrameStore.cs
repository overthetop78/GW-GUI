using GWGUI.Emulation;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Emulation.Example.Common.Services;

/// <summary>Publication des entrées par frame avec choix de la politique de mouvements relatifs.</summary>
/// <remarks>Le mode relatif utilise l’interface du SDK, qui accumule les mouvements entre deux lectures.
/// Le mode instantané conserve la dernière mise à jour complète, comme les adaptateurs à entrées déjà
/// encodées. Les deux modes copient les collections empruntées avant de les publier au cœur.</remarks>
internal sealed class InputFrameStore : IEmulationInputFrames
{
    // Le service du SDK porte le calcul d’accumulation ; Common ne le recopie pas.
    private readonly IEmulationInputFrames _relativeFrames = EmulationSdk.CreateInputFrames();
    // Politique fixe de la session : accumulation physique ou conservation de l’encodage natif.
    private readonly bool _accumulateRelativePointer;
    // Sérialise la réception et la publication du mode instantané ; le service SDK possède son propre verrou.
    private readonly object _gate = new();
    // Dernière mise à jour reçue, dont les collections ont été copiées lors de Update.
    private EmulationInputSnapshot _latest = EmulationInputSnapshot.Empty;
    // Instantané rendu visible au cœur lors du dernier Poll, stable pendant le calcul d’une frame.
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    /// <summary>Choisit entre accumulation physique des mouvements et conservation des valeurs natives.</summary>
    /// <param name="accumulateRelativePointer">True pour accumuler et consommer les mouvements ; false pour conserver le dernier instantané natif.</param>
    internal InputFrameStore(bool accumulateRelativePointer = true) => _accumulateRelativePointer = accumulateRelativePointer;

    /// <summary>Reçoit les entrées physiques ou natives selon la politique choisie.</summary>
    /// <param name="snapshot">Instantané emprunté ; null représente un ensemble d’entrées vide.</param>
    public void Update(EmulationInputSnapshot? snapshot)
    {
        if (_accumulateRelativePointer) _relativeFrames.Update(snapshot);
        else lock (_gate) _latest = EmulationSdk.InputProcessing.Freeze(snapshot);
    }

    /// <summary>Publie les entrées à lire pendant une frame ; seul le mode relatif consomme les déplacements.</summary>
    public void Poll()
    {
        if (_accumulateRelativePointer) _relativeFrames.Poll();
        else lock (_gate) _polled = _latest;
    }

    /// <summary>Dernier instantané publié, valable jusqu’au prochain Poll.</summary>
    public EmulationInputSnapshot Polled
    {
        get
        {
            if (_accumulateRelativePointer) return _relativeFrames.Polled;
            lock (_gate) return _polled;
        }
    }
}
