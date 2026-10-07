using System.IO;
using System.Globalization;
using System.Text;
using GWGUI.Emulation.Commodore.Emulators.VICE.Common.Constants;

namespace GWGUI.Emulation.Commodore.Emulators.VICE.Common.Factories;

internal abstract class ViceMachineFactory : GWGUI.Emulation.Commodore.Emulators.Common.Interop.Factories.MachineFactory
{
    public override IReadOnlyDictionary<EmulationMediaSlot, bool> MediaActivity(MachineConfiguration configuration, IReadOnlyDictionary<int, bool> ledStates) =>
        new Dictionary<EmulationMediaSlot, bool>
        {
            [EmulationMediaSlot.Floppy0] = ledStates.GetValueOrDefault(ContentConstants.DriveLed),
            [new(EmulationMediaCategory.CassetteDrive, ContentConstants.PrimaryMediaIndex)] = ledStates.GetValueOrDefault(ContentConstants.TapeLed)
        };

    internal abstract string ModelOption { get; }
    internal abstract string ResourceSection { get; }
    internal abstract IReadOnlyDictionary<string, (string Option, string Command)> Models { get; }
    internal abstract IReadOnlyList<(FirmwareSlot Slot, string Resource)> RomDefinitions(MachineConfiguration configuration);
    internal abstract IReadOnlyList<string> CartridgeExtensions { get; }

    public override IReadOnlyList<FirmwareSlot> GetFirmwareSlots(MachineConfiguration configuration) =>
        RomDefinitions(configuration).Select(rom => rom.Slot).ToArray();

    public override IReadOnlyList<EmulationMediaDevice> GetMediaDevices(MachineConfiguration configuration) =>
        Enumerable.Range(ControllerPortConstants.MinimumControllerPort, ContentConstants.DriveCount)
            .Select(index => new EmulationMediaDevice(new(EmulationMediaCategory.FloppyDrive, index),
                EmulationMediaType.Floppy, ContentConstants.DiskExtensions,
                IsPermanent: index == ContentConstants.PrimaryMediaIndex))
            .Concat([new EmulationMediaDevice(new(EmulationMediaCategory.CassetteDrive, ContentConstants.PrimaryMediaIndex),
                EmulationMediaType.Cassette, ContentConstants.CassetteExtensions)])
            .Concat(CartridgeExtensions.Count != BufferConstants.EmptyCollectionCount
                ? [new EmulationMediaDevice(new(EmulationMediaCategory.CartridgeSlot, ContentConstants.PrimaryMediaIndex),
                    EmulationMediaType.Cartridge, CartridgeExtensions, RequiresMachineRecreation: true)] : [])
            .ToArray();

    internal override uint ControllerDevice(IReadOnlyList<IReadOnlyList<ControllerDevice>> ports, int port, ControllerType type) =>
        type == ControllerType.None ? ContentConstants.NoControllerDevice : ContentConstants.JoypadDevice;

    internal override IReadOnlyDictionary<string, string> NativeOptions(MachineConfiguration configuration, IReadOnlyList<MediaConfiguration> media)
    {
        var options = (configuration.Options ?? new Dictionary<string, string>())
            .Where(pair => pair.Key.StartsWith(ContentConstants.OptionPrefix, StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        options[ModelOption] = Models[configuration.Model].Option;
        return options;
    }

    internal override void PrepareFirmware(MachineConfiguration configuration, string systemDirectory)
    {
        foreach (var rom in RomDefinitions(configuration))
        {
            var path = configuration.FirmwarePath(rom.Slot.FieldId);
            if (!string.IsNullOrWhiteSpace(path) && !File.Exists(path))
                throw new FileNotFoundException(rom.Slot.FieldId, path);
        }
    }

    internal override string? PrepareContent(MachineConfiguration configuration, string session, IReadOnlyList<MediaConfiguration> media)
    {
        if (media.Count > GetMediaDevices(configuration).Count)
            throw new ArgumentOutOfRangeException(nameof(media));
        var directory = Path.Combine(session, CoreDirectoryConstants.ContentDirectoryName);
        Directory.CreateDirectory(directory);
        var selected = RomDefinitions(configuration).Where(rom =>
            !string.IsNullOrWhiteSpace(configuration.FirmwarePath(rom.Slot.FieldId))).ToArray();
        if (selected.Length == BufferConstants.EmptyCollectionCount && media.Count <= ContentConstants.SingleMediaCount
            && (media.Count == BufferConstants.EmptyCollectionCount || media[ContentConstants.PrimaryMediaIndex].SlotIndex is null or ContentConstants.PrimaryMediaIndex))
            return media.FirstOrDefault()?.Path is { } path ? Path.GetFullPath(path) : null;
        var romFile = Path.Combine(directory, ContentConstants.RomConfigurationFile);
        var lines = new List<string>
        {
            string.Format(CultureInfo.InvariantCulture, ContentConstants.SectionFormat, ResourceSection)
        };
        foreach (var rom in selected)
        {
            var path = Path.GetFullPath(configuration.FirmwarePath(rom.Slot.FieldId));
            if (path.IndexOfAny(ContentConstants.InvalidPathCharacters) >= BufferConstants.FirstBufferIndex)
                throw new InvalidDataException(rom.Slot.FieldId);
            var line = string.Format(CultureInfo.InvariantCulture, ContentConstants.RomEntryFormat, rom.Resource, path);
            if (Encoding.UTF8.GetByteCount(line) > ContentConstants.MaximumResourceLineBytes)
                throw new InvalidDataException(rom.Slot.FieldId);
            lines.Add(line);
        }
        if (selected.Length > BufferConstants.EmptyCollectionCount)
            File.WriteAllLines(romFile, lines, new UTF8Encoding(false));
        var arguments = new List<string>
        {
            ContentConstants.ModelArgument, Models[configuration.Model].Command
        };
        if (selected.Length > BufferConstants.EmptyCollectionCount)
        {
            arguments.Add(ContentConstants.ConfigurationArgument);
            arguments.Add(Quote(romFile));
        }
        var floppyIndex = ContentConstants.PrimaryMediaIndex;
        foreach (var item in media)
        {
            switch (item.Category)
            {
                case MediaCategory.Floppy:
                    var slot = item.SlotIndex ?? floppyIndex++;
                    if (slot < ContentConstants.PrimaryMediaIndex || slot >= ContentConstants.DriveCount)
                        throw new ArgumentOutOfRangeException(nameof(media));
                    arguments.Add(ContentConstants.DriveArguments[slot]);
                    break;
                case MediaCategory.Cassette:
                    arguments.Add(ContentConstants.TapeArgument);
                    break;
                case MediaCategory.Cartridge:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(media));
            }
            arguments.Add(Quote(Path.GetFullPath(item.Path)));
        }
        var commandLine = string.Join(ContentConstants.ArgumentSeparator, arguments);
        if (Encoding.UTF8.GetByteCount(commandLine) > ContentConstants.MaximumCommandBytes)
            throw new InvalidDataException(nameof(configuration));
        var command = Path.Combine(directory, ContentConstants.CommandFile);
        File.WriteAllText(command, commandLine, new UTF8Encoding(false));
        return command;
    }

    private static string Quote(string path)
    {
        if (path.IndexOfAny(ContentConstants.InvalidPathCharacters) >= BufferConstants.FirstBufferIndex)
            throw new InvalidDataException(nameof(path));
        return string.Format(CultureInfo.InvariantCulture, ContentConstants.QuotedValueFormat, path);
    }
}
