using System.IO;
using System.IO.Compression;
using GWGUI.Emulation.Snk.Emulators.NeoCd.Constants;

namespace GWGUI.Emulation.Snk.Emulators.NeoCd.Functions;

internal static class FirmwareRuntimeFunctions
{
    private const int FamilyOffset = 0x6C;
    private const int WordBytes = 2;
    private static readonly byte[] ValidityPattern = [0x00, 0x10, 0xF3, 0x00];
    private static readonly byte[] FrontPattern = [0x00, 0xC0, 0xC8, 0x5E];
    private static readonly byte[] TopPattern = [0x00, 0xC0, 0xC2, 0x22];
    private static readonly byte[] CdzPattern = [0x00, 0xC0, 0xA3, 0xE8];
    private enum BiosFamily { Front, Top, Cdz, Unknown }
    private sealed record BiosEntry(string? ArchiveEntry, BiosFamily Family);

    internal static IReadOnlyList<FirmwareCategory> Roles => [FirmwareCategory.SystemBios];
    internal static bool Required(MachineModel model, FirmwareCategory category) => model == MachineModel.NeoGeoCd && category == FirmwareCategory.SystemBios;

    private static BiosFamily? Identify(Stream stream, long length)
    {
        if (length < RuntimeOptionConstants.BiosBytes) return null;
        var bytes = new byte[RuntimeOptionConstants.IdentificationBytes];
        stream.ReadExactly(bytes);
        if (bytes[0] == ValidityPattern[1] && bytes[1] == ValidityPattern[0]) for (var index = 0; index < bytes.Length; index += WordBytes) (bytes[index], bytes[index + 1]) = (bytes[index + 1], bytes[index]);
        var family = bytes.AsSpan(FamilyOffset, FrontPattern.Length);
        if (family.SequenceEqual(FrontPattern)) return BiosFamily.Front;
        if (family.SequenceEqual(TopPattern)) return BiosFamily.Top;
        if (family.SequenceEqual(CdzPattern)) return BiosFamily.Cdz;
        return bytes.AsSpan(0, ValidityPattern.Length).SequenceEqual(ValidityPattern) ? BiosFamily.Unknown : null;
    }

    private static bool Matches(BiosFamily family, MachineModel model) => model switch { MachineModel.NeoGeoCd => family is BiosFamily.Front or BiosFamily.Top, MachineModel.NeoGeoCdz => family is BiosFamily.Cdz or BiosFamily.Unknown, _ => false };
    private static BiosEntry? Find(string path, MachineModel model, CancellationToken cancellationToken)
    {
        if (string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.OpenRead(path);
            foreach (var entry in archive.Entries.OrderBy(item => item.FullName, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsBiosFile(entry.Name)) continue;
                using var stream = entry.Open();
                if (Identify(stream, entry.Length) is BiosFamily family && Matches(family, model)) return new(entry.FullName, family);
            }
            return null;
        }
        if (!IsBiosFile(path)) return null;
        using var file = File.OpenRead(path);
        return Identify(file, file.Length) is BiosFamily selected && Matches(selected, model) ? new(null, selected) : null;
    }

    private static bool IsBiosFile(string path) => Path.GetExtension(path).Equals(".rom", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".bin", StringComparison.OrdinalIgnoreCase);
    internal static ValueTask<EmulationFirmwareCandidate> Inspect(string path, MachineModel model, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolute = Path.GetFullPath(path);
        BiosEntry? selected = null;
        try { selected = Find(absolute, model, cancellationToken); }
        catch (InvalidDataException) { }
        catch (IOException) { }
        var compatibility = selected is null ? EmulationFirmwareCompatibility.Incompatible : selected.Family == BiosFamily.Unknown ? EmulationFirmwareCompatibility.PartiallyCompatible : EmulationFirmwareCompatibility.Compatible;
        return ValueTask.FromResult(new EmulationFirmwareCandidate(absolute, absolute, Path.GetFileName(absolute), null, compatibility, selected is null ? null : FirmwareFunctions.Field(FirmwareCategory.SystemBios)));
    }

    internal static void Prepare(MachineConfiguration configuration, string systemDirectory)
    {
        var firmware = configuration.Firmwares.SingleOrDefault(item => item.Category == FirmwareCategory.SystemBios);
        if (firmware is null)
        {
            if (Required(configuration.Model, FirmwareCategory.SystemBios)) throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareMissing, ErrorMessages.RequiredFirmwareMissing);
            return;
        }
        var entry = Find(firmware.Path, configuration.Model, CancellationToken.None) ?? throw new EmulationException(ErrorCategory.Firmware, ErrorCode.FirmwareInvalid, ErrorMessages.FirmwareFileUnreadable);
        var directory = Path.Combine(systemDirectory, RuntimeOptionConstants.SystemDirectory);
        Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, RuntimeOptionConstants.SelectedFirmware);
        if (entry.ArchiveEntry is null) File.Copy(firmware.Path, target, overwrite: true); else
        {
            using var archive = ZipFile.OpenRead(firmware.Path);
            var member = archive.GetEntry(entry.ArchiveEntry) ?? throw new InvalidDataException(ErrorMessages.FirmwareFileUnreadable);
            using var source = member.Open();
            using var destination = File.Create(target);
            source.CopyTo(destination);
        }
        File.SetAttributes(target, FileAttributes.Normal);
    }
}
