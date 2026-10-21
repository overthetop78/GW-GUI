namespace GWGUI.Emulation.Services;

/// <summary>Conserve les entrées absolues les plus récentes et accumule les mouvements relatifs entre deux frames.</summary>
/// <remarks>Les collections du snapshot sont empruntées et doivent rester stables ; utiliser EmulationInputFrameStore pour copier les collections reçues.</remarks>
public sealed class EmulationInputAccumulator
{
    private readonly object _gate = new();
    private EmulationInputSnapshot _current = EmulationInputSnapshot.Empty;

    /// <summary>Remplace touches et boutons, additionne les déplacements avec saturation ; null équivaut à un état vide.</summary>
    public void Update(EmulationInputSnapshot? snapshot)
    {
        snapshot ??= EmulationInputSnapshot.Empty;
        lock (_gate)
        {
            _current = snapshot with
            {
                Pointer = snapshot.Pointer with
                {
                    DeltaX = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.DeltaX, snapshot.Pointer.DeltaX),
                    DeltaY = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.DeltaY, snapshot.Pointer.DeltaY),
                    Wheel = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.Wheel, snapshot.Pointer.Wheel),
                    HorizontalWheel = EmulationInputMappingFunctions.SaturatingAdd(_current.Pointer.HorizontalWheel, snapshot.Pointer.HorizontalWheel)
                }
            };
        }
    }

    /// <summary>Renvoie les entrées accumulées puis remet les seuls mouvements relatifs à zéro.</summary>
    public EmulationInputSnapshot Consume()
    {
        lock (_gate)
        {
            var result = _current;
            _current = _current with
            {
                Pointer = _current.Pointer with
                {
                    DeltaX = EmulationHostProtocolConstants.EmptyPointerDelta,
                    DeltaY = EmulationHostProtocolConstants.EmptyPointerDelta,
                    Wheel = EmulationHostProtocolConstants.EmptyPointerDelta,
                    HorizontalWheel = EmulationHostProtocolConstants.EmptyPointerDelta
                }
            };
            return result;
        }
    }

    /// <summary>Efface toutes les entrées, notamment lors d’une réinitialisation du cœur.</summary>
    public void Reset()
    {
        lock (_gate) _current = EmulationInputSnapshot.Empty;
    }
}
