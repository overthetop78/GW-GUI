namespace Hst.Amiga.FileSystems.Pfs3.Blocks
{
    using System;

    public interface IPfs3DirEntry
    {
        string Name { get; }
        DateTime CreationDate { get; }
        uint Size { get; }
    }
}