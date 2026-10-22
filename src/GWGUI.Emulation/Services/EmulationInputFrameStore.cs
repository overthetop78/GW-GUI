namespace GWGUI.Emulation.Services;

/// <summary>Stockage atomique des entrées destiné aux cœurs qui interrogent leur état pendant une frame.</summary>
internal sealed class EmulationInputFrameStore : IEmulationInputFrames
{
    private readonly bool _accumulateRelativePointer;
    private EmulationInputSnapshot _pending = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    /// <summary>Choisit la politique fixe de la session : mouvements accumulés ou dernier instantané complet.</summary>
    internal EmulationInputFrameStore(bool accumulateRelativePointer = true) => _accumulateRelativePointer = accumulateRelativePointer;

    /// <summary>Copie les collections empruntées puis accumule les mouvements ou remplace le dernier état selon la politique de session.</summary>
    public void Update(EmulationInputSnapshot? snapshot)
    {
        var frozen = EmulationInputMappingFunctions.Freeze(snapshot);
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending, Accumulate(current, frozen), current), current));
    }

    /// <summary>Fige les entrées de la frame et consomme les seuls mouvements relatifs lorsque la politique le demande.</summary>
    public void Poll()
    {
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending, ConsumeRelativePointer(current), current), current));
        Interlocked.Exchange(ref _polled, current);
    }



    /// <summary>Snapshot figé au dernier Poll, stable jusqu’au Poll suivant.</summary>
    public EmulationInputSnapshot Polled => Volatile.Read(ref _polled);
    /// <summary>Conserve le dernier état absolu et applique la politique relative choisie à la création.</summary>
    private EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => !_accumulateRelativePointer ? EmulationInputMappingFunctions.Freeze(update) : new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
    {
        DeltaX = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel),
        HorizontalWheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.HorizontalWheel, update.Pointer.HorizontalWheel)
    }, update.Controllers.ToArray());

    /// <summary>Remet les quatre composantes relatives à zéro dans le mode accumulé ; conserve le mode instantané.</summary>
    private EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
        !_accumulateRelativePointer ? snapshot : snapshot with
        {
            Pointer = snapshot.Pointer with
            {
                DeltaX = EmulationHostProtocolConstants.EmptyPointerDelta,
                DeltaY = EmulationHostProtocolConstants.EmptyPointerDelta,
                Wheel = EmulationHostProtocolConstants.EmptyPointerDelta,
                HorizontalWheel = EmulationHostProtocolConstants.EmptyPointerDelta
            }
        };
}
