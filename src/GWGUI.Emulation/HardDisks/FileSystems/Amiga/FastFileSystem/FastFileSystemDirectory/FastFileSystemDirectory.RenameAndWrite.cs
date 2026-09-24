namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Blocks;
    using Exceptions;
    using Extensions;

    public static partial class FastFileSystemDirectory
    {
        public static async Task RenameEntry(FastFileSystemVolumeState vol, uint pSect, string oldName, uint nPSect, string newName)
        {
            oldName = TrimName(vol, oldName);
            newName = TrimName(vol, newName);

            // return, if name and sector are the same
            if (oldName == newName && pSect == nPSect)
            {
                return;
            }

            var intl = vol.UseIntl || vol.UseDirCache;
            var len = newName.Length;
            // myToUpper((uint8_t*)name2, (uint8_t*)newName, len, intl);
            // myToUpper((uint8_t*)name3, (uint8_t*)oldName, strlen(oldName), intl);
            var name2 = MyToUpper(newName, intl);
            var name3 = MyToUpper(oldName, intl);
            /* newName == oldName ? */

            var parent = await FastFileSystemDisk.ReadEntryBlock(vol, pSect);

            var hashValueO = GetHashValue(parent.HashTable.Length, oldName, intl);

            var result = await GetEntryBlock(vol, parent.HashTable, oldName, false);
            var nSect = result.NSect;
            var parentEntryBlock = result.EntryBlock;
            var prevSect = result.NUpdSect ?? 0;
            if (nSect == uint.MaxValue)
            {
                throw new PathNotFoundException($"Path '{oldName}' not found");
            }

            /* change name and parent dir */
            parentEntryBlock.Name = newName;
            parentEntryBlock.Parent = nPSect;
            var tmpSect = parentEntryBlock.NextSameHash;

            parentEntryBlock.NextSameHash = 0;
            await WriteEntryBlock(vol, nSect, parentEntryBlock);

            /* del from the oldname list */

            /* in hashTable */
            if (prevSect == 0)
            {
                parent.HashTable[hashValueO] = tmpSect;
                await WriteEntryBlock(vol, pSect, parent);
            }
            else
            {
                /* in linked list */
                var previous = await FastFileSystemDisk.ReadEntryBlock(vol, prevSect);
                /* entry.nextSameHash (tmpSect) could be == 0 */
                previous.NextSameHash = tmpSect;
                await WriteEntryBlock(vol, prevSect, previous);
            }

            var nParent = await FastFileSystemDisk.ReadEntryBlock(vol, nPSect);

            var hashValueN = GetHashValue(nParent.HashTable.Length, newName, intl);
            var nSect2 = nParent.HashTable[hashValueN];
            /* no list */
            if (nSect2 == 0)
            {
                nParent.HashTable[hashValueN] = nSect;
                await WriteEntryBlock(vol, nPSect, nParent);
            }
            else
            {
                /* a list exists : addition at the end */
                FastFileSystemEntryBlock previous;
                do
                {
                    previous = await FastFileSystemDisk.ReadEntryBlock(vol, nSect2);

                    if (previous.Name.Length == len)
                    {
                        name3 = MyToUpper(previous.Name, intl);
                        if (name3 == name2)
                        {
                            throw new PathAlreadyExistsException($"Path '{previous.Name}' already exists");
                        }
                    }

                    nSect2 = previous.NextSameHash;
                } while (nSect2 != 0);

                if (!(previous.SecType == FastFileSystemConstants.ST_DIR || previous.SecType == FastFileSystemConstants.ST_FILE))
                {
                    throw new FileSystemException($"Invalid entry secType {previous.SecType}");
                }

                previous.NextSameHash = nSect;
                await WriteEntryBlock(vol, previous.HeaderKey, previous);
            }

            if (vol.UseDirCache)
            {
                if (pSect != nPSect)
                {
                    await FastFileSystemCache.UpdateCache(vol, parent, parentEntryBlock, true);
                }
                else
                {
                    await FastFileSystemCache.DeleteFromCache(vol, parent, parentEntryBlock.HeaderKey);
                    await FastFileSystemCache.AddInCache(vol, nParent, parentEntryBlock);
                }
            }
        }

        public static async Task WriteEntryBlock(FastFileSystemVolumeState vol, uint nSect, FastFileSystemEntryBlock ent)
        {
            if (vol.UseLnfs)
            {
                // check if comment fits in lnfs dir block,
                // if yes: if comment block is present, move comment to entry block and free comment block
                // if no: create comment block, move comment, allocate block and write block

                var nameAndCommendSpaceLeft = FastFileSystemConstants.LNFSNAMECMMTLEN - ent.Name.Length + 1;
                var useCommentBlock = nameAndCommendSpaceLeft < ent.Comment.Length + 1;

                if (useCommentBlock)
                {
                    // get free block for comment block, if use comment block and no block is allocated
                    if (ent.CommentBlock == 0)
                    {
                        ent.CommentBlock = FastFileSystemBitmap.AdfGet1FreeBlock(vol);
                    }

                    // create comment block
                    var commentBlock = new FastFileSystemLongNameFileSystemCommentBlock
                    {
                        OwnKey = ent.CommentBlock,
                        HeaderKey = nSect,
                        Comment = ent.Comment
                    };

                    // remove comment from entry block
                    ent.Comment = string.Empty;

                    // write comment block to disk
                    var commentBlockBytes = FastFileSystemLongNameFileSystemCommentBlockWriter.Build(commentBlock, vol.FileSystemBlockSize);
                    await FastFileSystemDisk.WriteBlock(vol, ent.CommentBlock, commentBlockBytes);
                }
                else
                {
                    // free comment block, if not using comment block and block is allocated
                    if (ent.CommentBlock != 0)
                    {
                        FastFileSystemBitmap.AdfSetBlockFree(vol, ent.CommentBlock);
                        await FastFileSystemBitmap.AdfUpdateBitmap(vol);
                    }
                }
            }

            var blockBytes = FastFileSystemEntryBlockBuilder.Build(ent, vol.FileSystemBlockSize, vol.UseLnfs);
            await FastFileSystemDisk.WriteBlock(vol, nSect, blockBytes);
        }

    }
}
