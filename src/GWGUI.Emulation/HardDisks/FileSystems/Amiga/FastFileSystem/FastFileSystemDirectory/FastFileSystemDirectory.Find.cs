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
        public static async Task<FastFileSystemFindEntryResult> FindEntry(uint sector, string path, FastFileSystemVolumeState volume)
        {
            var isRoot = path.StartsWith(FastFileSystemPathConstants.Root);
            var parts = (isRoot ? path.Substring(1) : path)
                .Split(FastFileSystemPathConstants.Separator);

            if (isRoot)
            {
                sector = volume.RootBlockOffset;
            }

            if (parts.Length == 0 || string.IsNullOrEmpty(parts[0]))
            {
                return new FastFileSystemFindEntryResult
                {
                    Name = string.Empty,
                    Sector = sector,
                    PartsNotFound = Array.Empty<string>()
                };
            }

            var entryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, sector);
            sector = FastFileSystemHelper.GetSector(volume, entryBlock);

            int i;
            var entries = new List<FastFileSystemEntry>();
            for (i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                var currentEntry = (await ReadEntries(volume, sector)).FirstOrDefault(x =>
                    x.Name.Equals(part, StringComparison.OrdinalIgnoreCase));
                if (currentEntry == null)
                {
                    break;
                }

                entries.Add(currentEntry);

                if (currentEntry.IsDirectory())
                {
                    sector = FastFileSystemHelper.GetSector(volume, currentEntry.EntryBlock);
                }
            }

            return new FastFileSystemFindEntryResult
            {
                Name = parts.Last(),
                Sector = sector,
                Entries = entries,
                PartsNotFound = parts.Skip(i).ToArray()
            };
        }
    }
}
