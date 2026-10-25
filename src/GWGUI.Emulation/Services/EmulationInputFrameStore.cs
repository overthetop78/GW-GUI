namespace GWGUI.Emulation.Services;

/// <summary>Stockage atomique des entrées destiné aux cœurs qui interrogent leur état pendant une frame.</summary>
internal sealed class EmulationInputFrameStore : IEmulationInputFrames
{
    private EmulationInputSnapshot _pending = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    /// <summary>Copie les collections empruntées puis accumule les quatre mouvements relatifs avec saturation.</summary>
    public void Update(EmulationInputSnapshot? snapshot)
    {
        var frozen = EmulationInputMappingFunctions.Freeze(snapshot);
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending, Accumulate(current, frozen), current), current));
    }

    /// <summary>Fige les entrées de la frame et consomme les quatre mouvements relatifs en conservant les états absolus.</summary>
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
    /// <summary>Conserve le dernier état absolu et additionne les quatre composantes relatives avec saturation.</summary>
    private static EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => update with { Keys = new HashSet<EmulationKey>(update.Keys), Pointer = update.Pointer with
    {
        DeltaX = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel),
        HorizontalWheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.HorizontalWheel, update.Pointer.HorizontalWheel)
    }, Controllers = update.Controllers.ToArray() };

    /// <summary>Remet les quatre composantes relatives à zéro après leur consommation.</summary>
    private static EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
        snapshot with
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
