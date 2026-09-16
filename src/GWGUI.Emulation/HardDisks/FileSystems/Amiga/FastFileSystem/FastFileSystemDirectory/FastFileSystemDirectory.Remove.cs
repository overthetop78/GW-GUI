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
        public static async Task RemoveEntry(FastFileSystemVolumeState vol, uint pSect, string name, bool ignoreProtectionBits)
        {
            name = TrimName(vol, name);
            var parent = await FastFileSystemDisk.ReadEntryBlock(vol, pSect);

            var result = await GetEntryBlock(vol, parent.HashTable, name, false);
            var nSect = result.NSect;
            var entryBlock = result.EntryBlock;
            var nSect2 = result.NUpdSect ?? 0;
            if (nSect == uint.MaxValue)
            {
                throw new PathNotFoundException($"Path '{name}' not found");
            }

            if (!ignoreProtectionBits && FastFileSystemMacro.hasD(result.EntryBlock.Access))
            {
                throw new FileSystemException($"File '{name}' does not have delete protection bits set");
            }

            /* if it is a directory, is it empty ? */
            if (entryBlock.SecType == FastFileSystemConstants.ST_DIR && !IsEmpty(entryBlock))
            {
                throw new DirectoryNotEmptyException($"Directory '{name}' is not empty");
            }

            /* in parent hashTable */
            if (nSect2 == 0)
            {
                var intl = vol.UseIntl || vol.UseDirCache;
                var hashVal = GetHashValue(entryBlock.HashTable.Length, name, intl);
                parent.HashTable[hashVal] = entryBlock.NextSameHash;
                await WriteEntryBlock(vol, pSect, parent);
            }
            /* in linked list */
            else
            {
                var previous = await FastFileSystemDisk.ReadEntryBlock(vol, nSect2);
                previous.NextSameHash = entryBlock.NextSameHash;
                await WriteEntryBlock(vol, nSect2, previous);
            }

            if (entryBlock.SecType != FastFileSystemConstants.ST_FILE &&
                entryBlock.SecType != FastFileSystemConstants.ST_DIR &&
                entryBlock.SecType != FastFileSystemConstants.ST_LFILE &&
                entryBlock.SecType != FastFileSystemConstants.ST_LDIR)
            {
                throw new FileSystemException($"Invalid entry secType {entryBlock.SecType}");
            }

            switch (entryBlock.SecType)
            {
                case FastFileSystemConstants.ST_FILE:
                    var fileHeaderBlock = FastFileSystemEntryBlockParser.Parse(entryBlock.BlockBytes, vol.UseLnfs);
                    await FastFileSystemFile.AdfFreeFileBlocks(vol, fileHeaderBlock);
                    FastFileSystemBitmap.AdfSetBlockFree(vol, nSect); //marks the entry block as free in BitmapBlock
                    if (vol.UseLnfs && fileHeaderBlock.CommentBlock != 0)
                    {
                        FastFileSystemBitmap.AdfSetBlockFree(vol,
                            fileHeaderBlock.CommentBlock); //marks the comment block as free in BitmapBlock
                    }
                    break;
                case FastFileSystemConstants.ST_DIR:
                    FastFileSystemBitmap.AdfSetBlockFree(vol, nSect);
                    /* free dir cache block : the directory must be empty, so there's only one cache block */
                    if (vol.UseDirCache)
                    {
                        FastFileSystemBitmap.AdfSetBlockFree(vol, entryBlock.Extension);
                    }
                    break;
                case FastFileSystemConstants.ST_LFILE:
                case FastFileSystemConstants.ST_LDIR:
                    var realEntryBlock = await FastFileSystemDisk.ReadEntryBlock(vol, entryBlock.RealEntry);

                    // if entry block is the first link, then update real entry block next link to entry block next link
                    if (entryBlock.HeaderKey == realEntryBlock.NextLink)
                    {
                        realEntryBlock.NextLink = entryBlock.NextLink;
                        await FastFileSystemDisk.WriteEntryBlock(vol, entryBlock.RealEntry, realEntryBlock);
                    }

                    // iterate through chain of links to find first link entry with next link equal to entry block to delete
                    var nextLink = realEntryBlock.NextLink;
                    FastFileSystemEntryBlock firstLinkEntryBlock = null;
                    while (nextLink != 0 && nextLink != entryBlock.HeaderKey)
                    {
                        firstLinkEntryBlock = await FastFileSystemDisk.ReadEntryBlock(vol, nextLink);

                        nextLink = firstLinkEntryBlock.NextLink;

                    }

                    // update first link entry block next link to next link entry block, so entry block is removed from chain of links
                    if (firstLinkEntryBlock != null && nextLink != 0)
                    {
                        var nextLinkEntryBlock = await FastFileSystemDisk.ReadEntryBlock(vol, nextLink);

                        firstLinkEntryBlock.NextLink = nextLinkEntryBlock.NextLink;
                        await FastFileSystemDisk.WriteEntryBlock(vol, firstLinkEntryBlock.HeaderKey, firstLinkEntryBlock);
                    }

                    FastFileSystemBitmap.AdfSetBlockFree(vol, nSect); //marks the entry block as free in BitmapBlock
                    break;
            }

            if (vol.UseDirCache)
            {
                await FastFileSystemCache.DeleteFromCache(vol, parent, entryBlock.HeaderKey);
            }

            await FastFileSystemBitmap.AdfUpdateBitmap(vol);
        }

        public static bool IsEmpty(FastFileSystemEntryBlock dirBlock)
        {
            for (var i = 0; i < dirBlock.HashTable.Length; i++)
                if (dirBlock.HashTable[i] != 0)
                    return false;

            return true;
        }
    }
}
