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
        public static async Task<FastFileSystemEntryBlock> CreateFile(FastFileSystemVolumeState vol, FastFileSystemEntryBlock parent, string name)
        {
            /* -1 : do not use a specific, already allocated sector */
            var nSect = await CreateEntry(vol, parent, name, uint.MaxValue);
            if (nSect == uint.MaxValue)
            {
                throw new DiskFullException("No sector available");
            }

            ThrowIfParentEntryBlockIsInvalid(vol, parent);

            var entryBlock = new FastFileSystemEntryBlock(vol.FileSystemBlockSize)
            {
                HeaderKey = nSect,
                Name = name,
                Parent = parent.SecType == FastFileSystemConstants.ST_ROOT ? vol.RootBlockOffset : parent.HeaderKey,
                Date = DateTime.Now,
                SecType = FastFileSystemConstants.ST_FILE
            };

            await FastFileSystemDisk.WriteEntryBlock(vol, nSect, entryBlock);

            if (vol.UseDirCache)
            {
                await FastFileSystemCache.AddInCache(vol, parent, entryBlock);
            }

            await FastFileSystemBitmap.AdfUpdateBitmap(vol);

            return entryBlock;
        }

        private static void ThrowIfParentEntryBlockIsInvalid(FastFileSystemVolumeState volume, FastFileSystemEntryBlock dir)
        {
            if (!(dir.SecType == FastFileSystemConstants.ST_ROOT || dir.SecType == FastFileSystemConstants.ST_DIR))
            {
                throw new FileSystemException($"Entry block '{dir.GetType().Name}' at sector '{GetSector(volume, dir)}' has invalid secondary type '{dir.SecType}'");
            }
        }

        /// <summary>
        /// Create entry in entry block
        /// </summary>
        /// <param name="vol">FastFileSystemVolumeState</param>
        /// <param name="dir">Entry block to insert entry into it's hashTable</param>
        /// <param name="name">Name of entry</param>
        /// <param name="thisSect">insert this sector pointer into the hashTable (here 'thisSect' must be allocated before in the bitmap). if 'thisSect'==-1, allocate a sector</param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static async Task<uint> CreateEntry(FastFileSystemVolumeState vol, FastFileSystemEntryBlock dir, string name, uint thisSect)
        {
            ThrowIfParentEntryBlockIsInvalid(vol, dir);

            name = TrimName(vol, name);

            var intl = vol.UseIntl || vol.UseDirCache;
            var len = Math.Min(name.Length, FastFileSystemConstants.MAXNAMELEN);
            var name2 = MyToUpper(name, intl);
            var hashValue = GetHashValue(dir.HashTable.Length, name, intl);
            var nSect = dir.HashTable[hashValue];

            if (nSect == 0)
            {
                uint newSect;
                if (thisSect != uint.MaxValue)
                    newSect = thisSect;
                else
                {
                    newSect = FastFileSystemBitmap.AdfGet1FreeBlock(vol);
                    if (newSect == uint.MaxValue)
                    {
                        throw new FileSystemException("No sector available");
                    }
                }

                dir.HashTable[hashValue] = newSect;
                dir.Date = DateTime.Now;
                await WriteEntryBlock(vol, dir.SecType == FastFileSystemConstants.ST_ROOT ? vol.RootBlockOffset : dir.HeaderKey,
                    dir);

                return newSect;
            }

            FastFileSystemEntryBlock updEntry;
            do
            {
                updEntry = await FastFileSystemDisk.ReadEntryBlock(vol, nSect);
                if (updEntry == null)
                {
                    return uint.MaxValue;
                }

                if (updEntry.Name.Length == len)
                {
                    var name3 = MyToUpper(updEntry.Name, intl);
                    if (name3 == name2)
                    {
                        throw new PathAlreadyExistsException($"Path '{updEntry.Name}' already exists");
                    }
                }

                nSect = updEntry.NextSameHash;
            } while (nSect != 0);

            uint newSect2;
            if (thisSect != uint.MaxValue)
                newSect2 = thisSect;
            else
            {
                newSect2 = FastFileSystemBitmap.AdfGet1FreeBlock(vol);
                if (newSect2 == uint.MaxValue)
                {
                    throw new FileSystemException("No sector available");
                }
            }

            if (!(updEntry.SecType == FastFileSystemConstants.ST_DIR || updEntry.SecType == FastFileSystemConstants.ST_FILE))
            {
                throw new FileSystemException($"Invalid secondary type '{updEntry.SecType}'");
            }

            updEntry.NextSameHash = newSect2;
            await WriteEntryBlock(vol, updEntry.HeaderKey, updEntry);

            return newSect2;
        }

        /// <summary>
        /// Create directory
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="parentSector"></param>
        /// <param name="name"></param>
        /// <exception cref="IOException"></exception>
        public static async Task CreateDirectory(FastFileSystemVolumeState vol, uint parentSector, string name)
        {
            var parent = await FastFileSystemDisk.ReadEntryBlock(vol, parentSector);

            /* -1 : do not use a specific, already allocated sector */
            var nSect = await CreateEntry(vol, parent, name, uint.MaxValue);
            if (nSect == uint.MaxValue)
            {
                throw new FileSystemException("No sector available");
            }

            var dirBlock = new FastFileSystemEntryBlock(vol.FileSystemBlockSize)
            {
                HeaderKey = nSect,
                Name = name,
                Date = DateTime.Now,
                Parent = parent.SecType == FastFileSystemConstants.ST_ROOT ? vol.RootBlockOffset : parent.HeaderKey,
                SecType = FastFileSystemConstants.ST_DIR
            };

            if (vol.UseDirCache)
            {
                await FastFileSystemCache.AddInCache(vol, parent, dirBlock);
                await FastFileSystemCache.CreateEmptyCache(vol, dirBlock, uint.MaxValue);
            }

            await WriteEntryBlock(vol, nSect, dirBlock);

            await FastFileSystemBitmap.AdfUpdateBitmap(vol);
        }

        /// <summary>
        /// Resolve link entry.
        /// </summary>
        /// <param name="volume">FastFileSystemVolumeState.</param>
        /// <param name="entry">Link entry.</param>
        /// <returns>Real entry link points to.</returns>
        /// <exception cref="IOException">Exception thrown, if loop detected in links.</exception>
        public static async Task<FastFileSystemEntry> ResolveLinkEntry(FastFileSystemVolumeState volume, FastFileSystemEntry entry)
        {
            if (entry.Type != FastFileSystemConstants.ST_LFILE && entry.Type != FastFileSystemConstants.ST_LDIR)
            {
                return entry;
            }

            var sectorsVisited = new HashSet<uint>(new []{entry.Sector});

            while(entry.Type == FastFileSystemConstants.ST_LFILE || entry.Type == FastFileSystemConstants.ST_LDIR)
            {
                var entryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, entry.Real);
                entry = ConvertEntryBlockToEntry(volume, entryBlock);

                if (!sectorsVisited.Add(entry.Sector))
                {
                    throw new IOException($"Loop detected at sector {entry.Sector} while resolving entry for '{entry.Name}'");
                }
            }

            return entry;
        }

        public static async Task<FastFileSystemEntryBlock> CreateLink(FastFileSystemVolumeState volume, uint parentSector, string linkName, string realName)
        {
            var findEntryResult = await FindEntry(parentSector, realName, volume);
            if (findEntryResult.PartsNotFound.Any())
            {
                throw new PathNotFoundException($"Path '{realName}' not found");
            }

            var entry = findEntryResult.Entries.LastOrDefault();
            if (entry == null)
            {
                throw new PathNotFoundException($"Path '{realName}' not found");
            }

            entry = await ResolveLinkEntry(volume, entry);

            var parentEntryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, parentSector);

            ThrowIfParentEntryBlockIsInvalid(volume, parentEntryBlock);

            /* -1 : do not use a specific, already allocated sector */
            var nSect = await CreateEntry(volume, parentEntryBlock, linkName, uint.MaxValue);
            if (nSect == uint.MaxValue)
            {
                throw new DiskFullException("No sector available");
            }

            var realEntrySector = GetSector(volume, entry.EntryBlock);

            var linkEntryBlock = new FastFileSystemEntryBlock(volume.FileSystemBlockSize)
            {
                HeaderKey = nSect,
                Name = linkName,
                Parent = GetSector(volume, parentEntryBlock),
                RealEntry = realEntrySector,
                NextLink = entry.EntryBlock.NextLink,
                SecType = entry.EntryBlock.SecType == FastFileSystemConstants.ST_FILE ? FastFileSystemConstants.ST_LFILE : FastFileSystemConstants.ST_LDIR,
                Date = DateTime.Now
            };

            await FastFileSystemDisk.WriteEntryBlock(volume, nSect, linkEntryBlock);

            if (volume.UseDirCache)
            {
                await FastFileSystemCache.AddInCache(volume, parentEntryBlock, linkEntryBlock);
            }

            await FastFileSystemBitmap.AdfUpdateBitmap(volume);

            // update real entry next link to new link entry block
            entry.EntryBlock.NextLink = linkEntryBlock.HeaderKey;

            // write real entry block with updated next link to disk
            await FastFileSystemDisk.WriteEntryBlock(volume, realEntrySector, entry.EntryBlock);

            return linkEntryBlock;
        }

        private static uint GetSector(FastFileSystemVolumeState volume, FastFileSystemEntryBlock entryBlock)
        {
            return entryBlock.SecType == FastFileSystemConstants.ST_ROOT ? volume.RootBlockOffset : entryBlock.HeaderKey;
        }

    }
}
