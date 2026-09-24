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
        /// <summary>
        /// Get path for entry sector, if current sector is provided and found while traversing entries,
        /// path will be relative to current sector, otherwise path will be absolute.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="entrySector"></param>
        /// <param name="currentSector"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static async Task<string> GetPath(FastFileSystemVolumeState volume, uint entrySector, uint? currentSector = null)
        {
            if (entrySector == currentSector)
            {
                return string.Empty;
            }

            var pathComponents = new LinkedList<string>();
            var isRelative = false;

            var sector = entrySector;
            FastFileSystemEntryBlock entryBlock;
            do
            {
                if (sector == currentSector)
                {
                    isRelative = true;
                    break;
                }

                entryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, sector);

                if (entryBlock == null)
                {
                    throw new IOException($"Entry block not found at sector {sector}");
                }

                sector = entryBlock.Parent;

                if (sector == 0)
                {
                    continue;
                }

                pathComponents.AddFirst(entryBlock.Name);
            } while (!(entryBlock is FastFileSystemRootBlock) && sector > 0);

            return string.Concat(isRelative ? string.Empty : "/", string.Join("/", pathComponents.ToList()));
        }

        public static async Task<IEnumerable<FastFileSystemEntry>> ReadEntries(FastFileSystemVolumeState volume, uint nSect, bool recursive = false)
        {
            if (volume.UseDirCache)
            {
                return await FastFileSystemCache.ReadEntries(volume, nSect, recursive);
            }

            var startEntryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, nSect);

            var hashTable = startEntryBlock.HashTable.ToList();
            var entries = new List<FastFileSystemEntry>();

            for (var i = 0; i < hashTable.Count; i++)
            {
                if (hashTable[i] == 0)
                {
                    continue;
                }

                if (hashTable[i] < volume.FirstBlock || hashTable[i] > volume.LastBlock)
                {
                    continue;
                }

                var entryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, hashTable[i]);

                // convert entry block to entry and add to list
                var entry = ConvertEntryBlockToEntry(volume, entryBlock);

                if (volume.ResolveLinkPaths && entry.Type == FastFileSystemConstants.ST_LFILE || entry.Type == FastFileSystemConstants.ST_LDIR)
                {
                    entry.LinkEntryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, entry.Real);
                    var linkPath = await GetPath(volume, entry.LinkEntryBlock.Parent, entry.Parent);
                    entry.LinkPath = string.Concat(linkPath, string.IsNullOrEmpty(linkPath) ? string.Empty : "/",
                        entry.LinkEntryBlock.Name);
                }

                entry.Sector = hashTable[i];
                entries.Add(entry);

                if (recursive && entry.IsDirectory())
                {
                    entry.SubDir = (await ReadEntries(volume,
                        FastFileSystemHelper.GetSector(volume, entryBlock), true)).ToList();
                }

                //         /* same hashcode linked list */
                //         nextSector = entryBlk.nextSameHash;
                //         while( nextSector!=0 ) {
                var nextSector = entryBlock.NextSameHash;
                while (nextSector != 0)
                {
                    entryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, nextSector);

                    // convert entry block to entry and add to list
                    entry = ConvertEntryBlockToEntry(volume, entryBlock);
                    entry.Sector = nextSector;
                    entries.Add(entry);

                    if (recursive && entry.IsDirectory())
                    {
                        entry.SubDir = (await ReadEntries(volume,
                            FastFileSystemHelper.GetSector(volume, entryBlock), true)).ToList();
                    }

                    nextSector = entryBlock.NextSameHash;
                }
            }

            return entries;
        }

        /// <summary>
        /// Convert entry block to entry
        /// </summary>
        /// <param name="volume">FastFileSystemVolumeState containing the entry.</param>
        /// <param name="entryBlock"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public static FastFileSystemEntry ConvertEntryBlockToEntry(FastFileSystemVolumeState volume, FastFileSystemEntryBlock entryBlock)
        {
            var entry = new FastFileSystemEntry
            {
                Type = entryBlock.SecType,
                Parent = entryBlock.Parent,
                Name = entryBlock.Name,
                Comment = string.Empty,
                Date = entryBlock.Date,
                Access = uint.MaxValue, //-1
                Size = 0,
                Real = 0,
                Sector = GetSector(volume, entryBlock),
                EntryBlock = entryBlock,
            };

            switch (entryBlock.SecType)
            {
                case FastFileSystemConstants.ST_DIR:
                    entry.Access = entryBlock.Access;
                    entry.Comment = entryBlock.Comment;
                    break;
                case FastFileSystemConstants.ST_FILE:
                    entry.Access = entryBlock.Access;
                    entry.Size = entryBlock.ByteSize;
                    entry.Comment = entryBlock.Comment;
                    break;
                case FastFileSystemConstants.ST_LFILE:
                    entry.Real = entryBlock.RealEntry;
                    break;
                case FastFileSystemConstants.ST_LDIR:
                    entry.Real = entryBlock.RealEntry;
                    break;
                case FastFileSystemConstants.ST_LSOFT:
                    break;
            }

            return entry;
        }

        public static char ToUpper(char c)
        {
            return (char)(c >= 'a' && c <= 'z' ? c - ('a' - 'A') : c);
        }

        public static char IntlToUpper(char c)
        {
            return (char)((c >= 'a' && c <= 'z') || (c >= 224 && c <= 254 && c != 247) ? c - ('a' - 'A') : c);
        }

        private static string TrimName(FastFileSystemVolumeState volume, string name)
        {
            var maxNameLength = volume.UseLnfs ? FastFileSystemConstants.LNFSMAXNAMELEN : FastFileSystemConstants.MAXNAMELEN;
            return name.Length > maxNameLength ? name.Substring(0, maxNameLength) : name;
        }

        public static int GetHashValue(int hashTableSize, string name, bool intl)
        {
            var hash = (uint)name.Length;
            foreach (var c in name)
            {
                var upper = intl ? IntlToUpper(c) : ToUpper(c);
                hash = (hash * 13 + upper) & 0x7ff;
            }

            hash %= (uint)hashTableSize;
            return (int)hash;
        }

        public static string MyToUpper(string str, bool intl)
        {
            var nstr = str.ToCharArray();
            for (var i = 0; i < str.Length; i++)
            {
                nstr[i] = intl ? IntlToUpper(str[i]) : ToUpper(str[i]);
            }

            return new string(nstr);
        }

        public static async Task<FastFileSystemNameToEntryBlockResult> GetEntryBlock(FastFileSystemVolumeState volume, uint[] ht, string name,
            bool nUpdSect)
        {
            name = TrimName(volume, name);

            var intl = volume.UseIntl || volume.UseDirCache;
            var hashVal = GetHashValue(ht.Length, name, intl);
            var nameLen = name.Length;
            var upperName = MyToUpper(name, intl);

            var nSect = ht[hashVal];
            if (nSect == 0)
                return new FastFileSystemNameToEntryBlockResult
                {
                    NSect = uint.MaxValue //-1
                };

            FastFileSystemEntryBlock entry;
            var updSect = 0U;
            var found = false;
            do
            {
                entry = await FastFileSystemDisk.ReadEntryBlock(volume, nSect);
                if (entry == null)
                {
                    return new FastFileSystemNameToEntryBlockResult
                    {
                        NSect = uint.MaxValue //-1
                    };
                }

                if (nameLen == entry.Name.Length)
                {
                    var upperName2 = MyToUpper(entry.Name, intl);
                    found = upperName == upperName2;
                }

                if (!found)
                {
                    updSect = nSect;
                    nSect = entry.NextSameHash;
                }
            } while (!found && nSect != 0);

            if (nSect == 0 && !found)
                return new FastFileSystemNameToEntryBlockResult
                {
                    NSect = uint.MaxValue, // -1
                };

            return new FastFileSystemNameToEntryBlockResult
            {
                NSect = nSect,
                EntryBlock = entry,
                NUpdSect = nUpdSect ? new uint?(updSect) : null
            };
        }

    }
}
