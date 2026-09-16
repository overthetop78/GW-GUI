namespace Hst.Amiga.FileSystems.Pfs3.Doctor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;
    using Core.Converters;

    public static class Pfs3DoctorStandardScan
    {
        public static async Task Check(Stream stream)
        {
            var g = new Pfs3GlobalData
            {
                fnsize = 107,
                RootBlock = new Pfs3RootBlock
                {
                    ReservedBlksize = 1024
                }
            };

            var dirBlocks = new List<Pfs3CachedBlock>();
            var anodeBlocks = new List<Pfs3CachedBlock>();

            var buffer = new byte[1024];
            uint? currentBlockNr = null;
            bool endOfStream;
            do
            {
                var blockNr = currentBlockNr;
                var bytesRead = await stream.ReadAsync(buffer, 0, 512);
                endOfStream = bytesRead != 512;

                var blockId = BigEndianConverter.ConvertBytesToUInt16(buffer);
                var rootId = BigEndianConverter.ConvertBytesToInt32(buffer);

                if (rootId == Pfs3Constants.ID_PFS_DISK && !currentBlockNr.HasValue)
                {
                    currentBlockNr = 0;
                }

                switch (blockId)
                {
                    case Pfs3Constants.ABLKID:
                        bytesRead = await stream.ReadAsync(buffer, 512, 512);
                        endOfStream = bytesRead != 512;

                        if (currentBlockNr.HasValue)
                        {
                            currentBlockNr++;
                        }

                        var anodeBlock = Pfs3AnodeBlockReader.Parse(buffer, g);
                        anodeBlocks.Add(new Pfs3CachedBlock
                        {
                            blocknr = blockNr ?? uint.MaxValue,
                            blk = anodeBlock
                        });
                        break;
                    case Pfs3Constants.DBLKID:
                        bytesRead = await stream.ReadAsync(buffer, 512, 512);
                        endOfStream = bytesRead != 512;

                        if (currentBlockNr.HasValue)
                        {
                            currentBlockNr++;
                        }

                        var dirBlock = Pfs3DirBlockReader.Parse(buffer, g);
                        dirBlocks.Add(new Pfs3CachedBlock
                        {
                            blocknr = blockNr ?? uint.MaxValue,
                            blk = dirBlock
                        });
                        break;
                }

                if (currentBlockNr.HasValue)
                {
                    currentBlockNr++;
                }
            } while (!endOfStream);


            foreach (var dirBlock in dirBlocks)
            {
                var blk = dirBlock.dirblock;
                var dirEntries = new List<Pfs3DirEntry>();
                Pfs3DirEntry dirEntry;
                var entries = new byte[blk.BlockBytes.Length - 20]; // offset 20 in dirblock is start of entries
                Array.Copy(blk.BlockBytes, 20, entries, 0, entries.Length);
                var offset = 0;
                do
                {
                    dirEntry = Pfs3DirEntryReader.Read(entries, offset, g);
                    if (dirEntry.Next == 0)
                    {
                        break;
                    }

                    dirEntries.Add(dirEntry);
                    offset += dirEntry.Next;
                } while (offset + dirEntry.Next < entries.Length);

                foreach (var de in dirEntries)
                {
                    // if (de->next & 1)
                    if ((de.Next & 1) != 0)
                    {
                        throw new IOException("Odd directory entry length");
                    }

                    // if (de->nlength + offsetof(struct Pfs3DirEntry, nlength) > de->next)
                    if (de.Name.Length + Pfs3DirEntry.StartOfName > de.Next)
                    {
                        throw new IOException("Invalid filename");
                    }

                    // if (de->nlength > volume.fnsize)
                    if (de.Name.Length > g.fnsize)
                    {
                        throw new IOException("Filename too long");
                    }

                    // if (*FILENOTE(de) + de->nlength + offsetof(struct Pfs3DirEntry, nlength) > de->next)
                    // var fileNoteLength = entries[de.Offset + de.Name.Length + de.startofname];
                    if (Pfs3DirEntry.StartOfName + de.Name.Length + de.comment.Length > de.Next)
                    {
                        throw new IOException("Invalid filenote");
                    }

                    var anodeBlock = anodeBlocks.FirstOrDefault(x => x.blocknr == de.anode);
                    switch (de.type)
                    {
                        case Pfs3Constants.ST_USERDIR:
                            RepairDir(de, dirBlock);
                            break;
                        case Pfs3Constants.ST_FILE:
                            if (anodeBlock == null)
                            {
                                continue;
                            }
                            RepairFile(de, dirBlock, anodeBlock);
                            break;
                    }
                }
            }

        }

        private static void RepairDir(Pfs3DirEntry dirEntry, Pfs3CachedBlock dirblock)
        {

        }

        private static void RepairFile(Pfs3DirEntry dirEntry, Pfs3CachedBlock dirblock, Pfs3CachedBlock anodeblock)
        {
            Pfs3Anode filenode;
            var b = anodeblock.ANodeBlock;


            // /* bitmap gen */
            // for (var anodenr = dirEntry.anode; anodenr > 0; anodenr = filenode.next)
            // {
            //     GetAnode(filenode, anodenr, true);
            //     if ((error = AnodeUsed(anodenr)))
            //         break;
            //
            //     for (bl = filenode.blocknr; bl < filenode.blocknr + filenode.clustersize; bl++)
            //         if ((error = MainBlockUsed(bl)))
            //             break;
            // }
            //
        }

    }
}
