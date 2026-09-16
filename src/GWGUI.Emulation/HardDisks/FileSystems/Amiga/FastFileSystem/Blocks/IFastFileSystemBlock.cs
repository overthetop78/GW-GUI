namespace Hst.Amiga.FileSystems.FastFileSystem.Blocks
{
    public interface IFastFileSystemBlock
    {
        uint Offset { get; set; }
        byte[] BlockBytes { get; set; }
    }
}