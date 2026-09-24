namespace Hst.Amiga.FileSystems.Pfs3.Doctor
{
    using System.Collections.Generic;

    public class Pfs3DoctorCache
    {
        public LinkedList<Pfs3DoctorCacheLine> LRUqueue;
        public LinkedList<Pfs3DoctorCacheLine> LRUpool;
        public uint linesize; // linesize in blocks
        public uint nolines;
        public Pfs3DoctorCacheLine[] cachelines { get; set; }
    }
}
