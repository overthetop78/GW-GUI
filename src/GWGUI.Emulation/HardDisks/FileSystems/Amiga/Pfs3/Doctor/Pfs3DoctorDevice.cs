namespace Hst.Amiga.FileSystems.Pfs3.Doctor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public static class Pfs3DoctorDevice
    {
/* get block (of size 'bytes') from cache. Cache is per >device<.
 */
        public static async Task<byte[]> GetBlock(Pfs3DoctorVolume volume, uint bloknr, uint bytes)
        {
            uint lineblnr, offset, total;
            Pfs3DoctorCacheLine cl = null;
            //error_t error = e_none;
            var data = new byte[bytes];

            //__chkabort();
            var dataPtr = 0U;
            for (total = 0; total < bytes; total += volume.blocksize, bloknr++, dataPtr += volume.blocksize)
            {
                lineblnr = (bloknr / volume.cache.linesize) * volume.cache.linesize;
                offset = (bloknr % volume.cache.linesize);
                cl = await GetCacheLine(volume, lineblnr);
                if (cl == null)
                    throw new IOException("e_read_error");

                Array.Copy(cl.data, offset + volume.blocksize, data, dataPtr, volume.blocksize);
                // memcpy(data, cl->data + offset * volume.blocksize, volume.blocksize);
            }
            //return error;
            return data;
        }

/* (private) locale function to search a cacheline
 */
        public static async Task<Pfs3DoctorCacheLine> GetCacheLine(Pfs3DoctorVolume volume, uint bloknr)
        {
            int i;
            Pfs3DoctorCacheLine cl = null;
            // error_t error;
            var cache = volume.cache;

            for (i = 0; i < cache.nolines; i++)
            {
                if (cache.cachelines[i].blocknr == bloknr)
                {
                    cl = cache.cachelines[i];
                    break;
                }
            }

            if (cl == null)
            {
                if (cache.LRUpool.Count == 0)
                {
                    cl = cache.LRUqueue.Last.Value;
                    if (cl.dirty)
                    {
                        await writerawblocks(volume, cl.data, (int)cache.linesize, cl.blocknr);
                    }
                }
                else
                {
                    cl = cache.LRUpool.First.Value;
                }

                await getrawblocks(volume, cl.data, (int)cache.linesize, bloknr);
                cl.blocknr = bloknr;
                cl.dirty = false;
            }

            cache.LRUqueue.Remove(cl);
            cache.LRUpool.Remove(cl);

            cache.LRUqueue.AddFirst(cl);
            return cl;
        }

        public static async Task getrawblocks(Pfs3DoctorVolume volume, byte[] buffer, int blocks, uint blocknr)
        {
            var length = blocks << volume.blockshift;
            var offset = blocknr << volume.blockshift;

            volume.Stream.Position = offset;
            var bytesRead = await volume.Stream.ReadAsync(buffer, 0, length);
            if (bytesRead != length)
            {
                throw new IOException($"Read {bytesRead} bytes, expected {length} at offset {offset}");
            }
        }

        public static async Task writerawblocks(Pfs3DoctorVolume volume, byte[] buffer, int blocks, uint blocknr)
        {
            var length = blocks << volume.blockshift;
            var offset = blocknr << volume.blockshift;

            volume.Stream.Position = offset;
            await volume.Stream.WriteAsync(buffer, 0, length);
        }

/* create cache
 * cacheline size: 4-8-16
 * cachelines: 8-16-32-64-128
 * size = linesize * nolines * blocksize
 */
        public static Pfs3DoctorCache InitCache(Pfs3DoctorVolume volume, uint linesize, uint nolines)
        {
            int i;
            var cache = new Pfs3DoctorCache
            {
                /* allocate memory for the cache */
                cachelines = new Pfs3DoctorCacheLine[nolines],
                linesize = linesize,
                nolines = nolines,
                LRUqueue = new LinkedList<Pfs3DoctorCacheLine>(),
                LRUpool = new LinkedList<Pfs3DoctorCacheLine>()
            };

            for (i = 0; i < nolines; i++)
            {
                cache.cachelines[i] = new Pfs3DoctorCacheLine();
                cache.cachelines[i].blocknr = Pfs3Constants.CL_UNUSED; /* a bloknr that is never used */
                cache.cachelines[i].data = new byte[linesize * volume.blocksize];
                cache.LRUpool.AddFirst(cache.cachelines[i]);
            }

            return cache;
        }
    }
}
