namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    public interface IPfs3SeqBlock : IPfs3Block
    {
        uint seqnr { get; set; }
    }
}
