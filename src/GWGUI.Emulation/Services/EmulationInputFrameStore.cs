namespace GWGUI.Emulation.Services;

/// <summary>Stockage atomique des entrées destiné aux cœurs qui interrogent leur état pendant une frame.</summary>
public class EmulationInputFrameStore
{
    private EmulationInputSnapshot _pending = EmulationInputSnapshot.Empty;
    private EmulationInputSnapshot _polled = EmulationInputSnapshot.Empty;

    /// <summary>Copie les collections empruntées et accumule les mouvements jusqu’au prochain Poll.</summary>
    public void Update(EmulationInputSnapshot? snapshot)
    {
        var frozen = EmulationInputMappingFunctions.Freeze(snapshot);
        EmulationInputSnapshot current;
        do
        {
            current = Volatile.Read(ref _pending);
        } while (!ReferenceEquals(Interlocked.CompareExchange(ref _pending, Accumulate(current, frozen), current), current));
    }

    /// <summary>Fige les entrées de la frame et consomme les seuls mouvements relatifs en attente.</summary>
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
    /// <summary>Point d’adaptation des règles matérielles d’accumulation ; conserve par défaut le dernier état absolu.</summary>
    protected virtual EmulationInputSnapshot Accumulate(EmulationInputSnapshot current, EmulationInputSnapshot update) => new(new HashSet<EmulationKey>(update.Keys), update.Pointer with
    {
        DeltaX = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaX, update.Pointer.DeltaX),
        DeltaY = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.DeltaY, update.Pointer.DeltaY),
        Wheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.Wheel, update.Pointer.Wheel),
        HorizontalWheel = EmulationInputMappingFunctions.SaturatingAdd(current.Pointer.HorizontalWheel, update.Pointer.HorizontalWheel)
    }, update.Controllers.ToArray());

    /// <summary>Point d’adaptation de la consommation ; remet par défaut les quatre composantes relatives à zéro.</summary>
    protected virtual EmulationInputSnapshot ConsumeRelativePointer(EmulationInputSnapshot snapshot) =>
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
