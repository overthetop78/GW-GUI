using System.Globalization;
using System.IO;
using GWGUI.MediaFileSystems.Constants;
using GWGUI.MediaFileSystems.Contracts;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Interfaces;
using GWGUI.MediaFileSystems.Interfaces.Exploration;

namespace GWGUI.MediaFileSystems.FileSystems.Console.Cartridge;

/// <summary>Expose les banques des cartouches console décodées par MediaEngine.</summary>
public sealed class ConsoleCartridgeFileSystemReader : IMediaFileSystemReader
{
    private static readonly IReadOnlySet<string> SupportedFormats = new HashSet<string>(
        [MediaImageFormatIds.NintendoNes, MediaImageFormatIds.NintendoSnes,
         MediaImageFormatIds.NintendoN64, MediaImageFormatIds.NintendoGameBoy,
         MediaImageFormatIds.NintendoGameBoyColor, MediaImageFormatIds.NintendoGameBoyAdvance,
         MediaImageFormatIds.NintendoNds, MediaImageFormatIds.NintendoGameWatch,
         MediaImageFormatIds.Nintendo3Ds,
         MediaImageFormatIds.NintendoVirtualBoy, MediaImageFormatIds.SegaSg1000,
         MediaImageFormatIds.SegaMasterSystem, MediaImageFormatIds.SegaMegaDrive,
         MediaImageFormatIds.SegaGameGear, MediaImageFormatIds.SegaThirtyTwoX,
         MediaImageFormatIds.NecPcEngine, MediaImageFormatIds.NecSuperGrafx,
         MediaImageFormatIds.Atari2600,
         MediaImageFormatIds.Atari5200, MediaImageFormatIds.Atari7800,
         MediaImageFormatIds.BandaiWonderSwan], StringComparer.OrdinalIgnoreCase);

    public string Id => FileSystemIds.ConsoleCartridgeStructure;

    public bool CanRead(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(volume);
        return SupportedFormats.Contains(document.FormatId)
            && document.Representation is IMediaBlockRepresentation blocks
            && volume.Start == 0 && volume.Length == blocks.Capacity;
    }

    public FileSystemVolume Read(IMediaImageDocument document, MediaVolumeDescriptor volume)
    {
        if (!CanRead(document, volume) || document.Representation is not IMediaBlockRepresentation blocks)
            throw new InvalidDataException("The selected console cartridge volume is not readable.");
        if (!TryReadInt(document.Metadata, ConsoleCartridgeMetadataConstants.BankCount, out var bankCount)
            || bankCount <= ConsoleCartridgeMetadataConstants.FirstBank
            || bankCount > ConsoleCartridgeMetadataConstants.MaximumBankCount)
            throw new InvalidDataException("The console cartridge bank count is missing or invalid.");
        if (!TryReadInt(document.Metadata, ConsoleCartridgeMetadataConstants.BankSize, out var bankSize)
            || bankSize <= ConsoleCartridgeMetadataConstants.FirstBank)
            throw new InvalidDataException("The console cartridge bank size is missing or invalid.");

        var entries = new List<FileSystemEntry>(bankCount);
        for (var bank = ConsoleCartridgeMetadataConstants.FirstBank; bank < bankCount; bank++)
        {
            if (!TryReadInt(document.Metadata,
                    $"{ConsoleCartridgeMetadataConstants.BankPrefix}{bank}{ConsoleCartridgeMetadataConstants.LengthSuffix}",
                    out var length)
                || length <= ConsoleCartridgeMetadataConstants.EmptyLength || length > bankSize)
                throw new InvalidDataException($"Console cartridge bank {bank} has an invalid length.");
            var data = new byte[length];
            blocks.ReadExactlyAsync(bank * (long)bankSize, data).AsTask().GetAwaiter().GetResult();
            var name = document.Metadata.TryGetValue(
                    $"{ConsoleCartridgeMetadataConstants.BankPrefix}{bank}{ConsoleCartridgeMetadataConstants.NameSuffix}",
                    out var storedName)
                ? storedName
                : string.Format(CultureInfo.InvariantCulture,
                    ConsoleCartridgeMetadataConstants.DefaultBankNameFormat, bank);
            entries.Add(new FileSystemEntry(
                name, FileSystemEntryKind.File, length, null, string.Empty,
                ConsoleCartridgeMetadataConstants.FirstStorageReference,
                checked(bank * bankSize), true, [], data,
                nativeTypeId: ConsoleCartridgeMetadataConstants.BankEntryType,
                occupiedSize: length, attributes: [ConsoleCartridgeMetadataConstants.BankAttribute],
                dataValid: true, syntheticName: !document.Metadata.ContainsKey(
                    $"{ConsoleCartridgeMetadataConstants.BankPrefix}{bank}{ConsoleCartridgeMetadataConstants.NameSuffix}"),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ConsoleCartridgeMetadataConstants.BankNumberMetadata] = bank.ToString(CultureInfo.InvariantCulture),
                    [ConsoleCartridgeMetadataConstants.AddressMetadata] = (bank * (long)bankSize).ToString(CultureInfo.InvariantCulture)
                }));
        }

        return new FileSystemVolume(
            volume.Name ?? string.Empty, Id, blocks.Capacity,
            ConsoleCartridgeMetadataConstants.FirstAddress, null, null, entries,
            document.Diagnostics, freeSpaceKnown: false,
            attributes: [document.FormatId, ConsoleCartridgeMetadataConstants.BanksAttribute],
            fileSystemDisplayName: volume.Name ?? document.FormatId);
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, string> metadata,
        string key, out int value)
    {
        value = ConsoleCartridgeMetadataConstants.EmptyLength;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
