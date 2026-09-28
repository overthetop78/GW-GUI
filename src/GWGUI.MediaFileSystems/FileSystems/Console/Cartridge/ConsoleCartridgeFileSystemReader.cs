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
    private const int MaximumBankCount = 4096;
    private const string BankCountKey = "bankCount";
    private const string BankSizeKey = "bankSize";
    private const string BankPrefix = "bank.";
    private const string LengthSuffix = ".length";
    private const string NameSuffix = ".name";
    private const string BankEntryType = "cartridge-bank";
    private const string BankAttribute = "bank";
    private const string BankNumberMetadata = "bankNumber";
    private const string AddressMetadata = "address";
    private const string BanksAttribute = "cartridge-banks";
    private static readonly IReadOnlySet<string> SupportedFormats = new HashSet<string>(
        [MediaImageFormatIds.NintendoNes, MediaImageFormatIds.NintendoSnes,
         MediaImageFormatIds.NintendoN64, MediaImageFormatIds.NintendoGameBoy,
         MediaImageFormatIds.NintendoGameBoyColor, MediaImageFormatIds.NintendoGameBoyAdvance,
         MediaImageFormatIds.NintendoNds, MediaImageFormatIds.Nintendo3Ds,
         MediaImageFormatIds.NintendoVirtualBoy, MediaImageFormatIds.SegaSg1000,
         MediaImageFormatIds.SegaMasterSystem, MediaImageFormatIds.SegaMegaDrive,
         MediaImageFormatIds.SegaGameGear, MediaImageFormatIds.SegaThirtyTwoX,
         MediaImageFormatIds.NecPcEngine, MediaImageFormatIds.Atari2600,
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
        if (!TryReadInt(document.Metadata, BankCountKey, out var bankCount)
            || bankCount <= 0 || bankCount > MaximumBankCount)
            throw new InvalidDataException("The console cartridge bank count is missing or invalid.");
        if (!TryReadInt(document.Metadata, BankSizeKey, out var bankSize) || bankSize <= 0)
            throw new InvalidDataException("The console cartridge bank size is missing or invalid.");

        var entries = new List<FileSystemEntry>(bankCount);
        for (var bank = 0; bank < bankCount; bank++)
        {
            if (!TryReadInt(document.Metadata, $"{BankPrefix}{bank}{LengthSuffix}", out var length)
                || length <= 0 || length > bankSize)
                throw new InvalidDataException($"Console cartridge bank {bank} has an invalid length.");
            var data = new byte[length];
            blocks.ReadExactlyAsync(bank * (long)bankSize, data).AsTask().GetAwaiter().GetResult();
            var name = document.Metadata.TryGetValue($"{BankPrefix}{bank}{NameSuffix}", out var storedName)
                ? storedName : $"bank{bank:D2}";
            entries.Add(new FileSystemEntry(
                name, FileSystemEntryKind.File, length, null, string.Empty, 0,
                checked(bank * bankSize), true, [], data,
                nativeTypeId: BankEntryType, occupiedSize: length, attributes: [BankAttribute],
                dataValid: true, syntheticName: !document.Metadata.ContainsKey($"{BankPrefix}{bank}{NameSuffix}"),
                metadata: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [BankNumberMetadata] = bank.ToString(CultureInfo.InvariantCulture),
                    [AddressMetadata] = (bank * (long)bankSize).ToString(CultureInfo.InvariantCulture)
                }));
        }

        return new FileSystemVolume(
            volume.Name ?? string.Empty, Id, blocks.Capacity, 0, null, null, entries,
            document.Diagnostics, freeSpaceKnown: false,
            attributes: [document.FormatId, BanksAttribute],
            fileSystemDisplayName: volume.Name ?? document.FormatId);
    }

    private static bool TryReadInt(IReadOnlyDictionary<string, string> metadata,
        string key, out int value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
