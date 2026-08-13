using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Containers.Hfe;

/// <summary>Représente une face de piste HFE avec ses cellules et son timing uniforme.</summary>
public sealed record HfeTrack
{
    public HfeTrack(int cylinder, int head, IReadOnlyList<bool> bits, uint bitCellTicks)
    {
        Cylinder = cylinder;
        Head = head;
        Bits = Array.AsReadOnly(bits.ToArray());
        BitCellTicks = bitCellTicks;
        Revolution = FluxRevolutionFactory.Create(Bits, bitCellTicks, checked((uint)(Bits.Count * (long)bitCellTicks)));
    }

    public int Cylinder { get; }
    public int Head { get; }
    public IReadOnlyList<bool> Bits { get; }
    public uint BitCellTicks { get; }
    public FluxRevolution Revolution { get; }
}
