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
        /// Set date for entry
        /// </summary>
        /// <param name="volume">FastFileSystemVolumeState</param>
        /// <param name="parentSector"></param>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <exception cref="IOException"></exception>
        public static async Task SetEntryDate(FastFileSystemVolumeState volume, uint parentSector, string name, DateTime date)
        {
            var parent = await FastFileSystemDisk.ReadEntryBlock(volume, parentSector);

            var result = await GetEntryBlock(volume, parent.HashTable, name, false);
            var nSect = result.NSect;
            var entryBlock = result.EntryBlock;
            if (nSect == uint.MaxValue)
            {
                throw new PathNotFoundException($"Path '{name}' not found");
            }

            if (!(entryBlock.SecType == FastFileSystemConstants.ST_DIR || entryBlock.SecType == FastFileSystemConstants.ST_FILE))
            {
                throw new FileSystemException($"Invalid entry secType '{entryBlock.SecType}'");
            }

            entryBlock.Date = date;
            await WriteEntryBlock(volume, nSect, entryBlock);

            if (volume.UseDirCache)
            {
                await FastFileSystemCache.UpdateCache(volume, parent, entryBlock, false);
            }
        }

        /// <summary>
        /// Set access for entry
        /// </summary>
        /// <param name="volume">FastFileSystemVolumeState</param>
        /// <param name="parentSector"></param>
        /// <param name="name"></param>
        /// <param name="access"></param>
        /// <exception cref="IOException"></exception>
        public static async Task SetEntryAccess(FastFileSystemVolumeState volume, uint parentSector, string name, uint access)
        {
            var parent = await FastFileSystemDisk.ReadEntryBlock(volume, parentSector);

            var result = await GetEntryBlock(volume, parent.HashTable, name, false);
            var nSect = result.NSect;
            var entryBlock = result.EntryBlock;
            if (nSect == uint.MaxValue)
            {
                throw new PathNotFoundException($"Path '{name}' not found");
            }

            if (!(entryBlock.SecType == FastFileSystemConstants.ST_DIR || entryBlock.SecType == FastFileSystemConstants.ST_FILE))
            {
                throw new FileSystemException($"Invalid entry secType '{entryBlock.SecType}'");
            }

            entryBlock.Access = access;
            await WriteEntryBlock(volume, nSect, entryBlock);

            if (volume.UseDirCache)
            {
                await FastFileSystemCache.UpdateCache(volume, parent, entryBlock, false);
            }
        }

        /// <summary>
        /// Set entry comment
        /// </summary>
        /// <param name="volume">FastFileSystemVolumeState mounted</param>
        /// <param name="parentSector">Parent sector</param>
        /// <param name="name">Name of entry</param>
        /// <param name="comment">Comment</param>
        /// <exception cref="IOException"></exception>
        public static async Task SetEntryComment(FastFileSystemVolumeState volume, uint parentSector, string name, string comment)
        {
            var parent = await FastFileSystemDisk.ReadEntryBlock(volume, parentSector);

            var result = await GetEntryBlock(volume, parent.HashTable, name, false);
            var nSect = result.NSect;
            var entryBlock = result.EntryBlock;
            if (nSect == uint.MaxValue)
            {
                throw new DiskFullException("No sector available");
            }

            if (!(entryBlock.SecType == FastFileSystemConstants.ST_DIR || entryBlock.SecType == FastFileSystemConstants.ST_FILE))
            {
                throw new FileSystemException($"Invalid entry secType '{entryBlock.SecType}'");
            }

            entryBlock.Comment = comment.Length > FastFileSystemConstants.MAXCMMTLEN
                ? comment.Substring(0, FastFileSystemConstants.MAXCMMTLEN)
                : comment;
            await WriteEntryBlock(volume, nSect, entryBlock);

            if (volume.UseDirCache)
            {
                await FastFileSystemCache.UpdateCache(volume, parent, entryBlock, true);
            }
        }

    }
}
