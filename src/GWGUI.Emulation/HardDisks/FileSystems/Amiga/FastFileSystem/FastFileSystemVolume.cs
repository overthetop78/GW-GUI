using Hst.Amiga.FileSystems.FastFileSystem.Blocks;

namespace Hst.Amiga.FileSystems.FastFileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using RigidDiskBlocks;
    using FileMode = FileMode;

    public class FastFileSystemVolume : IFileSystemVolume
    {
        private readonly FastFileSystemVolumeState volume;
        private uint currentDirectorySector;

        public FastFileSystemVolume(FastFileSystemVolumeState volume, uint currentDirectorySector)
        {
            this.volume = volume;
            this.currentDirectorySector = currentDirectorySector;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }

        public void Dispose()
        {
        }

        public string Name => volume.RootBlock.DiskName;

        public long Size => (long)volume.Blocks * volume.BlockSize;

        public long Free
        {
            get
            {
                var freeBlocks = volume.BitmapTable.Sum(bitmapBlock =>
                    bitmapBlock.Map.Sum(m => FastFileSystemMapBlockHelper.ConvertUInt32ToBlockFreeMap(m).Count(f => f)));

                return (long)freeBlocks * volume.BlockSize;
            }
        }

        public async Task<IEnumerable<FileSystems.Entry>> ListEntries()
        {
            return (await FastFileSystemDirectory.ReadEntries(volume, currentDirectorySector)).Select(FastFileSystemEntryConverter.ToEntry)
                .ToList();
        }

        public async Task<IEnumerable<FastFileSystemEntry>> ListRawEntries()
        {
            return (await FastFileSystemDirectory.ReadEntries(volume, currentDirectorySector)).ToList();
        }

        /// <summary>
        /// Find entry in current directory
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<FileSystems.FindEntryResult> FindEntry(string name)
        {
            if (name.IndexOf("/", StringComparison.Ordinal) >= 0 || name.IndexOf("\\", StringComparison.Ordinal) >= 0)
            {
                throw new ArgumentException("Name contains directory separator", nameof(name));
            }

            var findEntryResult = await FastFileSystemDirectory.FindEntry(currentDirectorySector, name, volume);

            var entry = findEntryResult.Entries.LastOrDefault();
            return new FileSystems.FindEntryResult
            {
                PartsNotFound = findEntryResult.PartsNotFound.ToList(),
                Entry = entry == null ? null : FastFileSystemEntryConverter.ToEntry(entry)
            };
        }

        public async Task ChangeDirectory(string path)
        {
            var isRootPath = path.StartsWith("/");
            if (isRootPath)
            {
                currentDirectorySector = volume.RootBlockOffset;
            }

            var findEntryResult = await FastFileSystemDirectory.FindEntry(currentDirectorySector, path, volume);
            if (findEntryResult.PartsNotFound.Any())
            {
                throw new PathNotFoundException($"Path '{path}' not found");
            }

            // return if path is root path, otherwise find entry result should have at least one entry, which is the last entry in path
            if (!findEntryResult.Entries.Any())
            {
                return;
            }

            var entry = findEntryResult.Entries.LastOrDefault();
            if (entry == null)
            {
                throw new PathNotFoundException($"Path '{path}' not found");
            }

            if (entry.Type != FastFileSystemConstants.ST_DIR &&
                entry.Type != FastFileSystemConstants.ST_LDIR)
            {
                throw new PathNotFoundException($"Path '{path}' is not a directory");
            }

            entry = await FastFileSystemDirectory.ResolveLinkEntry(volume, entry);

            currentDirectorySector = entry.Sector;
        }

        /// <summary>
        /// Create directory in current directory
        /// </summary>
        /// <param name="dirName"></param>
        public async Task CreateDirectory(string dirName)
        {
            await FastFileSystemDirectory.CreateDirectory(volume, currentDirectorySector, dirName);
        }

        /// <summary>
        /// Create file in current directory
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="overwrite"></param>
        /// <param name="ignoreProtectionBits"></param>
        public async Task CreateFile(string fileName, bool overwrite = false, bool ignoreProtectionBits = false)
        {
            using (var _ = await FastFileSystemFile.Open(volume, currentDirectorySector, fileName, FileMode.Write, overwrite,
                       ignoreProtectionBits))
            {
            }
        }

        /// <summary>
        /// Create link in current directory.
        /// </summary>
        /// <param name="linkName">Link name to create.</param>
        /// <param name="name">Name of entry to create link to.</param>
        /// <param name="overwrite">Overwrite link, if it exists.</param>
        /// <param name="ignoreProtectionBits">Ignore protection bits, if link exists.</param>
        public async Task CreateLink(string linkName, string name, bool overwrite = false,
            bool ignoreProtectionBits = false)
        {
            await FastFileSystemDirectory.CreateLink(volume, currentDirectorySector, linkName, name);
        }

        /// <summary>
        /// Open file for reading or writing data. File is created if it doesn't exist.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mode"></param>
        /// <param name="overwrite"></param>
        /// <param name="ignoreProtectionBits"></param>
        /// <returns></returns>
        public async Task<Stream> OpenFile(string fileName, FileMode mode, bool overwrite = false, bool ignoreProtectionBits = false)
        {
            return await FastFileSystemFile.Open(volume, currentDirectorySector, fileName, mode, overwrite, ignoreProtectionBits);
        }

        /// <summary>
        /// Delete file or directory from current directory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ignoreProtectionBits"></param>
        public async Task Delete(string name, bool ignoreProtectionBits = false)
        {
            await FastFileSystemDirectory.RemoveEntry(volume, currentDirectorySector, name, ignoreProtectionBits);
        }

        /// <summary>
        /// Rename or move a file or directory
        /// </summary>
        /// <param name="oldName">Old name</param>
        /// <param name="newName">New name</param>
        /// <exception cref="IOException"></exception>
        public async Task Rename(string oldName, string newName)
        {
            var srcEntryResult = await FastFileSystemDirectory.FindEntry(currentDirectorySector, oldName, volume);

            if (srcEntryResult.PartsNotFound.Any())
            {
                throw new PathNotFoundException($"Path '{oldName}' not found");
            }

            var destEntryResult = await FastFileSystemDirectory.FindEntry(currentDirectorySector, newName, volume);
            var partsNotFound = destEntryResult.PartsNotFound.ToList();

            if (!partsNotFound.Any())
            {
                throw new PathAlreadyExistsException($"Path '{newName}' already exists");
            }

            if (partsNotFound.Count > 1)
            {
                throw new PathNotFoundException($"Path '{partsNotFound[0]}' not found");
            }

            await FastFileSystemDirectory.RenameEntry(volume, currentDirectorySector, srcEntryResult.Name, destEntryResult.Sector,
                destEntryResult.Name);
        }

        /// <summary>
        /// Set comment for file in current directory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="comment"></param>
        public async Task SetComment(string name, string comment)
        {
            await FastFileSystemDirectory.SetEntryComment(volume, currentDirectorySector, name, comment);
        }

        /// <summary>
        /// Set protection bits for file in current directory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="protectionBits"></param>
        public async Task SetProtectionBits(string name, ProtectionBits protectionBits)
        {
            await FastFileSystemDirectory.SetEntryAccess(volume, currentDirectorySector, name,
                ProtectionBitsConverter.ToProtectionValue(protectionBits));
        }

        /// <summary>
        /// Set creation date for file in current directory
        /// </summary>
        /// <param name="name"></param>
        /// <param name="date"></param>
        public async Task SetDate(string name, DateTime date)
        {
            await FastFileSystemDirectory.SetEntryDate(volume, currentDirectorySector, name, date);
        }

        /// <summary>
        /// Flush file system changes
        /// </summary>
        /// <returns></returns>
        public Task Flush()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<string> GetStatus()
        {
            return new List<string>();
        }

        /// <summary>
        /// Mount partition fast file system volume from stream partition block
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="partitionBlock"></param>
        /// <param name="resolveLinkPaths"></param>
        /// <returns></returns>
        public static async Task<FastFileSystemVolume> MountPartition(Stream stream, PartitionBlock partitionBlock,
            bool resolveLinkPaths = true)
        {
            return await Mount(stream, partitionBlock.LowCyl, partitionBlock.HighCyl,
                partitionBlock.Surfaces, partitionBlock.BlocksPerTrack, partitionBlock.Reserved,
                partitionBlock.BlockSize,
                partitionBlock.FileSystemBlockSize,
                resolveLinkPaths);
        }

        /// <summary>
        /// Mount adf fast file system volume from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="resolveLinkPaths"></param>
        /// <returns>Fast file system volume</returns>
        public static async Task<FastFileSystemVolume> MountAdf(Stream stream, bool resolveLinkPaths = true)
        {
            var volume = await FastFileSystemHelper.MountAdf(stream);
            volume.ResolveLinkPaths = resolveLinkPaths;

            return new FastFileSystemVolume(volume, volume.RootBlockOffset);
        }

        /// <summary>
        /// Mount fast file system volume from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lowCyl"></param>
        /// <param name="highCyl"></param>
        /// <param name="surfaces"></param>
        /// <param name="blocksPerTrack"></param>
        /// <param name="reserved"></param>
        /// <param name="blockSize"></param>
        /// <param name="fileSystemBlockSize"></param>
        /// <param name="resolveLinkPaths"></param>
        /// <returns></returns>
        public static async Task<FastFileSystemVolume> Mount(Stream stream, uint lowCyl, uint highCyl,
            uint surfaces, uint blocksPerTrack, uint reserved, uint blockSize, uint fileSystemBlockSize,
            bool resolveLinkPaths = true)
        {
            var volume = await FastFileSystemHelper.Mount(stream, lowCyl, highCyl, surfaces, blocksPerTrack, reserved,
                blockSize,
                fileSystemBlockSize);
            volume.ResolveLinkPaths = resolveLinkPaths;

            return new FastFileSystemVolume(volume, volume.RootBlockOffset);
        }

        /// <summary>
        /// Get the path to the current directory.
        /// </summary>
        /// <returns>Path to current directory.</returns>
        /// <exception cref="IOException"></exception>
        public async Task<string> GetCurrentPath()
        {
            var pathComponents = new LinkedList<string>();

            var directorySector = currentDirectorySector;
            FastFileSystemEntryBlock entryBlock;
            do
            {
                entryBlock = await FastFileSystemDisk.ReadEntryBlock(volume, directorySector);

                if (entryBlock == null)
                {
                    throw new IOException($"Entry block not found at sector {directorySector}");
                }

                directorySector = entryBlock.Parent;

                if (directorySector == 0)
                {
                    continue;
                }

                pathComponents.AddFirst(entryBlock.Name);
            } while (!(entryBlock is FastFileSystemRootBlock) && directorySector > 0);

            return string.Concat("/", string.Join("/", pathComponents.ToList()));
        }

        /// <summary>
        /// Current directory block number.
        /// </summary>
        public uint CurrentDirectoryBlockNumber => currentDirectorySector;
    }
}
