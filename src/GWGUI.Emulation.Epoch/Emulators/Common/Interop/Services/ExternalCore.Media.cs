using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Epoch.Emulators.Common.Interop.Functions;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Text;

namespace GWGUI.Emulation.Epoch.Emulators.Common.Interop.Services;

internal sealed partial class ExternalCore
{
    private IReadOnlySet<string> ContentExtensions => Emulator == Emulator.PD777 ? new HashSet<string>(_info.Extensions.Append(MediaRuntimeConstants.ArchiveExtension), StringComparer.OrdinalIgnoreCase) : _info.Extensions;

    private void PrepareInitialMedia(MachineConfiguration configuration)
    {
        var cartridge = configuration.Media.SingleOrDefault(value => value.IsInserted);
        if (cartridge is null) return;
        var source = ContentFunctions.Validate(cartridge.Path, ContentExtensions);
        var directory = Path.Combine(_sessionDirectory!, CoreDirectoryConstants.ContentDirectoryName, Guid.NewGuid().ToString(ConfigurationStoreConstants.MachineIdentifierFormat));
        Directory.CreateDirectory(directory);
        var extension = Path.GetExtension(source);
        string destination;
        if (configuration.Core == Emulator.PD777
            && (extension.Equals(MediaRuntimeConstants.PdProgramExtension, StringComparison.OrdinalIgnoreCase)
                || extension.Equals(MediaRuntimeConstants.PdPatternExtension, StringComparison.OrdinalIgnoreCase)))
        {
            var program = Path.ChangeExtension(source, MediaRuntimeConstants.PdProgramExtension);
            var pattern = Path.ChangeExtension(source, MediaRuntimeConstants.PdPatternExtension);
            RequireNonempty(program);
            RequireNonempty(pattern);
            using (var stream = File.OpenRead(program)) ValidatePdContent(stream, stream.Length, program: true);
            using (var stream = File.OpenRead(pattern)) ValidatePdContent(stream, stream.Length, program: false);
            destination = CopyContent(program, directory, MediaRuntimeConstants.PdProgramFileName);
            CopyContent(pattern, directory, MediaRuntimeConstants.PdPatternFileName);
        }
        else
        {
            destination = configuration.Core == Emulator.PD777 ? PrepareArchive(source, directory) : CopyContent(source, directory);
        }
        if (configuration.Core == Emulator.EmuSCV
            && extension.Equals(MediaRuntimeConstants.MultipartExtension, StringComparison.OrdinalIgnoreCase))
            for (var part = MediaRuntimeConstants.FirstCompanionPart; part <= MediaRuntimeConstants.LastCompanionPart; part++)
            {
                var companion = Path.ChangeExtension(source, part.ToString(CultureInfo.InvariantCulture));
                if (File.Exists(companion)) CopyContent(companion, directory);
            }
        // EmuSCV owns its .save files in the persistent save directory. Neither core exports save RAM.
        _content = ContentFunctions.Create(destination, _info.NeedsFullPath, ContentExtensions);
    }

    private static string CopyContent(string source, string directory, string? fileName = null)
    {
        var destination = Path.Combine(directory, fileName ?? Path.GetFileName(source));
        if (!string.Equals(Path.GetFullPath(source), Path.GetFullPath(destination), StringComparison.OrdinalIgnoreCase))
            File.Copy(source, destination, overwrite: true);
        return destination;
    }

    private static void RequireNonempty(string path)
    {
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentNotFound, ErrorMessages.ContentFileMissing, new Dictionary<string, string> { [ErrorContextConstants.Path] = path });
    }

    private static string PrepareArchive(string source, string directory)
    {
        using var archive = ZipFile.OpenRead(source);
        foreach (var extension in new[] { MediaRuntimeConstants.PdProgramExtension, MediaRuntimeConstants.PdPatternExtension })
        {
            var entry = archive.Entries.FirstOrDefault(entry => Path.GetExtension(entry.FullName).Equals(extension, StringComparison.OrdinalIgnoreCase) && entry.Length > 0);
            if (entry is null)
                throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
            using var stream = entry.Open();
            var program = extension == MediaRuntimeConstants.PdProgramExtension;
            ValidatePdContent(stream, entry.Length, program);
            // Reopen after validation: compressed streams need not support seeking.
            using var content = entry.Open();
            using var target = File.Create(Path.Combine(directory, program ? MediaRuntimeConstants.PdProgramFileName : MediaRuntimeConstants.PdPatternFileName));
            content.CopyTo(target);
        }
        return Path.Combine(directory, MediaRuntimeConstants.PdProgramFileName);
    }

    private static void ValidatePdContent(Stream stream, long length, bool program)
    {
        if (length == (program ? MediaRuntimeConstants.PdRawProgramBytes : MediaRuntimeConstants.PdRawPatternBytes)) return;
        var magic = Encoding.ASCII.GetBytes(program ? MediaRuntimeConstants.PdProgramMagic : MediaRuntimeConstants.PdPatternMagic);
        var formattedLength = program ? length >= MediaRuntimeConstants.PdProgramHeaderBytes && (length - MediaRuntimeConstants.PdProgramHeaderBytes) % MediaRuntimeConstants.PdProgramRecordBytes == 0 : length == MediaRuntimeConstants.PdFormattedPatternBytes;
        if (!formattedLength)
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
        var header = new byte[magic.Length + (program ? 1 : 0)];
        stream.ReadExactly(header);
        if (!header.AsSpan(0, magic.Length).SequenceEqual(magic)
            || (program && header[^1] != MediaRuntimeConstants.PdProgramVersionZero
                && header[^1] != MediaRuntimeConstants.PdProgramVersionOne))
            throw new EmulationException(ErrorCategory.Content, ErrorCode.ContentUnsupported, ErrorMessages.ContentLoadFailed);
    }

    public void InsertMedia(MediaConfiguration media)
    {
        if (media.Category != MediaCategory.Cartridge || media.Slot != EmulationMediaSlot.Cartridge0)
            throw new ArgumentOutOfRangeException(nameof(media));
        var current = _configuration ?? throw new InvalidOperationException(ErrorMessages.CoreNotInitialized);
        ContentFunctions.Validate(media.Path, ContentExtensions);
        ReloadMedia(current with { Media = [media with { IsInserted = true }] });
    }

    public void EjectMedia(EmulationMediaSlot slot)
    {
        if (slot != EmulationMediaSlot.Cartridge0) throw new ArgumentOutOfRangeException(nameof(slot));
        // Cartridge machines stop after removal; the owner creates a new runtime on insertion.
        Stop();
    }

    private void ReloadMedia(MachineConfiguration next)
    {
        ConfigurationFunctions.Validate(next);
        var session = _sessionDirectory!;
        var saves = _saveDirectory;
        DisposeNativeResources();
        Initialize(next, session, saves);
    }

    public void SelectDisk(int index) => throw new ArgumentOutOfRangeException(nameof(index));
    public DiskStatus GetDiskStatus() => RequireCallbacks().DiskControl.GetStatus();
    public void SaveMediaChanges(EmulationMediaSlot slot)
    {
        if (slot != EmulationMediaSlot.Cartridge0) throw new ArgumentOutOfRangeException(nameof(slot));
        // The native cart-close operation persists SRAM during unload; no separate flush export exists.
    }
    public bool HasUnsavedMediaChanges(EmulationMediaSlot slot) => false;
}
